using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using CodePlex.TfsLibrary;
using SvnBridge.Handlers;

namespace SvnBridge.Net
{
    public class HttpContextDispatcher
    {
        private readonly Dictionary<string, IHttpContextHandler> handlers;
        private string tfsServerUrl;

        public HttpContextDispatcher()
        {
            handlers = new Dictionary<string, IHttpContextHandler>();

            RegisterHandler<CheckOutHandler>();
            RegisterHandler<CopyHandler>();
            RegisterHandler<DeleteHandler>();
            RegisterHandler<MergeHandler>();
            RegisterHandler<MkActivityHandler>();
            RegisterHandler<MkColHandler>();
            RegisterHandler<OptionsHandler>();
            RegisterHandler<PropFindHandler>();
            RegisterHandler<PropPatchHandler>();
            RegisterHandler<PutHandler>();
            RegisterHandler<ReportHandler>();
        }

        public string TfsServerUrl
        {
            get { return tfsServerUrl; }
            set { tfsServerUrl = value; }
        }

        public void Dispatch(IHttpContext connection)
        {
            if (string.IsNullOrEmpty(TfsServerUrl))
                throw new InvalidOperationException("A TFS server URL must be specified before connections can be dispatched.");

            string httpMethod = connection.Request.HttpMethod.ToLowerInvariant();

            IHttpContextHandler handler;

            if (handlers.TryGetValue(httpMethod, out handler))
            {
                try
                {
                    handler.Handle(connection, tfsServerUrl);
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
            }
            else
                SendUnsupportedMethodResponse(connection);
        }

        public void RegisterHandler<THandler>()
            where THandler : IHttpContextHandler, new()
        {
            IHttpContextHandler handler = new THandler();

            if (!handlers.ContainsKey(handler.MethodToHandle))
                handlers.Add(handler.MethodToHandle, handler);
        }

        private static void SendUnauthorizedResponse(IHttpContext connection)
        {
            IHttpResponse response = connection.Response;

            response.StatusCode = (int) HttpStatusCode.Unauthorized;
            response.ContentType = "text/html; charset=iso-8859-1";

            response.Headers.Remove("DAV");
            response.Headers.Remove("MS-Author-Via");
            response.Headers.Remove("Allow");

            response.Headers.Add("WWW-Authenticate", "Basic realm=\"CodePlex Subversion Repository\"");

            string[] hostParts = connection.Request.Headers["Host"].Split(':');
            string server = hostParts[0];
            string port = hostParts[1];

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
                             "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + server + " Port " + port +
                             "</address>\n" +
                             "</body></html>\n";

            byte[] buffer = Encoding.UTF8.GetBytes(content);

            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }

        private static void SendUnsupportedMethodResponse(IHttpContext connection)
        {
            IHttpResponse response = connection.Response;

            response.StatusCode = (int) HttpStatusCode.MethodNotAllowed;

            response.ContentType = "text/html";

            response.Headers.Add("Allow", "PROPFIND, REPORT, OPTIONS, MKACTIVITY, CHECKOUT, PROPPATCH, PUT, MERGE, DELETE, MKCOL");

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

            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
    }
}