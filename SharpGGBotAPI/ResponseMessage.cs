using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HybridDSP.Net.HTTP;
using System.IO;

namespace SharpGGBotAPI
{
    ///// <summary>
    ///// Struktura obrazka GG Bot API
    ///// </summary>
    //public class ImageStructure
    //{
    //    #region Properties
    //    private long _crc32 = 0;
    //    private long _length = -1;
    //    private byte[] _data = null;

    //    /// <summary>
    //    /// Suma kontrolna CRC32 obrazka.
    //    /// </summary>
    //    public long CRC32
    //    {
    //        get { return _crc32; }
    //        set { _crc32 = value; }
    //    }
    //    /// <summary>
    //    /// Wielkość obrazka w bajtach. Jeśli wynosi -1, wielkość zostanie pobrana z właściwości Data.
    //    /// </summary>
    //    public long Length
    //    {
    //        get { return (_length < 0 && _data != null ? _data.LongLength : 0); }
    //        set { _length = value; }
    //    }
    //    /// <summary>
    //    /// Obrazek.
    //    /// </summary>
    //    public byte[] Data
    //    {
    //        get { return _data; }
    //        set { _data = value; }
    //    }
    //    #endregion

    //    #region Constructor
    //    /// <summary>
    //    /// Stwórz pustą struktrurę obrazka.
    //    /// </summary>
    //    public ImageStructure()
    //    {
    //    }
    //    /// <summary>
    //    /// Stwórz strukturę obrazka i oblicz sumę kontrolną CRC32 jeśli flaga computeCrc32 zostanie ustawiona na true.
    //    /// </summary>
    //    /// <param name="data">Dane obrazka.</param>
    //    /// <param name="computeCrc32">Obliczyć od razu sumę kontrolną CRC32?</param>
    //    public ImageStructure(byte[] data, bool computeCrc32 = true)
    //    {
    //        _data = data;
    //        ComputeCrc32FromData();
    //    }
    //    /// <summary>
    //    /// Stwórz strukturę obrazka. Botmaster sprawdzi czy taki obrazek istnieje na serwerze i jeśli tak, roześle go.
    //    /// </summary>
    //    /// <param name="crc32">Suma kontrolna CRC32.</param>
    //    /// <param name="length">Wielkość obrazka w bajtach.</param>
    //    public ImageStructure(long crc32, long length)
    //    {
    //        _crc32 = crc32;
    //        _length = length;
    //    }
    //    #endregion

    //    #region Methods
    //    /// <summary>
    //    /// Oblicz sumę kontrolną CRC32 z obrazka zapisanego w właściwości Data.
    //    /// </summary>
    //    public void ComputeCrc32FromData()
    //    {
    //        if (_data == null) throw new NullReferenceException("Data is null");
    //        _crc32 = Crc32.ComputeChecksum(_data);
    //    }

        
    //    #endregion
    //}

    /// <summary>
    /// Odpowiedź zwrotna na wiadomość przychodzącą.
    /// </summary>
    public class ResponseMessage
    {
        #region Properties
        private uint[] _recipients = null;
        private string _plainMessage = string.Empty;
        private string _htmlMessage = string.Empty;
        private byte[] _attributes = null;
        private bool _sendToOffline = false;
        private HTTPServerResponse _httpResponse;

        /// <summary>
        /// Odbiorcy wiadomości.
        /// </summary>
        public uint[] Recipients
        {
            get { return _recipients; }
            set { _recipients = value; }
        }
        /// <summary>
        /// Wiadomość zapisana czystym tekstem.
        /// </summary>
        public string PlainMessage
        {
            get { return _plainMessage; }
            set { _plainMessage = value; }
        }
        /// <summary>
        /// Wiadomość zapisana w HTML.
        /// </summary>
        public string HtmlMessage
        {
            get { return _htmlMessage; }
            set { _htmlMessage = value; }
        }
        /// <summary>
        /// Atrybuty wiadomości zapisanej czystym tekstem.
        /// </summary>
        public byte[] Attributes
        {
            get { return _attributes; }
            set { _attributes = value; }
        }
        /// <summary>
        /// Czy wysyłać wiadomość do niedostępnych klientów?
        /// </summary>
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
        #endregion

