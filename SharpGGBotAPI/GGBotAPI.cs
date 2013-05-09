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
    /// <summary>
    /// Delegat który obsługuje asynchroniczne odpowiedzi na zapytania PUSH.
    /// </summary>
    /// <param name="e">Argumenty - odpowiedź.</param>
    public delegate void PushRequestResultAsync(PushRequestResultAsyncEventArgs e);

    /// <summary>
    /// Klasa która umożliwia obsługę GG Bot API.
    /// </summary>
    public class GaduGaduBot
    {
        private class RequestHandler : IHTTPRequestHandlerFactory, IHTTPRequestHandler
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

        //TODO: full images support
        #region Properties
        private HTTPServer _server;
        private RequestHandler _factory;
        private string _authorizationFile = string.Empty;
        private string _botUin = string.Empty;
        private System.Net.NetworkCredential _botApiCredentials = new System.Net.NetworkCredential();
        private bool _tokenNeedUpdate = true;
        private string _currentToken = string.Empty;
        private string _currentTokenServer = string.Empty;
        private int _currentTokenServerPort = 80;
        private PushRequestResultEventArgs _currentPushResponse = null;
        private Queue<PushRequestItem> _pushRequestsQueue = new Queue<PushRequestItem>();
        private object _pushRequestsQueueLock = new object();

        /// <summary>
        /// Ścieżka do pliku autoryzacyjnego.
        /// </summary>
        public string AuthorizationFile
        {
            get { return _authorizationFile; }
            set { _authorizationFile = value; }
        }
        /// <summary>
        /// Numer GG bota.
        /// Będzie używany do pobierania tokenu dla operacji PUSH oraz do ich wykonywania.
        /// </summary>
        public uint BotUin
        {
            get { return uint.Parse(_botUin); }
            set { _botUin = value.ToString(); }
        }
        /// <summary>
        /// Login do Bot API dla danego numeru GG.
        /// Będzie używany do pobierania tokenu dla operacji PUSH.
        /// </summary>
        public string BotApiLogin
        {
            get { return _botApiCredentials.UserName; }
            set { _botApiCredentials.UserName = value; }
        }
        /// <summary>
        /// Hasło do Bot API dla danego numeru GG.
        /// Będzie używany do pobierania tokenu dla operacji PUSH.
        /// </summary>
        private string BotApiPassword
        {
            get { return _botApiCredentials.Password; }
            set { _botApiCredentials.Password = value; }
        }
        /// <summary>
        /// Czy serwer jest uruchomiony?
        /// </summary>
        public bool IsRunning
        {
            get { return _server.IsRunning; }
        }
        /// <summary>
        /// Filtr IP. Proste zabezpieczenie przeciw nieporządanym zapytaniom.
        /// Jeśli wynosi null, wszystkie zapytania zostaną przyjęte.
        /// Jeśli nie wynosi null, serwer będzie przyjmował tylko te połączenia, które pochądzą z adresów IP zawartych na tej liście.
        /// Standardowe adresy IP botmastera są we właściwości DefaultIPFilter.
        /// </summary>
        public List<IPAddress> IPFilter
        {
            get { return _server.IPFilter; }
            set { _server.IPFilter = value; }
        }

        /// <summary>
        /// Domyślny filtr IP.
        /// Zawiera adresy:
        /// 91.197.15.34
        /// </summary>
        public List<IPAddress> DefaultIPFilter
        {
            get
            {
                List<IPAddress> filter = new List<IPAddress>();
                filter.Add(IPAddress.Parse("91.197.15.34"));
                return filter;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Kiedy serwer wystartuje.
        /// </summary>
        public event EventHandler Started;
        /// <summary>
        /// Kiedy serwer zatrzymano.
        /// </summary>
        public event EventHandler Stopped;
        /// <summary>
        /// Kiedy odebrano wiadomość.
        /// </summary>
        public event EventHandler<MessageEventArgs> MessageReceived;
        /// <summary>
        /// Kiedy wystąpił błąd przy pobieraniu tokena.
        /// </summary>
        public event EventHandler TokenRequestErrorOccurred;
        #endregion

        #region Constructors
        /// <summary>
        /// Skonstruuj GaduGaduBot.
        /// </summary>
        /// <param name="botApiLogin">Login do Bot API</param>
        /// <param name="botApiPassword">Hasło do Bot API</param>
        /// <param name="botUin">Numer GG bota</param>
        public GaduGaduBot(string botApiLogin, string botApiPassword, uint botUin)
            : this(botApiLogin, botApiPassword, botUin, string.Empty)
        {
        }
        /// <summary>
        /// Skonstruuj GaduGaduBot.
        /// </summary>
        /// <param name="botApiLogin">Login do Bot API</param>
        /// <param name="botApiPassword">Hasło do Bot API</param>
        /// <param name="botUin">Numer GG bota</param>
        /// <param name="authorizationFile">Plik autoryzacyjny.</param>
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
        /// <summary>
        /// Wystartuj serwer bota.
        /// </summary>
        /// <param name="port">Port na którym ma działać bot.</param>
        /// <param name="ipFilter">Filtr IP</param>
        public void Start(int port, List<IPAddress> ipFilter = null)
        {
            if (_server != null && _server.IsRunning) throw new InvalidOperationException("Server was already running!");

            _pushRequestsQueue.Clear();
            _factory = new RequestHandler(this);
            _server = new HTTPServer(_factory, port);
            _server.OnServerStart += _server_OnServerStart;
            _server.OnServerStop += _server_OnServerStop;
            _server.IPFilter = ipFilter;
            _server.Start();
        }
        /// <summary>
        /// Zatrzymj serwer bota.
        /// </summary>
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

        #region SetStatus
        #region Sync
        /// <summary>
        /// Ustaw status bota.
        /// </summary>
        /// <param name="status">Status</param>
        /// <param name="description">Opis</param>
        /// <returns>Wynik.</returns>
        public PushRequestResultEventArgs SetStatus(Status status, string description)
        {
            ManualResetEvent locker = new ManualResetEvent(false);
            ProcessSetStatus(status, description, locker, null, null);
            locker.WaitOne();
            locker.Close();
            return _currentPushResponse;
        }
        #endregion
        #region Async
        /// <summary>
        /// Ustaw status bota asynchronicznie.
        /// </summary>
        /// <param name="status">Status</param>
        /// <param name="description">Opis</param>
        /// <param name="callback">Metoda zwrotna.</param>
        /// <param name="userData">Dane użytkownika.</param>
        public void SetStatusAsync(Status status, string description, PushRequestResultAsync callback, object userData = null)
        {
            ProcessSetStatus(status, description, null, callback, userData);
        }
        #endregion
        #endregion

        #region SendMessage
        #region Sync
        /// <summary>
        /// Wyślij wiadomość do wybranego numeru za pomocą zapytania PUSH.
        /// </summary>
        /// <param name="recipient">Numer GG odbiorcy</param>
        /// <param name="message">Wiadomość</param>
        /// <returns>Wynik.</returns>
        public PushRequestResultEventArgs SendMessage(uint recipient, string message)
        {
            return SendMessage(new uint[] { recipient }, message);
        }
        /// <summary>
        /// Wyślij wiadomość do wybranych numerów za pomocą zapytania PUSH.
        /// </summary>
        /// <param name="recipients">Numery GG odbiorców</param>
        /// <param name="message">Wiadomość</param>
        /// <returns>Wynik.</returns>
        public PushRequestResultEventArgs SendMessage(uint[] recipients, string message)
        {
            ManualResetEvent locker = new ManualResetEvent(false);
            ProcessSendMessage(recipients, message, locker, null, null);
            locker.WaitOne();
            locker.Close();
            return _currentPushResponse;
        }
        /// <summary>
        /// Wyślij wybrane wiadomości do wybranych numerów za pomocą zapytania PUSH.
        /// W kluczu słownika muszą być zawarte numery GG odbiorców wiadomości zawartej w wartości słownika. 
        /// </summary>
        /// <param name="messageToRecipients">Słownik</param>
        /// <returns>Wynik.</returns>
        public PushRequestResultEventArgs SendMessage(Dictionary<uint[], string> messageToRecipients)
        {
            ManualResetEvent locker = new ManualResetEvent(false);
            ProcessSendMessage(messageToRecipients, locker, null, null);
            locker.WaitOne();
            locker.Close();
            return _currentPushResponse;
        }
        #endregion
        #region Async
        /// <summary>
        /// Wyślij wiadomość do wybranego numeru za pomocą zapytania PUSH asynchronicznie.
        /// </summary>
        /// <param name="recipient">Numer GG odbiorcy</param>
        /// <param name="message">Wiadomość</param>
        /// <param name="callback">Metoda zwrotna.</param>
        /// <param name="userData">Dane użytkownika.</param>
        public void SendMessageAsync(uint recipient, string message, PushRequestResultAsync callback, object userData = null)
        {
            SendMessageAsync(new uint[] { recipient }, message, callback, userData);
        }
        /// <summary>
        /// Wyślij wiadomość do wybranych numerów za pomocą zapytania PUSH asynchronicznie.
        /// </summary>
        /// <param name="recipients">Numery GG odbiorców</param>
        /// <param name="message">Wiadomość</param>
        /// <param name="callback">Metoda zwrotna.</param>
        /// <param name="userData">Dane użytkownika.</param>
        public void SendMessageAsync(uint[] recipients, string message, PushRequestResultAsync callback, object userData = null)
        {
            ProcessSendMessage(recipients, message, null, callback, userData);
        }
        /// <summary>
        /// Wyślij wybrane wiadomości do wybranych numerów za pomocą zapytania PUSH asynchronicznie.
        /// W kluczu słownika muszą być zawarte numery GG odbiorców wiadomości zawartej w wartości słownika. 
        /// </summary>
        /// <param name="messageToRecipients">Słownik</param>
        /// <param name="callback">Metoda zwrotna.</param>
        /// <param name="userData">Dane użytkownika.</param>
        public void SendMessageAsync(Dictionary<uint[], string> messageToRecipients, PushRequestResultAsync callback, object userData = null)
        {
            ProcessSendMessage(messageToRecipients, null, callback, userData);
        }
        #endregion
        #endregion

        #region SendImage
        #region Sync
        /// <summary>
        /// Wyślij obrazek na serwer.
        /// </summary>
        /// <param name="imageData">Dane obrazka.</param>
        /// <returns>Wynik.</returns>
        public PushRequestResultEventArgs SendImage(byte[] imageData)
        {
            ManualResetEvent locker = new ManualResetEvent(false);
            ProcessSendImage(imageData, locker, null, null);
            locker.WaitOne();
            locker.Close();
            return _currentPushResponse;
        }
        #endregion
        #region Async
        #endregion
        #endregion
        #endregion

        #region PacketProcessors
        private void RequestToken()
        {
            Uri requestUri = new Uri(Container.BOTAPI_BOTMASTER_MAIN_SERVER + Container.BOTAPI_BOTMASTER_RESOURCE_GET_TOKEN + _botUin);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            //CredentialCache cache = new CredentialCache();
            //cache.Add(requestUri, "Basic", _botApiCredentials);
            //request.Credentials = cache;
            //request.PreAuthenticate = true;
            //request.Credentials = _botApiCredentials;
            string auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(_botApiCredentials.UserName + ":" + _botApiCredentials.Password));
            request.Headers.Add("Authorization", "Basic " + auth);
            request.Method = "GET";
            request.UserAgent = Container.BOTAPI_REQUEST_USER_AGENT;
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
                    _currentTokenServerPort = int.Parse(current.SelectSingleNode("port").Value);
                }
                bodyStream.Close();
                response.Close();
                _tokenNeedUpdate = false;
            }
            catch { }
            //catch (WebException e)
            //{
            //    //ProcessErrorResponse(e, "botmaster");
            //}
            //catch { OnTokenRequestErrorOccurred(new PushRequestResultEventArgs() { BotmasterMessage = "Nieznany błąd", BotmasterErrorCode = PushErrorCode.Unknown }); }

            DequeuePushRequest();
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

            //string uri = _currentTokenServer;
            //if (!(uri.StartsWith("https://") || uri.StartsWith("http://"))) uri = uri.Insert(0, "https://");
            //uri += ":" + _currentTokenServerPort;
            UriBuilder uriBuilder = new UriBuilder((item.UseMainServer ? Container.BOTAPI_BOTMASTER_MAIN_SERVER : "http://" + _currentTokenServer) + item.Uri);
            //UriBuilder uriBuilder = new UriBuilder("http://", _currentTokenServer, 80, item.Uri);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = item.Data.Length;
            request.UserAgent = Container.BOTAPI_REQUEST_USER_AGENT;
            request.Headers.Add("Token", _currentToken);
            object[] pack = new object[] { request, item };
            request.BeginGetRequestStream(new AsyncCallback(OnPushStreamCallback), pack);
        }
        private void OnPushStreamCallback(IAsyncResult ar)
        {
            //bool exc = false;
            try
            {
                object[] pack = (object[])ar.AsyncState;
                HttpWebRequest request = (HttpWebRequest)pack[0];
                PushRequestItem item = (PushRequestItem)pack[1];
                Stream bodyStream = request.EndGetRequestStream(ar);
                bodyStream.Write(item.Data, 0, item.Data.Length);
                bodyStream.Close();
                request.BeginGetResponse(new AsyncCallback(OnPushResponseCallback), pack);
            }
            catch { OnPushResponseCallback(null); }
            //catch (WebException e)
            //{
            //    ProcessErrorResponse(e, "result");
            //    exc = true;
            //}
            //catch
            //{
            //    OnTokenRequestErrorOccurred(new PushRequestResultEventArgs() { BotmasterMessage = "Nieznany błąd", BotmasterErrorCode = PushErrorCode.Unknown });
            //    exc = true;
            //}
            //if (exc)
            //{
            //    bool requestNewToken = false;
            //    lock (_pushRequestsQueueLock)
            //    {
            //        if (_pushRequestsQueue.Count > 0) requestNewToken = true;
            //    }
            //    if (requestNewToken) RequestToken();
            //}
        }
        private void OnPushResponseCallback(IAsyncResult ar)
        {
            PushRequestResultEventArgs evArgs = new PushRequestResultEventArgs();
            PushRequestItem item = null;
            try
            {
                object[] pack = (object[])ar.AsyncState;
                HttpWebRequest request = (HttpWebRequest)pack[0];
                item = (PushRequestItem)pack[1];

                evArgs.OperationType = item.OperationType;

                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);

                evArgs.HttpErrorCode = response.StatusCode;
                //TODO: parse PUSH response if required
                Stream bodyStream = response.GetResponseStream();
                switch (item.OperationType)
                {
                    case PushOperation.ImageSend:
                        XPathDocument doc = new XPathDocument(bodyStream);
                        XPathNavigator docNavigator = doc.CreateNavigator();
                        XPathNodeIterator docIterator = docNavigator.Select("/result");
                        foreach (XPathNavigator current in docIterator)
                        {
                            try
                            {
                                evArgs.ImageHash = current.SelectSingleNode("hash").Value;
                                evArgs.BotmasterErrorCode = (PushErrorCode)int.Parse(current.SelectSingleNode("status").Value);
                            }
                            catch { }
                        }
                        break;
                }
                bodyStream.Close();
                response.Close();
            }
            catch (WebException e)
            {
                ProcessErrorResponse(e, "result", evArgs);
            }
            catch { evArgs.BotmasterMessage = "Nieznany błąd"; evArgs.BotmasterErrorCode = PushErrorCode.Unknown; }
            if (item != null)
            {
                if (item.Callback == null) //synchronized
                {
                    _currentPushResponse = evArgs;
                    item.Locker.Set();
                }
                else //async
                {
                    item.Callback(new PushRequestResultAsyncEventArgs(evArgs) { UserData = item.UserData });
                }
            }
            bool requestNewToken = false;
            lock (_pushRequestsQueueLock)
            {
                if (_pushRequestsQueue.Count > 0) requestNewToken = true;
            }
            if (requestNewToken) RequestToken();
        }

        /// <summary>
        /// Obsłuż błąd serwera.
        /// </summary>
        /// <param name="e">Wyjątek.</param>
        /// <param name="rootElement">Główny element dokumentu XML.</param>
        /// <param name="evArgs">Wynik.</param>
        protected void ProcessErrorResponse(WebException e, string rootElement, PushRequestResultEventArgs evArgs)
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

                //PushRequestResultEventArgs evArgs = new PushRequestResultEventArgs();
                evArgs.HttpErrorCode = response.StatusCode;

                XPathDocument doc = new XPathDocument(bodyStream);
                XPathNavigator docNavigator = doc.CreateNavigator();
                XPathNodeIterator docIterator = docNavigator.Select("/" + rootElement);
                foreach (XPathNavigator current in docIterator)
                {
                    try
                    {
                        evArgs.BotmasterMessage = current.SelectSingleNode("errorMsg").Value;
                        evArgs.BotmasterErrorCode = (PushErrorCode)int.Parse(current.SelectSingleNode("status").Value);
                    }
                    catch { }
                }
                bodyStream.Close();
                response.Close();
                //OnTokenRequestErrorOccurred(evArgs);
            }
            catch
            {
                //throw new Exception();
                evArgs.BotmasterMessage = "Nieznany błąd.";
                evArgs.BotmasterErrorCode = PushErrorCode.Unknown;
                //OnTokenRequestErrorOccurred(new PushRequestResultEventArgs() { BotmasterMessage = "Nieznany błąd", BotmasterErrorCode = PushErrorCode.Unknown });
            }
        }

        /// <summary>
        /// Obsłuż zapytanie o zmianę statusu.
        /// </summary>
        /// <param name="status">Status</param>
        /// <param name="description">Opis</param>
        /// <param name="locker">Blokada dla operacji synchronicznej.</param>
        /// <param name="callback">Zwrot dla operacji asynchronicznej.</param>
        /// <param name="userData">Dane użytkownika.</param>
        protected void ProcessSetStatus(Status status, string description, ManualResetEvent locker, PushRequestResultAsync callback, object userData)
        {
            PushRequestItem item = new PushRequestItem();
            item.Uri = Container.BOTAPI_BOTMASTER_RESOURCE_SET_STATUS + _botUin;
            bool isDesc = !string.IsNullOrEmpty(description);

            string data = "status=" + Utils.ToInternalStatus(status, isDesc).ToString();
            if (isDesc) data += "&desc=" + /*WebUtility.UrlEncode(description);*/ System.Web.HttpUtility.UrlEncode(description);

            item.Data = Encoding.UTF8.GetBytes(data);
            item.OperationType = PushOperation.SetStatus;
            item.Locker = locker;
            item.Callback = callback;
            item.UserData = userData;

            EnqueuePushRequest(item);
        }
        /// <summary>
        /// Obsłuż zapytanie o wysłanie wiadomości.
        /// </summary>
        /// <param name="recipients">Odbiorcy.</param>
        /// <param name="message">Wiadomość</param>
        /// <param name="locker">Blokada dla operacji synchronicznej.</param>
        /// <param name="callback">Zwrot dla operacji asynchronicznej.</param>
        /// <param name="userData">Dane użytkownika.</param>
        protected void ProcessSendMessage(uint[] recipients, string message, ManualResetEvent locker, PushRequestResultAsync callback, object userData)
        {
            PushRequestItem item = new PushRequestItem();
            item.Uri = Container.BOTAPI_BOTMASTER_RESOURCE_SEND_MESSAGE + _botUin;

            StringBuilder builderRecipients = new StringBuilder();
            builderRecipients.Append("to=");
            foreach (uint recipient in recipients)
            {
                builderRecipients.Append(recipient.ToString());
                builderRecipients.Append(',');
            }
            builderRecipients.Remove(builderRecipients.Length - 1, 1); //wywalamy ostatni przecinek

            string msg = "msg=" + /*WebUtility.UrlEncode(message);*/ System.Web.HttpUtility.UrlEncode(message);

            item.Data = Encoding.UTF8.GetBytes(builderRecipients.ToString() + '&' + msg);
            item.OperationType = PushOperation.SendMessage;
            item.Locker = locker;
            item.Callback = callback;
            item.UserData = userData;

            EnqueuePushRequest(item);
        }
        /// <summary>
        /// Obsłuż zapytanie o wysłanie wiadomości.
        /// </summary>
        /// <param name="messageToRecipients">Słownik</param>
        /// <param name="locker">Blokada dla operacji synchronicznej.</param>
        /// <param name="callback">Zwrot dla operacji asynchronicznej.</param>
        /// <param name="userData">Dane użytkownika.</param>
        protected void ProcessSendMessage(Dictionary<uint[], string> messageToRecipients, ManualResetEvent locker, PushRequestResultAsync callback, object userData)
        {
            PushRequestItem item = new PushRequestItem();
            item.Uri = Container.BOTAPI_BOTMASTER_RESOURCE_SEND_MESSAGE + _botUin;

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
                builder.Append(string.Format("&msg{0}={1}", id.ToString(), /*WebUtility.UrlEncode(keyValue.Value)*/ System.Web.HttpUtility.UrlEncode(keyValue.Value)));
                builder.Append('&');
                ++id;
            }
            builder.Remove(builder.Length - 1, 1); //wywalamy ostatnie to coś &

            item.Data = Encoding.UTF8.GetBytes(builder.ToString());
            item.OperationType = PushOperation.SendMessage;
            item.Locker = locker;
            item.Callback = callback;
            item.UserData = userData;

            EnqueuePushRequest(item);
        }
        /// <summary>
        /// Obsłuż zapytanie o wysłanie obrazka.
        /// </summary>
        /// <param name="imageData">Dane obrazka.</param>
        /// <param name="locker">Blokada dla operacji synchronicznej.</param>
        /// <param name="callback">Zwrot dla operacji asynchronicznej.</param>
        /// <param name="userData">Dane użytkownika.</param>
        protected void ProcessSendImage(byte[] imageData, ManualResetEvent locker, PushRequestResultAsync callback, object userData)
        {
            PushRequestItem item = new PushRequestItem();
            item.Uri = Container.BOTAPI_BOTMASTER_RESOURCE_PUT_IMAGE + _botUin;

            using (MemoryStream memStream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(memStream))
                {
                    byte[] hash = Encoding.ASCII.GetBytes(Utils.ComputeHash(Utils.ComputeCrc32(imageData), imageData.Length));
                    writer.Write(hash);
                    writer.Write(imageData);
                }
                item.Data = memStream.ToArray();
            }
            //item.Data = imageData;
            item.Locker = locker;
            item.Callback = callback;
            item.UserData = userData;
            item.OperationType = PushOperation.ImageSend;
            item.UseMainServer = true;

            EnqueuePushRequest(item);
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
                bodyStream.Close();

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
        /// <summary>
        /// Kiedy serwer wystartuje.
        /// </summary>
        protected void OnStarted()
        {
            if (Started != null) Started(this, EventArgs.Empty);
        }
        /// <summary>
        /// Kiedy serwer się zatrzyma
        /// </summary>
        protected void OnStopped()
        {
            if (Stopped != null) Stopped(this, EventArgs.Empty);
        }
        /// <summary>
        /// Kiedy zostanie przysłana wiadomość.
        /// </summary>
        /// <param name="e">Argumenty</param>
        protected void OnMessageReceived(MessageEventArgs e)
        {
            if (MessageReceived != null) MessageReceived(this, e);
        }
        /// <summary>
        /// Kiedy zostanie wyrzucony błąd zapytania o token.
        /// </summary>
        protected void OnTokenRequestErrorOccurred()
        {
            if (TokenRequestErrorOccurred != null) TokenRequestErrorOccurred(this, EventArgs.Empty);
        }
        #endregion
        #endregion
    }
}
