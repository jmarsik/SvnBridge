using System;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;
using SvnBridge.Interfaces;
using SvnBridge.Net;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

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

			NetworkCredential credential = GetCredential(context);
			PerRequest.Items["credentials"] = credential;
			ISourceControlProvider sourceControlProvider =
				SourceControlProviderFactory.Create(tfsUrl, projectName, credential);
			Handle(context, sourceControlProvider);
		}

		public void Initialize(IHttpContext context, IPathParser parser)
		{
			this.httpContext = context;
			this.pathParser = parser;
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
		    string result;
			if (href.StartsWith("/") == false && ApplicationPath.EndsWith("/") == false)
			    result =  ApplicationPath + "/" + href;
			if (href.StartsWith("/") && ApplicationPath.EndsWith("/"))
			    result = ApplicationPath + href.Substring(1);
		    else
                result = ApplicationPath + href;
            if (result.EndsWith("/"))
                return result.Substring(0, result.Length - 1);
		    return result;
		}

		public string GetLocalPathFromUrl(string path)
		{
			return PathParser.GetLocalPath(httpContext.Request, path);
		}

        protected void WriteFileNotFoundResponse(IHttpRequest request, IHttpResponse response)
        {
            response.StatusCode = (int)HttpStatusCode.NotFound;
            response.ContentType = "text/html; charset=iso-8859-1";

            string responseContent =
                "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                "<html><head>\n" +
                "<title>404 Not Found</title>\n" +
                "</head><body>\n" +
                "<h1>Not Found</h1>\n" +
                "<p>The requested URL " + Helper.Decode(GetPath(request)) +
                " was not found on this server.</p>\n" +
                "<hr>\n" +
                "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + request.Url.Host +
                " Port " + request.Url.Port + "</address>\n" +
                "</body></html>\n";

            WriteToResponse(response, responseContent);
        }

	}
}