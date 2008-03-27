using System;
using SvnBridge.Interfaces;
using SvnBridge.Net;

namespace SvnBridge.PathParsing
{
	public class StaticServerPathParser : BasePathParser
	{
		private readonly string server;

		public StaticServerPathParser(string server)
		{
			Uri ignored;
			if (Uri.TryCreate(server, UriKind.Absolute, out ignored) == false)
				throw new InvalidOperationException("The url '" + server + "' is not a valid url");

			this.server = server;
		}

		public override string GetServerUrl(Uri requestUrl)
		{
			return server;
		}

		public override string GetLocalPath(IHttpRequest request)
		{
			return request.LocalPath;
		}

		public override string GetLocalPath(IHttpRequest request, string url)
		{
			Uri urlAsUri = new Uri(url);
			string path = urlAsUri.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped);
			if (path == "")
				return "/";
			return path;
		}

		public override string GetProjectName(IHttpRequest request)
		{
			return null;
		}

		public override string GetApplicationPath(IHttpRequest request)
		{
			return request.ApplicationPath;
		}
	}
}