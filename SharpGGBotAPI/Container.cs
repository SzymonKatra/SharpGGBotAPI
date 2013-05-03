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
    }
}
