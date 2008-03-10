using System;
using SvnBridge.Interfaces;
using System.IO;

namespace SvnBridge.Infrastructure
{
    public class FileLogger : ILogger
    {
        private static void Write(string level, Action<TextWriter> action)
        {
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, level + ".log");
            using (TextWriter tw = new StreamWriter(logPath, true))
            {
                tw.WriteLine(level);
                tw.WriteLine(DateTime.Now.ToString("yyyy MM dd hh:mm:ss:fff"));
                action(tw);
                tw.WriteLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -");
            }
        }

        public void Error(string message, Exception exception)
        {
            Write("Error", delegate(TextWriter writer)
            {
                writer.WriteLine(message);
                writer.WriteLine(exception);
            });
        }

        public void Info(string message, Exception exception)
        {

            Write("Info", delegate(TextWriter writer)
            {
                writer.WriteLine(message);
                writer.WriteLine(exception);
            });
        }
    }
}