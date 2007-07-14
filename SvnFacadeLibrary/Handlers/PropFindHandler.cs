using System.IO;
using System.Net;
using System.Text;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class PropFindHandler : RequestHandlerBase
    {   
        public override string Method
        {
            get { return "propfind"; }
        }

        protected override void Handle(IHttpRequest request, ISourceControlProvider sourceControlProvider)
        {
            WebDavService webDavService = new WebDavService(sourceControlProvider);

            PropFindData propfind = Helper.DeserializeXml<PropFindData>(request.InputStream);

            try
            {
                SetResponseSettings(request, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 207);
                
                if (request.Headers["Label"] != null)
                {
                    request.AddHeader("Vary", "Label");
                }
                
                webDavService.PropFind(propfind, request.Path, request.Headers["Depth"], request.Headers["Label"], request.OutputStream);
            }
            catch (FileNotFoundException)
            {
                request.StatusCode = (int)HttpStatusCode.NotFound;
                request.ContentType = "text/html; charset=iso-8859-1";
                string server = request.Headers["Host"].Split(':')[0];
                string port = request.Headers["Host"].Split(':')[1];
                string response = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
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
