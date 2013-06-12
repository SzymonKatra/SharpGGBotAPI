using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGGBotAPI
{
    internal class Container
    {
        public const uint BOTAPI_STATUS_CURRENT = 0;
        public const uint BOTAPI_STATUS_AVAILABLE = 2;
        public const uint BOTAPI_STATUS_BUSY = 3;
        public const uint BOTAPI_STATUS_AVAILABLE_DESCR = 4;
        public const uint BOTAPI_STATUS_BUSY_DESCR = 5;
        public const uint BOTAPI_STATUS_INVISIBLE = 20;
        public const uint BOTAPI_STATUS_INVISIBLE_DESCR = 22;
        public const uint BOTAPI_STATUS_FFC = 23;
        public const uint BOTAPI_STATUS_FFC_DESCR = 24;
        public const uint BOTAPI_STATUS_ADVERTISING = 32;
        public const uint BOTAPI_STATUS_DND = 33;
        public const uint BOTAPI_STATUS_DND_DESCR = 34;

        public const string BOTAPI_BOTMASTER_MAIN_SERVER = "http://botapi.gadu-gadu.pl/botmaster";
        public const string BOTAPI_BOTMASTER_RESOURCE_GET_TOKEN = "/getToken/";
        public const string BOTAPI_BOTMASTER_RESOURCE_SET_STATUS = "/setStatus/";
        public const string BOTAPI_BOTMASTER_RESOURCE_SEND_MESSAGE = "/sendMessage/";
        public const string BOTAPI_BOTMASTER_RESOURCE_PUT_IMAGE = "/putImage/";
        public const string BOTAPI_BOTMASTER_RESOURCE_EXISTS_IMAGE = "/existsImage/";
        public const string BOTAPI_BOTMASTER_RESOURCE_GET_IMAGE = "/getImage/";
        public const string BOTAPI_BOTMASTER_RESOURCE_IS_BOT = "/isBot/";
        public const string BOTAPI_REQUEST_USER_AGENT = "SharpGGBotAPI";

        public const byte BOTAPI_ATTRIBUTES_FLAG = 0x02;
        public const byte BOTAPI_ATTRIBUTES_BOLD = 0x01;
        public const byte BOTAPI_ATTRIBUTES_ITALIC = 0x02;
        public const byte BOTAPI_ATTRIBUTES_UNDERLINE = 0x04;
        public const byte BOTAPI_ATTRIBUTES_COLOR = 0x08;
        public const byte BOTAPI_ATTRIBUTES_IMAGE = 0x80;

        public const byte BOTAPI_IMAGE_CONST_FIRST = 0x09;
        public const byte BOTAPI_IMAGE_CONST_LAST = 0x01;
    }
}
