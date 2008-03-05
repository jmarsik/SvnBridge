using System;

namespace SvnBridge.Infrastructure
{
    public class ConsoleLogger : ILogger
    {
        public void Error(string message, Exception ex)
        {
            Console.Error.WriteLine(message);
            Console.Error.WriteLine(ex);
        }
    }
}