using System;
using SvnBridge.Interfaces;
using System.IO;

namespace SvnBridge.Infrastructure
{
    public class FileLogger : ILogger
    {
        private static void Write(Action<TextWriter> action)
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "errors.log");
            using (TextWriter tw = new StreamWriter(logPath, true))
            {
                action(tw);
            }
        }

        public void Error(string message, Exception ex)
        {
            Write(delegate(TextWriter writer)
            {
                writer.WriteLine(message);
                writer.WriteLine(ex);
                writer.WriteLine("- - - - - - - - - - - - - - - - - - - - - - - -");
                writer.WriteLine("- - - - - - - - - - - - - - - - - - - - - - - -");
            });
        }
    }
}