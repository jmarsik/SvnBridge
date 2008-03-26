using System;
using SvnBridge.Interfaces;
using SvnBridge.PathParsing;

namespace SvnBridge.Net
{
    public interface IListener
    {
        int Port { get; set; }

        event EventHandler<ListenErrorEventArgs> ListenError;
        event EventHandler<FinishedHandlingEventArgs> FinishedHandling;

        void Start(IPathParser parser);
        void Stop();
    }
}