using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using HybridDSP.Net.HTTP;
using System.Net;
using System.Xml;
using System.Xml.XPath;

namespace SharpGGBotAPI
{
    internal class RequestHandler : IHTTPRequestHandlerFactory, IHTTPRequestHandler
    {
        private GaduGaduBot _owner;

        internal GaduGaduBot Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        public RequestHandler(GaduGaduBot owner)
        {
            _owner = owner;
        }

        public IHTTPRequestHandler CreateRequestHandler(HTTPServerRequest request)
        {
            return this;
        }
        public void HandleRequest(HTTPServerRequest request, HTTPServerResponse response)
        {
            _owner.HandleRequest(request, response);
        }
    }

    public class GaduGaduBot
    {
        #region Properties
        private HTTPServer _server;
        private RequestHandler _factory;
        private string _authorizationFile = string.Empty;
        private string _botUin = string.Empty;
        private System.Net.NetworkCredential _botApiCredentials = new System.Net.NetworkCredential();
        private SynchronizationContext _syncContext = null;
        private bool _tokenNeedUpdate = true;
        private string _currentToken = string.Empty;
        private string _currentTokenServer = string.Empty;
        private string _currentTokenServerPort = string.Empty;
        private Queue<PushRequestItem> _pushRequestsQueue = new Queue<PushRequestItem>();
        private object _pushRequestsQueueLock = new object();

        private const string _botmasterTokenServer = "https://botapi.gadu-gadu.pl/botmaster/getToken/";
        private const string _botmasterSetStatus = "/setStatus/";
        private const string _botmasterSendMessage = "/sendMessage/";
        private const string _ourUserAgent = "SharpGGBotAPI";

        public string AuthorizationFile
        {
            get { return _authorizationFile; }
            set { _authorizationFile = value; }
        }
        public uint BotUin
        {
            get { return uint.Parse(_botUin); }
            set { _botUin = value.ToString(); }
        }
        public string BotApiLogin
        {
            get { return _botApiCredentials.UserName; }
            set { _botApiCredentials.UserName = value; }
        }
        private string BotApiPassword
        {
            get { return _botApiCredentials.Password; }
            set { _botApiCredentials.Password = value; }
        }
        public SynchronizationContext SyncContext
        {
            get { return _syncContext; }
            set { _syncContext = value; }
        }
        public bool IsRunning
        {
            get { return _server.IsRunning; }
        }
        #endregion

        #region Events
        public event EventHandler Started;
        public event EventHandler Stopped;
        public event EventHandler<MessageEventArgs> MessageReceived;
        public event EventHandler<PushRequestErrorEventArgs> PushRequestErrorOccurred;
        #endregion

        #region Constructors
        public GaduGaduBot(string botApiLogin, string botApiPassword, uint botUin)
            : this(botApiLogin, botApiPassword, botUin, string.Empty)
        {
        }
        public GaduGaduBot(string botApiLogin, string botApiPassword, uint botUin, string authorizationFile)
        {
            _botApiCredentials.UserName = botApiLogin;
            _botApiCredentials.Password = botApiPassword;
            BotUin = botUin;
            _authorizationFile = authorizationFile;
        }
        #endregion

        #region Methods
        #region Common
        public void Start(int port)
        {
            if (_server != null && _server.IsRunning) throw new InvalidOperationException("Server was already running!");

            _pushRequestsQueue.Clear();
            _factory = new RequestHandler(this);
            _server = new HTTPServer(_factory, port);
            _server.OnServerStart += _server_OnServerStart;
            _server.OnServerStop += _server_OnServerStop;
            _server.Start();
        }
        public void Stop()
        {
            try
            {
                _server.OnServerStart -= _server_OnServerStart;
                _server.OnServerStop -= _server_OnServerStop;
            }
            catch { }
            if (_server != null && _server.IsRunning) _server.Stop();         
        }

