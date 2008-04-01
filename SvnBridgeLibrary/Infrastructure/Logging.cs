namespace SvnBridge.Infrastructure
{
	public static class Logging
	{
		private static bool traceEnabled = true;

		public static bool TraceEnabled
		{
			get { return traceEnabled; }
			set { traceEnabled = value; }
		}
	}
}