using System;
using System.Net.Sockets;

namespace SvnBridge.Interfaces
{
    public interface ILogger
    {
        void Error(string message, Exception exception);
        void Info(string message, Exception exception);
		void Trace(string message, params object[] args);
    }
}