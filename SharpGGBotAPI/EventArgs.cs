using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace SharpGGBotAPI
{
    public class MessageEventArgs : EventArgs
    {
        private uint _uin = 0;
        private string _message = string.Empty;
        private ResponseMessage _response;

        public uint Uin
        {
            get { return _uin; }
            set { _uin = value; }
        }
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        public ResponseMessage Response
        {
            get { return _response; }
            set { _response = value; }
        }
    }

    public class PushRequestErrorEventArgs : EventArgs
    {
        private HttpStatusCode _httpErrorCode = 0;
        private int _botmasterErrorCode = 0;
        private string _message = string.Empty;

        public HttpStatusCode HttpErrorCode
        {
            get { return _httpErrorCode; }
            set { _httpErrorCode = value; }
        }
        public int BotmasterErrorCode
        {
            get { return _botmasterErrorCode; }
            set { _botmasterErrorCode = value; }
        }
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
    }
}
