using System.Text;
using SvnBridge.Net;
using SvnBridge.SourceControl;

namespace SvnBridge.Handlers
{
    public class MkActivityHandler : HttpContextHandlerBase
    {
        protected override void Handle(IHttpContext context, ISourceControlProvider sourceControlProvider)
        {
            IHttpRequest request = context.Request;
            IHttpResponse response = context.Response;

            string path = GetPath(request);

            WebDavService webDavService = new WebDavService(sourceControlProvider);

            webDavService.MkActivity(path);

            string server = request.Headers["Host"].Split(':')[0];
            string port = request.Headers["Host"].Split(':')[1];

            SetResponseSettings(response, "text/html", Encoding.UTF8, 201);

            response.Headers.Add("Cache-Control", "no-cache");
            response.Headers.Add("Location", "http://" + request.Headers["Host"] + path);
            response.Headers.Add("X-Pad", "avoid browser bug");

            string responseContent = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                                     "<html><head>\n" +
                                     "<title>201 Created</title>\n" +
                                     "</head><body>\n" +
                                     "<h1>Created</h1>\n" +
                                     "<p>Activity " + path + " has been created.</p>\n" +
                                     "<hr />\n" +
                                     "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + server + " Port " + port + "</address>\n" +
                                     "</body></html>\n";

            WriteToResponse(response, responseContent);
        }
    }
}