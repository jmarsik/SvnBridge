using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using CodePlex.TfsLibrary;
using SvnBridge.Handlers;
using SvnBridge.Handlers.Renderers;
using SvnBridge.Infrastructure;
using SvnBridge.Infrastructure.Statistics;
using SvnBridge.Interfaces;
using SvnBridge.SourceControl;

namespace SvnBridge.Net
{
    public class HttpContextDispatcher
    {
        private readonly IPathParser parser;
        private readonly IActionTracking actionTracking;

        public HttpContextDispatcher(IPathParser parser, IActionTracking actionTracking)
        {
            this.parser = parser;
            this.actionTracking = actionTracking;
        }

        public virtual HttpContextHandlerBase GetHandler(string httpMethod)
        {
            switch (httpMethod.ToLowerInvariant())
            {
                case "checkout":
                    return new CheckOutHandler();
                case "copy":
                    return new CopyHandler();
                case "delete":
                    return new DeleteHandler();
                case "merge":
                    return new MergeHandler();
                case "mkactivity":
                    return new MkActivityHandler();
                case "mkcol":
                    return new MkColHandler();
                case "options":
                    return new OptionsHandler();
                case "propfind":
                    return new PropFindHandler();
                case "proppatch":
                    return new PropPatchHandler();
                case "put":
                    return new PutHandler();
                case "report":
                    return new ReportHandler();
                case "get":
                    return new GetHandler();
                default:
                    return null;
            }
        }

        public void Dispatch(IHttpContext connection)
        {
            HttpContextHandlerBase handler = null;
            try
            {
                IHttpRequest request = connection.Request;
                if ("/!stats/request".Equals(request.LocalPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    new StatsRenderer(IoC.Resolve<IActionTracking>()).Render(connection);
                    return;
                }

                NetworkCredential credential = GetCredential(connection);
                string tfsUrl = parser.GetServerUrl(request, credential);
                if (string.IsNullOrEmpty(tfsUrl))
                {
                    throw new InvalidOperationException("A TFS server URL must be specified before connections can be dispatched.");
                }

                if (credential != null && tfsUrl.ToLowerInvariant().EndsWith("codeplex.com"))
                {
                    string username = credential.UserName;
                    string domain = credential.Domain;
                    if (!username.ToLowerInvariant().EndsWith("_cp"))
                    {
                        username += "_cp";
                    }
                    if (domain == "")
                    {
                        domain = "snd";
                    }
                    credential = new NetworkCredential(username, credential.Password, domain);
                }

                handler = GetHandler(connection.Request.HttpMethod);

                if (handler == null)
                {
                    actionTracking.Error();
                    SendUnsupportedMethodResponse(connection);
                    return;
                }

                try
                {
                    actionTracking.Request(handler);
                    handler.Handle(connection, parser, credential);
                }
                catch (TargetInvocationException e)
                {
                    ExceptionHelper.PreserveStackTrace(e.InnerException);
                    throw e.InnerException;
                }
            }
            catch (WebException ex)
            {
                actionTracking.Error();

                HttpWebResponse response = ex.Response as HttpWebResponse;

                if (response != null && response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SendUnauthorizedResponse(connection);
                }
                else
                {
                    throw;
                }
            }
            catch (NetworkAccessDeniedException)
            {
                SendUnauthorizedResponse(connection);
            }
            catch (IOException)
            {
                // Error caused by client cancelling operation
                if (handler != null)
                    handler.Cancel();
            }

        }

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


        private static void SendUnauthorizedResponse(IHttpContext connection)
        {
            IHttpRequest request = connection.Request;
            IHttpResponse response = connection.Response;

            response.ClearHeaders();

            response.StatusCode = (int)HttpStatusCode.Unauthorized;
            response.ContentType = "text/html; charset=iso-8859-1";

            response.AppendHeader("WWW-Authenticate", "Basic realm=\"CodePlex Subversion Repository\"");

            string content = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                             "<html><head>\n" +
                             "<title>401 Authorization Required</title>\n" +
                             "</head><body>\n" +
                             "<h1>Authorization Required</h1>\n" +
                             "<p>This server could not verify that you\n" +
                             "are authorized to access the document\n" +
                             "requested.  Either you supplied the wrong\n" +
                             "credentials (e.g., bad password), or your\n" +
                             "browser doesn't understand how to supply\n" +
                             "the credentials required.</p>\n" +
                             "<hr>\n" +
                             "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + request.Url.Host + " Port " +
                             request.Url.Port +
                             "</address>\n" +
                             "</body></html>\n";

            byte[] buffer = Encoding.UTF8.GetBytes(content);

            response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        private static void SendUnsupportedMethodResponse(IHttpContext connection)
        {
            IHttpResponse response = connection.Response;

            response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;

            response.ContentType = "text/html";

            response.AppendHeader("Allow",
                                  "PROPFIND, REPORT, OPTIONS, MKACTIVITY, CHECKOUT, PROPPATCH, PUT, MERGE, DELETE, MKCOL");

            string content =
                @"
                <html>
                    <head>
                        <title>405 Method Not Allowed</title>
                    </head>
                    <body>
                        <h1>The requested method is not supported.</h1>
                    </body>
                </html>";

            byte[] buffer = Encoding.UTF8.GetBytes(content);

            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
    }
}
