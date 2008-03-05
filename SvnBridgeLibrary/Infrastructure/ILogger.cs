using System;

namespace SvnBridge.Infrastructure
{
    public interface ILogger
    {
        void Error(string message, Exception ex);
    }
}