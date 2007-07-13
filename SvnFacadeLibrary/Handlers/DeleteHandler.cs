using System;
using System.Text;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class DeleteHandler : RequestHandlerBase 
    {
        public override string Method
        {
            get { return "delete"; }
        }

        protected override void Handle(IHttpRequest request, WebDavService webDavService)
        {
            bool fileDeleted = webDavService.Delete(request.Path);

            if (fileDeleted)
            {
                SetResponseSettings(request, "text/plain", Encoding.UTF8, 204);
            }
            else
            {
                string server = request.Headers["Host"].Split(':')[0];
                string port = request.Headers["Host"].Split(':')[1];
                
                SetResponseSettings(request, "text/html; charset=iso-8859-1", Encoding.UTF8, 404);

                string response =
                    "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                    "<html><head>\n" +
                    "<title>404 Not Found</title>\n" +
                    "</head><body>\n" +
                    "<h1>Not Found</h1>\n" +
                    "<p>The requested URL " + Helper.Decode(request.Path) + " was not found on this server.</p>\n" +
                    "<hr>\n" +
                    "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + server + " Port " + port + "</address>\n" +
                    "</body></html>\n";
                
                request.Write(response);
            }
        }
    }
}