        #region Methods
        /// <summary>
        /// Dopisz tekst (html i zwykły) do wiadomości.
        /// Domyślnie bez formatowania, kolor tła i tekstu czarny, czcionka MS Shell Dlg 2 o wielkości 9pt.
        /// </summary>
        /// <param name="text">Tekst.</param>
        /// <param name="formatting">Flagi formatowania.</param>
        /// <param name="redColor">Składowa czerwieni koloru tekstu.</param>
        /// <param name="greenColor">Składowa zieleni koloru tekstu.</param>
        /// <param name="blueColor">Składowa niebieska koloru tekstu.</param>
        /// <param name="fontFamily">Nazwa czcionki.</param>
        /// <param name="fontSize">Rozmiar czcionki, np. 9pt czyli 9 punktów.</param>
        public void AppendText(string text, MessageFormatting formatting = MessageFormatting.None, byte redColor = 0, byte greenColor = 0, byte blueColor = 0, string fontFamily = "MS Shell Dlg 2", string fontSize = "9pt")
        {
            //check color exists. if true then end <span> and set flag
            bool haveColor = false;
            if (redColor != 0 || greenColor != 0 || blueColor != 0)
            {
                haveColor = true;
                if (fontFamily != "MS Shell Dlg 2" || fontSize != "9pt")
                {
                    if (!_htmlMessage.EndsWith("</span>"))
                    {
                        _htmlMessage += "</span>";
                    }
                }
            }

            #region Html
            //open html message if closed
            if (string.IsNullOrEmpty(_htmlMessage) || _htmlMessage.EndsWith("</span>"))
            {
                _htmlMessage += string.Format("<span style=\"color:#{0}{1}{2}; font-family:'{3}'; font-size:{4}; \">",
                    redColor.ToString("X2"),
                    greenColor.ToString("X2"),
                    blueColor.ToString("X2"),
                    fontFamily,
                    fontSize);

            }

            //build tags
            StringBuilder builder = new StringBuilder();

            if (formatting.HasFlag(MessageFormatting.Bold)) builder.Append("<b>");
            if (formatting.HasFlag(MessageFormatting.Erasure)) builder.Append("<s>");
            if (formatting.HasFlag(MessageFormatting.Italic)) builder.Append("<i>");
            if (formatting.HasFlag(MessageFormatting.Subscript)) builder.Append("<sub>");
            if (formatting.HasFlag(MessageFormatting.Superscript)) builder.Append("<sup>");
            if (formatting.HasFlag(MessageFormatting.Underline)) builder.Append("<u>");

            //add text
            builder.Append(text);

            //and close tags
            if (formatting.HasFlag(MessageFormatting.Underline)) builder.Append("</u>");
            if (formatting.HasFlag(MessageFormatting.Superscript)) builder.Append("</sup>");
            if (formatting.HasFlag(MessageFormatting.Subscript)) builder.Append("</sub>");
            if (formatting.HasFlag(MessageFormatting.Italic)) builder.Append("</i>");
            if (formatting.HasFlag(MessageFormatting.Erasure)) builder.Append("</s>");
            if (formatting.HasFlag(MessageFormatting.Bold)) builder.Append("</b>");

            if (formatting.HasFlag(MessageFormatting.NewLine)) builder.Append("<br>");

            //add built message
            _htmlMessage += builder.ToString();
            #endregion

            #region Plain
            //plain message for GG 7.x and oldest
            _plainMessage += text;
            if (formatting.HasFlag(MessageFormatting.NewLine)) _plainMessage += Environment.NewLine;

            //attributes
            using (MemoryStream memStream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(memStream))
                {
                    //flag
                    writer.Write(Container.BOTAPI_ATTRIBUTES_FLAG);
                    //position
                    writer.Write((short)(_plainMessage.Length - text.Length));
                    //flags, bold etc.
                    writer.Write((byte)((formatting.HasFlag(MessageFormatting.Bold) ? Container.BOTAPI_ATTRIBUTES_BOLD : 0) |
                        (formatting.HasFlag(MessageFormatting.Italic) ? Container.BOTAPI_ATTRIBUTES_ITALIC : 0) |
                        (formatting.HasFlag(MessageFormatting.Underline) ? Container.BOTAPI_ATTRIBUTES_UNDERLINE : 0) |
                        (haveColor ? Container.BOTAPI_ATTRIBUTES_COLOR : 0)));
                    //color if exists
                    if (haveColor)
                    {
                        writer.Write(redColor);
                        writer.Write(greenColor);
                        writer.Write(blueColor);
                    }
                }
                //go attributes to can :D
                byte[] newData = memStream.ToArray();

                if (_attributes == null)
                    _attributes = new byte[newData.Length];
                else
                    Array.Resize(ref _attributes, _attributes.Length + newData.Length);

                //copy new attributes to global can ;P
                Buffer.BlockCopy(newData, 0, _attributes, _attributes.Length - newData.Length, newData.Length);
            }
            #endregion
        }

