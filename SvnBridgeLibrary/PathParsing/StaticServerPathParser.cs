using SvnBridge.Interfaces;
using SvnBridge.Net;

namespace SvnBridge.PathParsing
{
	public class StaticServerPathParser : IPathParser
	{
		private readonly string server;

		public StaticServerPathParser(string server)
		{
			this.server = server;
		}

		public string GetServerUrl(IHttpRequest request)
		{
			return server;
		}

		public string GetLocalPath(IHttpRequest request)
		{
			return request.LocalPath;
		}

		#region IPathParser Members

		public string GetProjectName(IHttpRequest request)
		{
			return null;
		}

		#endregion
	}
}