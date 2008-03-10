using System;

namespace SvnBridge.Interfaces
{
    public interface ILogger
    {
        void Error(string message, Exception ex);
    }
}