using System;
using SvnBridge.Interfaces;

namespace SvnBridge.Infrastructure
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

		#endregion
	}
}