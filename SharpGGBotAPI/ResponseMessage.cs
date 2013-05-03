using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybridDSP.Net.HTTP;
using System.IO;

namespace SharpGGBotAPI
{
    public class ResponseMessage
    {
        private uint[] _recipients = null;
        private string _plainMessage = string.Empty;
        private string _htmlMessage = string.Empty;
        private byte[] _attributes = null;
        private bool _sendToOffline = false;
        private HTTPServerResponse _httpResponse;

        public uint[] Recipients
        {
            get { return _recipients; }
            set { _recipients = value; }
        }
        public string PlainMessage
        {
            get { return _plainMessage; }
            set { _plainMessage = value; }
        }
        public string HtmlMessage
        {
            get { return _htmlMessage; }
            set { _htmlMessage = value; }
        }
        public byte[] Attributes
        {
            get { return _attributes; }
            set { _attributes = value; }
        }
        public bool SendToOffline
        {
            get { return _sendToOffline; }
            set { _sendToOffline = value; }
        }
        internal HTTPServerResponse HttpResponse
        {
            get { return _httpResponse; }
            set { _httpResponse = value; }
        }

        public void MakeHtmlFromPlain()
        {
            _htmlMessage = string.Format("<span style=\"color:#000000; font-family:'MS Shell Dlg 2'; font-size:9pt; \">{0}</span>\0", _plainMessage);
        }
        public void Send()
        {
            if (_httpResponse == null) throw new NullReferenceException("HTTP response was null!");

            //add some data to http header

            //about recipients
            StringBuilder recipientsBuiler = new StringBuilder();
            foreach (uint uin in _recipients) recipientsBuiler.Append(uin.ToString() + ',');
            recipientsBuiler.Remove(recipientsBuiler.Length - 1, 1);

            //about send only to online clients
            _httpResponse.Add("To", recipientsBuiler.ToString());
            if (!_sendToOffline) _httpResponse.Add("Send-to-offline", "false");

            /*we can add message struct
            all strings are UTF-8
            
            uint htmlLength    //html message length. add \0 char too
            uint plainLength   //plain message length. add \0 char too
            uint imgLength     //image struct length. 0 if you don't send image
            uint formatLength  //plain text formatting length. 0 if you don't send formatting
            byte[htmlLength]   //html message with \0 ending
            byte[plainLength]  //plain message with \0 ending
            byte[imgLength]    //image struct (optional)
            byte[formatLength] //formatting struct (optional)
            */

            byte[] htmlMsg = Encoding.UTF8.GetBytes(_htmlMessage + '\0');
            byte[] plainMsg = Encoding.UTF8.GetBytes(_plainMessage + '\0');

            Stream bodyStream = _httpResponse.Send();
            bodyStream.Write(BitConverter.GetBytes(htmlMsg.Length), 0, 4); //4 is int length
            bodyStream.Write(BitConverter.GetBytes(plainMsg.Length), 0, 4);
            bodyStream.Write(BitConverter.GetBytes((int)0), 0, 4);
            bodyStream.Write(BitConverter.GetBytes((_attributes == null ? 0 : _attributes.Length)), 0, 4);

            bodyStream.Write(htmlMsg, 0, htmlMsg.Length);
            bodyStream.Write(plainMsg, 0, plainMsg.Length);
            if (_attributes != null) bodyStream.Write(_attributes, 0, _attributes.Length);
            bodyStream.Flush();

            //_httpResponse.Write(bodyStream);
        }
    }
}
