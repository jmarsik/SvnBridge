using System;

namespace SvnBridge.Net
{
    public class ListenErrorEventArgs : EventArgs
    {
        public Exception Exception
        {
            get { return exception; }
        }

        private readonly Exception exception;

        public ListenErrorEventArgs(Exception ex)
        {
            this.exception = ex;
        }
    }
}