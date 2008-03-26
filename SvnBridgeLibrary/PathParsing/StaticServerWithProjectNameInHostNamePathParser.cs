using System;
using SvnBridge.Interfaces;
using SvnBridge.Net;

namespace SvnBridge.PathParsing
{
	public class StaticServerWithProjectNameInHostNamePathParser : StaticServerPathParser
	{
		public StaticServerWithProjectNameInHostNamePathParser(string server) : base(server)
		{
		}

		public override string GetProjectName(IHttpRequest request)
		{
			return request.Headers["Host"].Split('.')[0];
		}
	}
}