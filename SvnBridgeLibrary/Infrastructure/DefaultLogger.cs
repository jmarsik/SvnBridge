using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using SvnBridge.Interfaces;
using System.Configuration;

namespace SvnBridge.Infrastructure
{
    public class DefaultLogger : ILogger, ICanValidateMyEnvironment
    {
        #region ILogger Members
        string logPath;

        public void Error(string message, Exception exception)
        {
            WebException we = exception as WebException;
            if (we != null && we.Response != null)
            {
                HttpWebResponse hwr = we.Response as HttpWebResponse;
                if (hwr != null && hwr.StatusCode != HttpStatusCode.Unauthorized)
                {
                    using (StreamReader sr = new StreamReader(we.Response.GetResponseStream()))
                    {
                        StringBuilder sb = new StringBuilder(message);
                        sb.AppendLine(" Error page is:");
                        sb.AppendLine(sr.ReadToEnd());
                        message = sb.ToString();
                    }
                }
            }
            Log("Error", message, exception.ToString());
        }

        public void Info(string message, Exception exception)
        {
            Log("Info", message, exception.ToString());
        }

        public void Trace(string message, params object[] args)
        {
            if (Logging.TraceEnabled == false)
                return;
            Log("Trace", string.Format(message, args), null);
        }

        public void TraceMessage(string message)
        {
            if (Logging.TraceEnabled == false)
                return;
            Log("TraceMessage", message, null);
        }

        #endregion

        private void Log(string level, string message, string exception)
        {
            try
            {
                WriteLogMessageWithNoExceptionHandling(level, message, exception);
            }
            catch (Exception)
            {
                //We don't have anything to do here, can't 
                // fix errors in error handling code
            }
        }

        private void WriteLogMessageWithNoExceptionHandling(string level, string message, string exception)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;

            string logFile = Path.Combine(LogPath, level + ".log");

            using (StreamWriter text = File.AppendText(logFile))
            using (XmlWriter writer = XmlWriter.Create(text, settings))
            {
                writer.WriteStartElement("log");
                writer.WriteAttributeString("level", level);
                WriteCDataElement(writer, "message", message);
                if (string.IsNullOrEmpty(exception) == false)
                    WriteCDataElement(writer, "exception", exception);
                writer.WriteEndElement();
            }
        }

        private string LogPath
        {
            get
            {
                if (logPath != null)
                    return logPath;
                logPath = ConfigurationManager.AppSettings["LogPath"];
                if (logPath != null)
                    return logPath;
                logPath = "";
                try
                {
                    File.WriteAllText("tmp.log", "test");
                    File.Delete("tmp.log");
                }
                catch (UnauthorizedAccessException)
                {
                    string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    logPath = Path.Combine(localAppData, "SvnBridge");
                    if (Directory.Exists(logPath) == false)
                        Directory.CreateDirectory(logPath);
                }
                return logPath;
            }
        }

        private static void WriteCDataElement(XmlWriter writer, string name, string message)
        {
            writer.WriteStartElement(name);
            writer.WriteCData(message);
            writer.WriteEndElement();
        }

        #region ICanValidateMyEnvironment Members

        public void ValidateEnvironment()
        {
            WriteLogMessageWithNoExceptionHandling("test", "can write to file", null);
        }

        #endregion
    }
}