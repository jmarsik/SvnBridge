using System;
using SvnBridge.Interfaces;
using SvnBridge.Infrastructure;

namespace SvnBridge.NullImpl
{
    public class NullLogger : DefaultLogger
	{
		public override void Error(string message, Exception exception)
		{
		}

        public override void Info(string message, Exception exception)
		{
		}

        public override void Trace(string message, params object[] args)
		{
		}

        public override void TraceMessage(string message)
		{
		}
	}
}