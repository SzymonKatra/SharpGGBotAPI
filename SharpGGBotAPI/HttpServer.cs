//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Net;
//using System.Net.Sockets;

//namespace SharpGGBotAPI
//{
//    internal class HttpItem
//    {
//        private string _name = string.Empty;
//        private List<string> _values = new List<string>();

//        public string Name
//        {
//            get { return _name; }
//            set { _name = value; }
//        }
//        public string Value
//        {
//            get { return (_values.Count > 0 ? _values[0] : string.Empty); }
//            set { if (_values.Count > 0) _values[0] = value; else _values.Add(value); }
//        }
//        public List<string> Values
//        {
//            get { return _values; }
//            set { _values = value; }
//        }
//    }
//    internal class HttpRequest
//    {
//        private string _httpVersion = string.Empty;
//        private string _method = string.Empty;
//        private List<HttpItem> _headers;
//        private List<HttpItem> _queryItems;
//        private long _contentLength = 0;
//        private string _uriPath = string.Empty;
//        private byte[] _body;

//        public string HttpVersion
//        {
//            get { return _httpVersion; }
//            set { _httpVersion = value; }
//        }
//        public string Method
//        {
//            get { return _method; }
//            set { _method = value; }
//        }
//        public List<HttpItem> Headers
//        {
//            get { return _headers; }
//            set { _headers = value; }
//        }
//        public List<HttpItem> QueryItems
//        {
//            get { return _queryItems; }
//            set { _queryItems = value; }
//        }
//        public long ContentLength
//        {
//            get { return _contentLength; }
//            set { _contentLength = value; }
//        }
//        public string UriPath
//        {
//            get { return _uriPath; }
//            set
//            {
//                _uriPath = value;
//                int queryIndex = value.IndexOf('?');
//                if (queryIndex >= 0)
//                {
//                    _queryItems = new List<HttpItem>();
//                    string[] query = value.Remove(0, queryIndex + 1).Split('&');
//                    foreach (string streamItem in query)
//                    {
//                        string[] vals = streamItem.Split('=');
//                        if (vals.Length == 2)
//                        {
//                            _queryItems.Add(new HttpItem() { Name = vals[0], Value = vals[1] });
//                        }
//                        else if (vals.Length > 2)
//                        {
//                            string[] moreVals = vals[1].Split(',');
//                            _queryItems.Add(new HttpItem() { Name = vals[0], Values = new List<string>(moreVals) });
//                        }
//                    }
//                }
//            }
//        }
//        public byte[] Body
//        {
//            get { return _body; }
//            set { _body = value; }
//        }
//    }
//    internal class HttpResponse
//    {
//        private string _httpVersion = "HTTP/1.1";
//        private string _errorCode = "200 OK";
//        private List<HttpItem> _headers;
//        private byte[] _body;
//        private TcpClient _client;

//        public string HttpVersion
//        {
//            get { return _httpVersion; }
//            set { _httpVersion = value; }
//        }
//        public string ErrorCode
//        {
//            get { return _errorCode; }
//            set { _errorCode = value; }
//        }
//        public List<HttpItem> Headers
//        {
//            get { return _headers; }
//            set { _headers = value; }
//        }
//        public byte[] Body
//        {
//            get { return _body; }
//            set { _body = value; }
//        }
//        internal TcpClient Client
//        {
//            get { return _client; }
//            set { _client = value; }
//        }

//        public void Send()
//        {
//            try
//            {
//            }
//            catch { }
//        }
//    }
//    internal class HttpRequestReceivedEventArgs : EventArgs
//    {
//        private HttpRequest _request;
//        private HttpResponse _response;

//        public HttpRequest Request
//        {
//            get { return _request; }
//            set { _request = value; }
//        }
//        public HttpResponse Response
//        {
//            get { return _response; }
//            set { _response = value; }
//        }
//    }

//    internal class HttpServer
//    {
//        private TcpListener _listener;
//        private long _maxRequestSize = 32768; //32 KB
//        private bool _isRunning = false;

//        public bool IsRunning
//        {
//            get { return _isRunning; }
//            set { _isRunning = value; }
//        }

//        public HttpServer()
//        {
//        }

//        public void Start(IPEndPoint endPoint, int backlog = 50)
//        {
//            if (_listener != null && _isRunning) throw new InvalidOperationException("Server was already running!");
//            try
//            {
//                _listener = new TcpListener(endPoint);
//                _listener.Start(backlog);
//                _listener.BeginAcceptTcpClient(new AsyncCallback(OnAcceptCallback), null);
//            }
//            catch { _isRunning = false; throw new SocketException(); }
//            _isRunning = true;
//        }
//        public void Stop()
//        {
//            try
//            {
//                _listener.Stop();
//            }
//            catch { }
//            _isRunning = false;
//        }

//        protected void OnAcceptCallback(IAsyncResult ar)
//        {
//            try
//            {
//                TcpClient client = _listener.EndAcceptTcpClient(ar);
                
//            }
//            catch { }
//        }
//    }
//}
