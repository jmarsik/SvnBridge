using System.IO;
using System.Net;
using System.Text;
using SvnBridge.Net;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class PropFindHandler : HttpContextHandlerBase
    {
        protected override void Handle(IHttpContext context, ISourceControlProvider sourceControlProvider)
        {
            IHttpRequest request = context.Request;
            IHttpResponse response = context.Response;

            string path = GetPath(request);

            WebDavService webDavService = new WebDavService(sourceControlProvider);

            PropFindData propfind = Helper.DeserializeXml<PropFindData>(request.InputStream);

            try
            {
                SetResponseSettings(response, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 207);

                if (request.Headers["Label"] != null)
                {
                    response.AppendHeader("Vary", "Label");
                }

                webDavService.PropFind(propfind, path, request.Headers["Depth"], request.Headers["Label"], response.OutputStream);
            }
            catch (FileNotFoundException)
            {
                response.StatusCode = (int) HttpStatusCode.NotFound;
                response.ContentType = "text/html; charset=iso-8859-1";
                string server = request.Headers["Host"].Split(':')[0];
                string port = request.Headers["Host"].Split(':')[1];
                string responseContent = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                                         "<html><head>\n" +
                                         "<title>404 Not Found</title>\n" +
                                         "</head><body>\n" +
                                         "<h1>Not Found</h1>\n" +
                                         "<p>The requested URL " + Helper.Decode(path) + " was not found on this server.</p>\n" +
                                         "<hr>\n" +
                                         "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + server + " Port " + port + "</address>\n" +
                                         "</body></html>\n";

                WriteToResponse(response, responseContent);
            }
        }
    }
}