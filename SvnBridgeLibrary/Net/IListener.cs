using System;

namespace SvnBridge.Net
{
    public interface IListener
    {
        int Port { get; set; }
        string TfsUrl { get; set; }

        event EventHandler<ListenErrorEventArgs> ListenError;

        void Start();
        void Stop();
    }
}