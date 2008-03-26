using SvnBridge.Interfaces;
using SvnBridge.Net;

namespace SvnBridge.PathParsing
{
	public class StaticServerWithProjectNameInHostNamePathParser : IPathParser
	{
		private readonly string server;

		public StaticServerWithProjectNameInHostNamePathParser(string server)
		{
			this.server = server;
		}

		#region IPathParser Members

		public string GetServerUrl(IHttpRequest request)
		{
			return server;
		}

		public string GetLocalPath(IHttpRequest request)
		{
			return request.LocalPath;
		}

		public string GetProjectName(IHttpRequest request)
		{
			return request.Headers["Host"].Split('.')[0];
		}

		#endregion
	}
}