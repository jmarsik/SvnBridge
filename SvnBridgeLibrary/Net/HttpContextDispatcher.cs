using System;
using System.Net;
using System.Text;
using CodePlex.TfsLibrary;
using SvnBridge.Handlers;
using System.IO;

namespace SvnBridge.Net
{
    public class HttpContextDispatcher
    {
        private string tfsUrl;

        public HttpContextHandlerBase GetHandler(string httpMethod)
        {
            switch (httpMethod.ToLowerInvariant())
            {
                case "checkout": return new CheckOutHandler();
                case "copy": return new CopyHandler();
                case "delete": return new DeleteHandler();
                case "merge": return new MergeHandler();
                case "mkactivity": return new MkActivityHandler();
                case "mkcol": return new MkColHandler();
                case "options": return new OptionsHandler();
                case "propfind": return new PropFindHandler();
                case "proppatch": return new PropPatchHandler();
                case "put": return new PutHandler();
                case "report": return new ReportHandler();
                case "get": return new GetHandler();
                default: return null;
            }
        }

        public string TfsUrl
        {
            get { return tfsUrl; }
            set { tfsUrl = value; }
        }

        public void Dispatch(IHttpContext connection)
        {
            if (string.IsNullOrEmpty(TfsUrl))
                throw new InvalidOperationException("A TFS server URL must be specified before connections can be dispatched.");

            HttpContextHandlerBase handler = GetHandler(connection.Request.HttpMethod);

            if (handler != null)
            {
                try
                {
                    handler.Handle(connection, tfsUrl);
                }
                catch (WebException ex)
                {
                    HttpWebResponse response = ex.Response as HttpWebResponse;

                    if (response != null && response.StatusCode == HttpStatusCode.Unauthorized)
                        SendUnauthorizedResponse(connection);
                    else
                        throw;
                }
                catch (NetworkAccessDeniedException)
                {
                    SendUnauthorizedResponse(connection);
                }
                catch (IOException)
                {
                    // Error caused by client cancelling operation
                    handler.Cancel();
                }
            }
            else
                SendUnsupportedMethodResponse(connection);
        }

        private static void SendUnauthorizedResponse(IHttpContext connection)
        {
            IHttpRequest request = connection.Request;
            IHttpResponse response = connection.Response;

            response.ClearHeaders();

            response.StatusCode = (int) HttpStatusCode.Unauthorized;
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
                             "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + request.Url.Host + " Port " + request.Url.Port +
                             "</address>\n" +
                             "</body></html>\n";

            byte[] buffer = Encoding.UTF8.GetBytes(content);

            response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        private static void SendUnsupportedMethodResponse(IHttpContext connection)
        {
            IHttpResponse response = connection.Response;

            response.StatusCode = (int) HttpStatusCode.MethodNotAllowed;

            response.ContentType = "text/html";

            response.AppendHeader("Allow", "PROPFIND, REPORT, OPTIONS, MKACTIVITY, CHECKOUT, PROPPATCH, PUT, MERGE, DELETE, MKCOL");

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