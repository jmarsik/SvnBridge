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
        public void Handle(IHttpContext context, string tfsServerUrl)
        {
            ISourceControlProvider sourceControlProvider = SourceControlProviderFactory.Create(tfsServerUrl, GetCredential(context));

            Handle(context, sourceControlProvider);
        }

        protected abstract void Handle(IHttpContext context, ISourceControlProvider sourceControlProvider);

        private static NetworkCredential GetCredential(IHttpContext context)
        {
            NetworkCredential credential = null;

            if (context.User != null)
            {
                HttpListenerBasicIdentity identity = (HttpListenerBasicIdentity) context.User.Identity;

                string username = identity.Name;

                if (identity.Name.Contains(@"\"))
                {
                    string domain = username.Substring(0, username.IndexOf('\\'));
                    username = username.Substring(username.IndexOf('\\') + 1);
                    credential = new NetworkCredential(username, identity.Password, domain);
                }
                else
                    credential = new NetworkCredential(username, identity.Password);
            }

            return credential;
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