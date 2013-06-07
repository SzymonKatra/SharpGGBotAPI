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
    /// Argumenty dla odpowiedzi na zapytanie PUSH.
    /// </summary>
    public class PushRequestResultEventArgs : EventArgs
    {
        private HttpStatusCode _httpErrorCode = 0;
        private PushErrorCode _botmasterErrorCode = 0;
        private string _botmasterMessage = string.Empty;
        private PushOperation _operationType = PushOperation.None;
        private string _imageHash = string.Empty;
        private byte[] _imageData;

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
        public PushErrorCode BotmasterErrorCode
        {
            get { return _botmasterErrorCode; }
            set { _botmasterErrorCode = value; }
        }
        /// <summary>
        /// Wiadomość od Botmastera GG.
        /// </summary>
        public string BotmasterMessage
        {
            get { return _botmasterMessage; }
            set { _botmasterMessage = value; }
        }
        /// <summary>
        /// Rodzaj operacji.
        /// </summary>
        public PushOperation OperationType
        {
            get { return _operationType; }
            set { _operationType = value; }
        }
        /// <summary>
        /// Hash obrazka który wysłaliśmy.
        /// Jeśli nie wysyłaliśmy obrazka lub wystąpił błąd to pole będzie puste.
        /// </summary>
        public string ImageHash
        {
            get { return _imageHash; }
            set { _imageHash = value; }
        }
        /// <summary>
        /// Dane obrazka.
        /// Jeśli nie pobieraliśmy obrazka lub wystąpił błąd to pole będzie puste.
        /// </summary>
        public byte[] ImageData
        {
            get { return _imageData; }
            set { _imageData = value; }
        }
    }

    /// <summary>
    /// Asynchroniczne argumenty dla odpowiedzi na zapytanie PUSH.
    /// </summary>
    public class PushRequestResultAsyncEventArgs : PushRequestResultEventArgs
    {
        private object _userObject = null;

        /// <summary>
        /// Dane użytkownika.
        /// </summary>
        public object UserData
        {
            get { return _userObject; }
            set { _userObject = value; }
        }

        /// <summary>
        /// Tworzy pusty obiekt.
        /// </summary>
        public PushRequestResultAsyncEventArgs()
        {
        }
        /// <summary>
        /// Tworzy argumenty z synchronicznego obiektu.
        /// </summary>
        public PushRequestResultAsyncEventArgs(PushRequestResultEventArgs e)
        {
            this.BotmasterErrorCode = e.BotmasterErrorCode;
            this.BotmasterMessage = e.BotmasterMessage;
            this.HttpErrorCode = e.HttpErrorCode;
            this.OperationType = e.OperationType;
        }
    }
}
