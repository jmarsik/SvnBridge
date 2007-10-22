using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using SvnBridge.Net;
using SvnBridge.SourceControl;

namespace SvnBridge.Handlers
{
    public abstract class HttpContextHandlerBase
    {
        public void Handle(IHttpContext context, string tfsUrl)
        {
            ISourceControlProvider sourceControlProvider = SourceControlProviderFactory.Create(tfsUrl, GetCredential(context));

            Handle(context, sourceControlProvider);
        }

        public virtual void Cancel() { }

        protected abstract void Handle(IHttpContext context, ISourceControlProvider sourceControlProvider);

        private static NetworkCredential GetCredential(IHttpContext context)
        {
            string authorizationHeader = context.Request.Headers["Authorization"];

            if (!string.IsNullOrEmpty(authorizationHeader))
            {
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
                    return new NetworkCredential(username, password);
            }
            else
                return null;
        }

        protected static void SetResponseSettings(IHttpResponse response, string contentType, Encoding contentEncoding, int status)
        {
            response.ContentType = contentType;
            response.ContentEncoding = contentEncoding;
            response.StatusCode = status;
        }

        protected static void WriteToResponse(IHttpResponse response, string content)
        {
            using (StreamWriter writer = new StreamWriter(response.OutputStream))
            {
                writer.Write(content);
            }
        }

        protected static string GetPath(IHttpRequest request)
        {
            return HttpUtility.UrlPathEncode(request.Url.LocalPath);
        }
    }
}