        public void SetStatus(Status status, string description)
        {
            PushRequestItem item = new PushRequestItem();
            item.Uri = _botmasterSetStatus + _botUin;
            bool isDesc = !string.IsNullOrEmpty(description);

            string data = "status=" + Utils.ToInternalStatus(status, isDesc).ToString();
            if (isDesc) data += "&desc=" + WebUtility.UrlEncode(description);

            item.Data = Encoding.UTF8.GetBytes(data);
            EnqueuePushRequest(item);
        }
        public void SendMessage(uint recipient, string message)
        {
            SendMessage(new uint[] { recipient }, message);
        }
        public void SendMessage(uint[] recipients, string message)
        {
            PushRequestItem item = new PushRequestItem();
            item.Uri = _botmasterSendMessage + _botUin;

            StringBuilder builderRecipients = new StringBuilder();
            builderRecipients.Append("to=");
            foreach (uint recipient in recipients)
            {
                builderRecipients.Append(recipient.ToString());
                builderRecipients.Append(',');
            }
            builderRecipients.Remove(builderRecipients.Length - 1, 1); //wywalamy ostatni przecinek heheszki

            string msg = "msg=" + WebUtility.UrlEncode(message);

            item.Data = Encoding.UTF8.GetBytes(builderRecipients.ToString() + '&' + msg);
            EnqueuePushRequest(item);
        }
        public void SendMessage(Dictionary<uint[], string> messageToRecipients)
        {
            PushRequestItem item = new PushRequestItem();
            item.Uri = _botmasterSendMessage + _botUin;

            int id = 1;
            StringBuilder builder = new StringBuilder();
            foreach (var keyValue in messageToRecipients)
            {
                builder.Append(string.Format("to{0}=", id.ToString()));
                foreach (uint recipient in keyValue.Key)
                {
                    builder.Append(recipient.ToString());
                    builder.Append(',');
                }
                builder.Remove(builder.Length - 1, 1); //wywalamy ostatni przecinek
                builder.Append(string.Format("&msg{0}={1}", id.ToString(), WebUtility.UrlEncode(keyValue.Value)));
                builder.Append('&');
                ++id;
            }
            builder.Remove(builder.Length - 1, 1); //wywalamy ostatnie to coś &
            item.Data = Encoding.UTF8.GetBytes(builder.ToString());
            EnqueuePushRequest(item);
        }
        #endregion

