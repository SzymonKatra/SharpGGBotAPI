using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpGGBotAPI
{
    internal class Utils
    {
        public static uint ToInternalStatus(Status status, bool description)
        {
            switch (status)
            {
                case Status.Advertising: return Container.BOTAPI_STATUS_ADVERTISING;
                case Status.Available: return (description ? Container.BOTAPI_STATUS_AVAILABLE_DESCR : Container.BOTAPI_STATUS_AVAILABLE);
                case Status.Busy: return (description ? Container.BOTAPI_STATUS_BUSY_DESCR : Container.BOTAPI_STATUS_BUSY);
                case Status.DoNotDisturb: return (description ? Container.BOTAPI_STATUS_DND_DESCR : Container.BOTAPI_STATUS_DND);
                case Status.FreeForCall: return (description ? Container.BOTAPI_STATUS_FFC_DESCR : Container.BOTAPI_STATUS_FFC);
                case Status.Invisible: return (description ? Container.BOTAPI_STATUS_INVISIBLE_DESCR : Container.BOTAPI_STATUS_INVISIBLE);
                case Status.None:
                default: return Container.BOTAPI_STATUS_CURRENT;
            }
        }
        public static Status ToPublicStatus(uint status, out bool description)
        {
            description = false;
            switch (status)
            {
                case Container.BOTAPI_STATUS_ADVERTISING: return Status.Advertising;
                case Container.BOTAPI_STATUS_AVAILABLE: return Status.Available;
                case Container.BOTAPI_STATUS_AVAILABLE_DESCR: description = true; return Status.Available;
                case Container.BOTAPI_STATUS_BUSY: return Status.Busy;
                case Container.BOTAPI_STATUS_BUSY_DESCR: description = true; return Status.Busy;
                case Container.BOTAPI_STATUS_DND: return Status.DoNotDisturb;
                case Container.BOTAPI_STATUS_DND_DESCR: description = true; return Status.DoNotDisturb;
                case Container.BOTAPI_STATUS_FFC: return Status.FreeForCall;
                case Container.BOTAPI_STATUS_FFC_DESCR: description = true; return Status.FreeForCall;
                case Container.BOTAPI_STATUS_INVISIBLE: return Status.Invisible;
                case Container.BOTAPI_STATUS_INVISIBLE_DESCR: description = true; return Status.Invisible;
                case Container.BOTAPI_STATUS_CURRENT:
                default: return Status.None;
            }
        }
    }
}
