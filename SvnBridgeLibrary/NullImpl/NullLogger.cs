using System;
using SvnBridge.Interfaces;

namespace SvnBridge.NullImpl
{
	public class NullLogger : ILogger
	{
		#region ILogger Members

		public void Error(string message, Exception exception)
		{
		}

		public void Info(string message, Exception exception)
		{
		}

		public void Trace(string message, params object[] args)
		{
		}

		public void TraceMessage(string message)
		{
			
		}

		#endregion
	}
}