        #region PacketProcessors
        protected void RequestToken()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_botmasterTokenServer + _botUin);
            //request.Credentials = _botApiCredentials;
            string auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(_botApiCredentials.UserName + ":" + _botApiCredentials.Password));
            request.Headers.Add("Authorization", "Basic " + auth);
            request.Method = "GET";
            request.UserAgent = _ourUserAgent;
            try
            {
                request.BeginGetResponse(new AsyncCallback(OnRequestTokenCallback), request);
            }
            catch { }
        }
        private void OnRequestTokenCallback(IAsyncResult ar)
        {
            HttpWebResponse response;
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                response = (HttpWebResponse)request.EndGetResponse(ar);

                //MemoryStream allMsg = new MemoryStream();
                //byte[] buffer = new byte[256];
                //int len = 0;
                Stream bodyStream = response.GetResponseStream();
                //while ((len = bodyStream.Read(buffer, 0, buffer.Length)) > 0)
                //{
                //    allMsg.Write(buffer, 0, len);
                //}

                XPathDocument doc = new XPathDocument(bodyStream);
                XPathNavigator docNavigator = doc.CreateNavigator();
                XPathNodeIterator docIterator = docNavigator.Select("/botmaster");
                foreach (XPathNavigator current in docIterator)
                {
                    _currentToken = current.SelectSingleNode("token").Value;
                    _currentTokenServer = current.SelectSingleNode("server").Value;
                    _currentTokenServerPort = current.SelectSingleNode("port").Value;
                }
                bodyStream.Close();
                response.Close();
                _tokenNeedUpdate = false;
            }
            catch (WebException e)
            {
                ProcessErrorResponse(e, "botmaster");
            }
            catch { OnPushRequestErrorOccurred(new PushRequestErrorEventArgs() { Message = "Nieznany błąd", BotmasterErrorCode = -1 }); }

            DequeuePushRequest();
        }
        protected void ProcessErrorResponse(WebException e, string rootElement)
        {
            try
            {
                //WebException we = (WebException)e;
                HttpWebResponse response = (HttpWebResponse)e.Response;

                //MemoryStream allMsg = new MemoryStream();
                //byte[] buffer = new byte[256];
                //int len = 0;
                Stream bodyStream = response.GetResponseStream();
                //while ((len = bodyStream.Read(buffer, 0, buffer.Length)) > 0)
                //{
                //    allMsg.Write(buffer, 0, len);
                //}

                PushRequestErrorEventArgs evArgs = new PushRequestErrorEventArgs();
                evArgs.HttpErrorCode = response.StatusCode;

                XPathDocument doc = new XPathDocument(bodyStream);
                XPathNavigator docNavigator = doc.CreateNavigator();
                XPathNodeIterator docIterator = docNavigator.Select("/" + rootElement);
                foreach (XPathNavigator current in docIterator)
                {
                    try
                    {
                        evArgs.Message = current.SelectSingleNode("errorMsg").Value;
                        evArgs.BotmasterErrorCode = int.Parse(current.SelectSingleNode("status").Value);
                    }
                    catch { }
                }
                bodyStream.Close();
                response.Close();
                OnPushRequestErrorOccurred(evArgs);
            }
            catch
            {
                //throw new Exception();
                OnPushRequestErrorOccurred(new PushRequestErrorEventArgs() { Message = "Nieznany błąd", BotmasterErrorCode = -1 });
            }
        }
        private void EnqueuePushRequest(PushRequestItem item)
        {
            lock (_pushRequestsQueueLock)
            {
                _pushRequestsQueue.Enqueue(item);
                if (_pushRequestsQueue.Count == 1) RequestToken();
            }
        }
        private void DequeuePushRequest()
        {
            if (_pushRequestsQueue.Count <= 0) return;
            if (_tokenNeedUpdate)
            {
                RequestToken();
                return;
            }
            PushRequestItem item;
            lock (_pushRequestsQueueLock) item = _pushRequestsQueue.Dequeue();

            _tokenNeedUpdate = true;

            string uri = _currentTokenServer;
            if (!(uri.StartsWith("https://") || uri.StartsWith("http://"))) uri = uri.Insert(0, "https://");
            //uri += ":" + _currentTokenServerPort;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri + item.Uri);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = item.Data.Length;
            request.UserAgent = _ourUserAgent;
            request.Headers.Add("Token", _currentToken);
            object[] pack = new object[] { request, item };
            request.BeginGetRequestStream(new AsyncCallback(OnPushStreamCallback), pack);
        }
        private void OnPushStreamCallback(IAsyncResult ar)
        {
            bool exc = false;
            try
            {
                object[] pack = (object[])ar.AsyncState;
                HttpWebRequest request = (HttpWebRequest)pack[0];
                PushRequestItem item = (PushRequestItem)pack[1];
                Stream bodyStream = request.EndGetRequestStream(ar);
                bodyStream.Write(item.Data, 0, item.Data.Length);
                bodyStream.Close();
                request.BeginGetResponse(new AsyncCallback(OnPushResponseCallback), request);
            }
            catch (WebException e)
            {
                ProcessErrorResponse(e, "result");
                exc = true;
            }
            catch
            {
                OnPushRequestErrorOccurred(new PushRequestErrorEventArgs() { Message = "Nieznany błąd", BotmasterErrorCode = -1 });
                exc = true;
            }
            if (exc)
            {
                bool requestNewToken = false;
                lock (_pushRequestsQueueLock)
                {
                    if (_pushRequestsQueue.Count > 0) requestNewToken = true;
                }
                if (requestNewToken) RequestToken();
            }
        }
        private void OnPushResponseCallback(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
                response.Close();
            }
            catch (WebException e)
            {
                ProcessErrorResponse(e, "result");
            }
            catch { OnPushRequestErrorOccurred(new PushRequestErrorEventArgs() { Message = "Nieznany błąd", BotmasterErrorCode = -1 }); }
            bool requestNewToken = false;
            lock (_pushRequestsQueueLock)
            {
                if (_pushRequestsQueue.Count > 0) requestNewToken = true;
            }
            if (requestNewToken) RequestToken();
        }
        internal void HandleRequest(HTTPServerRequest request, HTTPServerResponse response)
        {
            #region AuthorizationCheck
            if (!string.IsNullOrEmpty(_authorizationFile))
            {
                string file = request.URI;
                while (file.StartsWith("/")) file = file.Remove(0, 1);
                if (file == _authorizationFile)
                {
                    byte[] resp = null;
                    if (File.Exists(file)) resp = File.ReadAllBytes(file); else resp = new byte[] { 0 };

                    Stream bodyStream = response.Send();
                    bodyStream.Write(resp, 0, resp.Length);
                    response.Write(bodyStream);
                    return;
                }
            }
            #endregion

            #region ReadMessage
            try
            {
                #region ParseQuery
                string queryString = request.URI;
                while (queryString.Length > 0 && (queryString[0] == '/' || queryString[0] == '?')) queryString = queryString.Remove(0, 1);
                string[] nameValues = queryString.Split('&');
                Dictionary<string, string> getData = new Dictionary<string, string>();
                foreach (string nameValue in nameValues)
                {
                    string[] buff = nameValue.Split('=');
                    if (buff.Length >= 2) getData.Add(buff[0], buff[1]);
                }
                #endregion

                MessageEventArgs evArgs = new MessageEventArgs();
                if (getData.ContainsKey("from"))
                {
                    uint uin;
                    uint.TryParse(getData["from"], out uin);
                    evArgs.Uin = uin;
                }

                if (request.ContentLength > 32768) //32 KB
                {
                    response.StatusAndReason = HTTPServerResponse.HTTPStatus.HTTP_BAD_REQUEST;
                    response.Send();
                }
                Stream bodyStream = request.GetRequestStream();
                byte[] data = new byte[request.ContentLength];
                bodyStream.Read(data, 0, (int)request.ContentLength);

                evArgs.Message = Encoding.UTF8.GetString(data);
                ResponseMessage responseMsg = new ResponseMessage();
                responseMsg.HttpResponse = response;
                evArgs.Response = responseMsg;

                OnMessageReceived(evArgs);
            }
            catch { }
            #endregion
        }
        #endregion

        #region Other
        private void _server_OnServerStart()
        {
            OnStarted();
        }
        private void _server_OnServerStop()
        {
            OnStopped();
        }
        #endregion

        #region EventPerformers
        protected void OnStarted()
        {
            RaiseEvent(Started);
        }
        protected void OnStopped()
        {
            RaiseEvent(Stopped);
        }
        protected void OnMessageReceived(MessageEventArgs e)
        {
            RaiseEvent<MessageEventArgs>(MessageReceived, e);
        }
        protected void OnPushRequestErrorOccurred(PushRequestErrorEventArgs e)
        {
            RaiseEvent<PushRequestErrorEventArgs>(PushRequestErrorOccurred, e);
        }

        /// <summary>
        /// Synchronizuje zdarzenia wywoływane na różnych wątkach.
        /// </summary>
        /// <param name="handler">Zdarzenie</param>
        protected void RaiseEvent(EventHandler handler)
        {
            RaiseEvent(handler, EventArgs.Empty);
        }
        /// <summary>
        /// Synchronizuje zdarzenia wywoływane na różnych wątkach.
        /// </summary>
        /// <param name="handler">Zdarzenie</param>
        /// <param name="e">Argumenty</param>
        protected void RaiseEvent(EventHandler handler, EventArgs e)
        {
            if (handler == null) return;
            if (_syncContext != null)
                _syncContext.Post(new SendOrPostCallback((state) => { handler(this, e); }), null);
            else handler(this, e);
        }
        /// <summary>
        /// Synchronizuje zdarzenia wywoływane na różnych wątkach.
        /// </summary>
        /// <typeparam name="T">Typ argumentów</typeparam>
        /// <param name="handler">Zdarzenie</param>
        /// <param name="e">Argumenty</param>
        protected void RaiseEvent<T>(EventHandler<T> handler, T e) where T : EventArgs
        {
            if (handler == null) return;
            if (_syncContext != null)
                _syncContext.Post(new SendOrPostCallback((state) => { handler(this, e); }), null);
            else handler(this, e);
        }
        #endregion
        #endregion
    }
}
