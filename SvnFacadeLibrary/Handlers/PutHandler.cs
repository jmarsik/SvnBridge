using System.Text;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class PutHandler : RequestHandlerBase
    {
        public override string Method
        {
            get { return "put"; }
        }

        protected override void Handle(IHttpRequest request, ISourceControlProvider sourceControlProvider)
        {
            WebDavService webDavService = new WebDavService(sourceControlProvider);
            
            webDavService.Put(request.Path, request.InputStream, request.Headers["X-SVN-Base-Fulltext-MD5"], request.Headers["X-SVN-Result-Fulltext-MD5"]);

            string server = request.Headers["Host"].Split(':')[0];
            string port = request.Headers["Host"].Split(':')[1];
            
            if (request.Headers["X-SVN-Base-Fulltext-MD5"] == null)
            {
                SetResponseSettings(request, "text/html", Encoding.UTF8, 201);
                
                request.AddHeader("Location", "http://" + request.Headers["Host"] + Helper.Decode(request.Path));
                
                string response = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                                  "<html><head>\n" +
                                  "<title>201 Created</title>\n" +
                                  "</head><body>\n" +
                                  "<h1>Created</h1>\n" +
                                  "<p>Resource " + Helper.Decode(request.Path) + " has been created.</p>\n" +
                                  "<hr />\n" +
                                  "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + server + " Port " + port + "</address>\n" +
                                  "</body></html>\n";
                
                request.Write(response);
            }
            else
            {
                SetResponseSettings(request, "text/plain", Encoding.UTF8, 204);
            }
        }
    }
}
