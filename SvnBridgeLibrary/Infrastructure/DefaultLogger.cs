using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using SvnBridge.Interfaces;

namespace SvnBridge.Infrastructure
{
	public class DefaultLogger : ILogger, ICanValidateMyEnvironment
	{
		#region ILogger Members

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

		private static void Log(string level, string message, string exception)
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

		private static void WriteLogMessageWithNoExceptionHandling(string level, string message, string exception)
		{
			using(XmlWriter writer = XmlWriter.Create(File.AppendText(level+".log")))
			{
				writer.WriteStartElement("log");
				writer.WriteAttributeString("level", level);
				writer.WriteElementString("message", message);
				if(string.IsNullOrEmpty(exception)==false)
					writer.WriteElementString("exception", exception);
				writer.WriteEndElement();
			}
		}

		#region ICanValidateMyEnvironment Members

		public void ValidateEnvironment()
		{
			WriteLogMessageWithNoExceptionHandling("test", "can write to file", null);
		}

		#endregion
	}
}