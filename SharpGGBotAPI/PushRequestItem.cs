using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SharpGGBotAPI
{
    internal class PushRequestItem
    {
        private string _uri = string.Empty;
        private byte[] _data = null;
        private ManualResetEvent _locker = null;
        private PushRequestResultAsync _callback = null;
        private PushOperation _operationType = PushOperation.None;
        private object _userData = null;
        private bool _useMainServer = false;

        public string Uri
        {
            get { return _uri; }
            set { _uri = value; }
        }
        public byte[] Data
        {
            get { return _data; }
            set { _data = value; }
        }
        internal ManualResetEvent Locker
        {
            get { return _locker; }
            set { _locker = value; }
        }
        internal PushRequestResultAsync Callback
        {
            get { return _callback; }
            set { _callback = value; }
        }
        internal PushOperation OperationType
        {
            get { return _operationType; }
            set { _operationType = value; }
        }
        internal object UserData
        {
            get { return _userData; }
            set { _userData = value; }
        }
        internal bool UseMainServer
        {
            get { return _useMainServer; }
            set { _useMainServer = value; }
        }
    }
}
