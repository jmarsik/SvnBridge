using System.Collections.Generic;
using System.Net;
using CodePlex.TfsLibrary;

namespace SvnBridge.Handlers
{
    public class RequestDispatcher :IRequestDispatcher
    {
        // Fields
        
        Dictionary<string, IRequestHandler> handlers;
        string tfsServer;
        
        // Lifetime

        public RequestDispatcher(string tfsServer)
        {
            handlers = new Dictionary<string, IRequestHandler>();
            this.tfsServer = tfsServer;
        }
        
        // Methods

        public void Dispatch(IHttpRequest request)
        {
            string method = request.HttpMethod.ToLowerInvariant();
            
            IRequestHandler handler;
            
            if (handlers.TryGetValue(method, out handler))
            {
                try
                {
                    handler.Handle(request, tfsServer);
                }
                catch (WebException ex)
                {
                    HttpWebResponse response = ex.Response as HttpWebResponse;

                    if (response != null && response.StatusCode == HttpStatusCode.Unauthorized)
                        SendUnauthorizedResponse(request);
                    else
                        throw;
                }
                catch (NetworkAccessDeniedException)
                {
                    SendUnauthorizedResponse(request);
                }
            }
            else
                SendUnsupportedMethodResponse(request);
        }

        public void RegisterHandler<THandler>() 
            where THandler : IRequestHandler, new()
        {
            IRequestHandler handler = new THandler();

            if (!handlers.ContainsKey(handler.Method))
                handlers.Add(handler.Method, handler);
        }

        private static void SendUnauthorizedResponse(IHttpRequest request)
        {
            request.StatusCode = (int)HttpStatusCode.Unauthorized;
            request.ContentType = "text/html; charset=iso-8859-1";
            
            request.RemoveHeader("DAV");
            request.RemoveHeader("MS-Author-Via");
            request.RemoveHeader("Allow");
            
            request.AddHeader("WWW-Authenticate", "Basic realm=\"CodePlex Subversion Repository\"");
            
            string server = request.Headers["Host"].Split(':')[0];
            string port = request.Headers["Host"].Split(':')[1];
            
            string response = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
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
                              "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + server + " Port " + port + "</address>\n" +
                              "</body></html>\n";
            
            request.Write(response);

            request.OutputStream.Flush();
        }
        
        private static void SendUnsupportedMethodResponse(IHttpRequest request)
        {
            request.StatusCode = 405;
            request.ContentType = "text/html";
            request.AddHeader("Allow", "PROPFIND, REPORT, OPTIONS, MKACTIVITY, CHECKOUT, PROPPATCH, PUT, MERGE, DELETE, MKCOL");
            request.Write(@"
                <html>
                    <head>
                        <title>405 Method Not Allowed</title>
                    </head>
                    <body>
                        <h1>The requested method is not supported.</h1>
                    </body>
                </html>");

            request.OutputStream.Flush();
        }
    }
}
