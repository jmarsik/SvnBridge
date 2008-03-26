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
        private string applicationPath;
    	private IPathParser parser;

        public void Handle(IHttpContext context, IPathParser pathParser)
        {
        	this.parser = pathParser;
        	string tfsUrl = pathParser.GetServerUrl(context.Request);
        	string projectName = pathParser.GetProjectName(context.Request);

            ISourceControlProvider sourceControlProvider =
                SourceControlProviderFactory.Create(tfsUrl, projectName, GetCredential(context));
            Handle(context, sourceControlProvider);
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
        	return parser.GetLocalPath(request);
        }

        public string ApplicationPath
        {
            get { return applicationPath; }
            set
            {
                applicationPath = value;
                if(applicationPath.EndsWith("/"))
                {
                    applicationPath = applicationPath.Substring(0, applicationPath.Length - 1);
                }
            }
        }

        public string VccPath
        {
            get
            {
                return ApplicationPath + Constants.SvnVccPath;
            }
        }
    }
}
