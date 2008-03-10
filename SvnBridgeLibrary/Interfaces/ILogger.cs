using System;
using System.Net.Sockets;

namespace SvnBridge.Interfaces
{
    public interface ILogger
    {
        void Error(string message, Exception exception);
        void Info(string message, Exception exception);
    }
}