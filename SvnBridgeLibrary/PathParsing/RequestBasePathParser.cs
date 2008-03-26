using System;
using System.IO;
using SvnBridge.Interfaces;
using SvnBridge.Net;

namespace SvnBridge.PathParsing
{
	public class RequestBasePathParser : BasePathParser
	{
		private readonly ITfsUrlValidator urlValidator;

		public RequestBasePathParser(ITfsUrlValidator urlValidator)
		{
			this.urlValidator = urlValidator;
		}

		public override string GetServerUrl(Uri requestUrl)
		{
			string url = GetUrlFromRequest(requestUrl);

			if(urlValidator.IsValidTfsServerUrl("https://"+url))
				return "https://" + url;
			return "http://" + url;
		}

		private static string GetUrlFromRequest(Uri requestUrl)
		{
			string path = requestUrl.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped);
			int firstIndexOfSlash = path.IndexOf('/');
			
			if(firstIndexOfSlash==-1)
				throw new InvalidOperationException("Could not find server url in the url. Not valid when using the RequestBasePathParser");

			string url = path.Substring(0, firstIndexOfSlash);
			return url;
		}

		public override string GetLocalPath(IHttpRequest request)
		{
			return GetLocalPath(request, request.Url.AbsoluteUri);
		}

		public override string GetLocalPath(IHttpRequest request, string url)
		{
			Uri urlAsUri = new Uri(url);
			string path = urlAsUri.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped);
			string urlFromRequest = GetUrlFromRequest(urlAsUri);
			string localPath = path.Substring(urlFromRequest.Length);
			if (localPath == "")
				return "/";
			return localPath;
		}

		public override string GetProjectName(IHttpRequest request)
		{
			return null;
		}

		public override string GetApplicationPath(IHttpRequest request)
		{
			string url = GetUrlFromRequest(request.Url);
			string path = url + request.ApplicationPath;
			if(path.StartsWith("/")==false)
				path = '/' + path;
			if (path.EndsWith("/") == false)
				path = path + '/';
			return path;
		}
	}
}