        /// <summary>
        /// Dodaj obrazek do wiadomości.
        /// Obrazek musi być najpierw wysłany na serwer.
        /// </summary>
        /// <param name="hash">Hash obrazka</param>
        public void AppendImage(string hash)
        {
            try
            {
                if (hash.Length != 16) throw new InvalidOperationException("Bad hash length");

                AppendImage(Convert.ToInt64(hash.Remove(8), 16), Convert.ToInt64(hash.Remove(0, 8), 16));
            }
            catch { throw new InvalidOperationException("Bad hash"); }
        }
        /// <summary>
        /// Dodaj obrazek do wiadomości.
        /// Obrazek musi być najpierw wysłany na serwer.
        /// </summary>
        /// <param name="crc32">Suma kontrolna CRC32.</param>
        /// <param name="length">Wielkość obrazka w bajtach.</param>
        public void AppendImage(long crc32, long length)
        {
            #region Html
            //open html message if closed
            if (string.IsNullOrEmpty(_htmlMessage) || _htmlMessage.EndsWith("</span>"))
            {
                _htmlMessage += string.Format("<span style=\"color:#000000; font-family:'MS Shell Dlg 2'; font-size:9pt; \">");
            }
            _htmlMessage += string.Format("<img name=\"{0}\"></img>", Utils.ComputeHash(crc32, length));
            #endregion

            #region Plain
            using (MemoryStream memStream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(memStream))
                {
                    //flag
                    writer.Write(Container.BOTAPI_ATTRIBUTES_FLAG);
                    //position
                    writer.Write((short)_plainMessage.Length);
                    writer.Write(Container.BOTAPI_ATTRIBUTES_IMAGE);
                    writer.Write(Container.BOTAPI_IMAGE_CONST_FIRST);
                    writer.Write(Container.BOTAPI_IMAGE_CONST_LAST);
                    writer.Write((uint)length);
                    writer.Write((uint)crc32);
                }
                //go attributes to can :D
                byte[] newData = memStream.ToArray();

                if (_attributes == null)
                    _attributes = new byte[newData.Length];
                else
                    Array.Resize(ref _attributes, _attributes.Length + newData.Length);

                //copy new attributes to global can ;P
                Buffer.BlockCopy(newData, 0, _attributes, _attributes.Length - newData.Length, newData.Length);
            }
            #endregion
        }

        /// <summary>
        /// Wyślij odpowiedź.
        /// </summary>
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

            if (!string.IsNullOrEmpty(_htmlMessage) && !_htmlMessage.EndsWith("</span>")) _htmlMessage += "</span>";

            byte[] htmlMsg = Encoding.UTF8.GetBytes(_htmlMessage + '\0');
            byte[] plainMsg = Encoding.UTF8.GetBytes(_plainMessage + '\0');

            Stream bodyStream = _httpResponse.Send();
            bodyStream.Write(BitConverter.GetBytes(htmlMsg.Length), 0, 4); //4 is int length
            bodyStream.Write(BitConverter.GetBytes(plainMsg.Length), 0, 4);
            bodyStream.Write(BitConverter.GetBytes((int)0), 0, 4);
            //bodyStream.Write(BitConverter.GetBytes((int)(_image == null ? 0 : (int)_image.Length + 16)), 0, 4);
            bodyStream.Write(BitConverter.GetBytes((_attributes == null ? 0 : _attributes.Length)), 0, 4);

            bodyStream.Write(htmlMsg, 0, htmlMsg.Length);
            bodyStream.Write(plainMsg, 0, plainMsg.Length);
            //if (_image != null)
            //{
            //    bodyStream.Write(BitConverter.GetBytes(_image.CRC32), 0, 8); //8 is long length
            //    bodyStream.Write(BitConverter.GetBytes(_image.Length), 0, 8);
            //    //byte[] crc32 = Encoding.ASCII.GetBytes(_image.CRC32.ToString("X"));
            //    //byte[] length = Encoding.ASCII.GetBytes(_image.Length.ToString("X"));
            //    //bodyStream.Write(crc32, 0, crc32.Length);
            //    //bodyStream.Write(length, 0, length.Length);
            //    if (_image.Data != null) bodyStream.Write(_image.Data, 0, _image.Data.Length);
            //}
            if (_attributes != null) bodyStream.Write(_attributes, 0, _attributes.Length);
            bodyStream.Flush();

            //_httpResponse.Write(bodyStream);
        }
        #endregion
    }
}
