using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace SharpGGBotAPI
{
    /// <summary>
    /// Argumenty dla wiadomości przychodzącej.
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        private uint _uin = 0;
        private string _message = string.Empty;
        private ResponseMessage _response;

        /// <summary>
        /// Numer z którego wysłano wiadomość.
        /// </summary>
        public uint Uin
        {
            get { return _uin; }
            set { _uin = value; }
        }
        /// <summary>
        /// Wiadomość.
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        /// <summary>
        /// Twoja odpowiedź na wiadomość.
        /// </summary>
        public ResponseMessage Response
        {
            get { return _response; }
            set { _response = value; }
        }
    }

    /// <summary>
    /// Argumenty dla błędu zapytania PUSH.
    /// </summary>
    public class PushRequestErrorEventArgs : EventArgs
    {
        private HttpStatusCode _httpErrorCode = 0;
        private int _botmasterErrorCode = 0;
        private string _message = string.Empty;

        /// <summary>
        /// Kod błędu HTTP.
        /// </summary>
        public HttpStatusCode HttpErrorCode
        {
            get { return _httpErrorCode; }
            set { _httpErrorCode = value; }
        }
        /// <summary>
        /// Kod błędu Botmastera GG.
        /// </summary>
        public int BotmasterErrorCode
        {
            get { return _botmasterErrorCode; }
            set { _botmasterErrorCode = value; }
        }
        /// <summary>
        /// Wiadomość od Botmastera GG.
        /// </summary>
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
    }
}
