using System;
using System.IO;
using System.Net;
using System.Text;
using SvnBridge.Interfaces;
using SvnBridge.Net;
using SvnBridge.SourceControl;

namespace SvnBridge.Handlers
{
	public abstract class HttpContextHandlerBase
	{
		private IPathParser pathParser;
		private IHttpContext httpContext;
		public IPathParser PathParser
		{
			get { return pathParser; }
		}


		public void Handle(IHttpContext context, IPathParser pathParser)
		{
			Initialize(context, pathParser);
			IHttpRequest request = context.Request;
			string tfsUrl = pathParser.GetServerUrl(request.Url);
			string projectName = pathParser.GetProjectName(context.Request);

			ISourceControlProvider sourceControlProvider =
				SourceControlProviderFactory.Create(tfsUrl, projectName, GetCredential(context));
			Handle(context, sourceControlProvider);
		}

		public void Initialize(IHttpContext context, IPathParser pathParser)
		{
			this.httpContext = context;
			this.pathParser = pathParser;
		}

		public virtual void Cancel()
		{
		}

		protected abstract void Handle(IHttpContext context,
									   ISourceControlProvider sourceControlProvider);

		private static NetworkCredential GetCredential(IHttpContext context)
		{
			string authorizationHeader = context.Request.Headers["Authorization"];
			if (!string.IsNullOrEmpty(authorizationHeader))
			{
				if (authorizationHeader.StartsWith("Negotiate"))
				{
					return (NetworkCredential)CredentialCache.DefaultCredentials;
				}
				string encodedCredential = authorizationHeader.Substring(authorizationHeader.IndexOf(' ') + 1);
				string credential = UTF8Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredential));
				string[] credentialParts = credential.Split(':');

				string username = credentialParts[0];
				string password = credentialParts[1];

				if (username.IndexOf('\\') >= 0)
				{
					string domain = username.Substring(0, username.IndexOf('\\'));
					username = username.Substring(username.IndexOf('\\') + 1);
					return new NetworkCredential(username, password, domain);
				}
				else
				{
					return new NetworkCredential(username, password);
				}
			}
			else
			{
				return CredentialsHelper.NullCredentials;
			}
		}

		protected static void SetResponseSettings(IHttpResponse response,
												  string contentType,
												  Encoding contentEncoding,
												  int status)
		{
			response.ContentType = contentType;
			response.ContentEncoding = contentEncoding;
			response.StatusCode = status;
		}

		protected static void WriteToResponse(IHttpResponse response,
											  string content)
		{
			using (StreamWriter writer = new StreamWriter(response.OutputStream))
			{
				writer.Write(content);
			}
		}

		protected string GetPath(IHttpRequest request)
		{
			return pathParser.GetLocalPath(request);
		}

		private string ApplicationPath
		{
			get
			{
				return PathParser.GetApplicationPath(httpContext.Request);
			}
		}

		public string VccPath
		{
			get { return GetLocalPath(Constants.SvnVccPath); }
		}

		public string GetLocalPath(string href)
		{
			if (href.StartsWith("/") == false && ApplicationPath.EndsWith("/") == false)
				return ApplicationPath + "/" + href;
			if (href.StartsWith("/") && ApplicationPath.EndsWith("/"))
				return ApplicationPath + href.Substring(1);
			return ApplicationPath + href;
		}
	}
}