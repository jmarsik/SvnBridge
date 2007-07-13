using System.Text;
using SvnBridge.Exceptions;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class MkColHandler : RequestHandlerBase
    {
        public override string Method
        {
            get { return "mkcol"; }
        }

        protected override void Handle(IHttpRequest request, WebDavService webDavService)
        {
            string path = Helper.Decode(request.Path);
            string server = request.Headers["Host"].Split(':')[0];
            string port = request.Headers["Host"].Split(':')[1];
            
            try
            {
                webDavService.MkCol(request.Path, request.Headers["Host"]);
                
                SetResponseSettings(request, "text/html", Encoding.UTF8, 201);

                request.AddHeader("Location", "http://" + request.Headers["Host"] + path);
                
                string response = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                                  "<html><head>\n" +
                                  "<title>201 Created</title>\n" +
                                  "</head><body>\n" +
                                  "<h1>Created</h1>\n" +
                                  "<p>Collection " + path + " has been created.</p>\n" +
                                  "<hr />\n" +
                                  "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + server + " Port " + port + "</address>\n" +
                                  "</body></html>\n";

                request.Write(response);
            }
            catch (FolderAlreadyExistsException)
            {
                SetResponseSettings(request, "text/html; charset=iso-8859-1", Encoding.UTF8, 405);
                
                request.AddHeader("Allow", "TRACE");
                
                string response =
                    "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                    "<html><head>\n" +
                    "<title>405 Method Not Allowed</title>\n" +
                    "</head><body>\n" +
                    "<h1>Method Not Allowed</h1>\n" +
                    "<p>The requested method MKCOL is not allowed for the URL " + path + ".</p>\n" +
                    "<hr>\n" +
                    "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + server + " Port " + port + "</address>\n" +
                    "</body></html>\n";
                
                request.Write(response);
            }
        }
    }
}
