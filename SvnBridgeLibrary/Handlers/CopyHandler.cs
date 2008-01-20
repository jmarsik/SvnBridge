using System.Text;
using SvnBridge.Net;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class CopyHandler : HttpContextHandlerBase
    {
        protected override void Handle(IHttpContext context, ISourceControlProvider sourceControlProvider)
        {
            IHttpRequest request = context.Request;
            IHttpResponse response = context.Response;

            string path = GetPath(request);

            SetResponseSettings(response, "text/html", Encoding.UTF8, 201);

            string destination = Helper.DecodeC(request.Headers["Destination"]);
            destination = destination.Substring(destination.IndexOf("/", destination.IndexOf("://") + 3));

            string activityId = request.Headers["Destination"].Split('/')[6];
            path = path.Substring(path.IndexOf('/', 9));
            string targetPath = destination.Substring(destination.IndexOf('/', 12));
            sourceControlProvider.CopyItem(activityId, path, targetPath);

            response.AppendHeader("Location", Helper.DecodeC(request.Headers["Destination"]));

            string responseContent =
                "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                "<html><head>\n" +
                "<title>201 Created</title>\n" +
                "</head><body>\n" +
                "<h1>Created</h1>\n" +
                "<p>Destination " + Helper.EncodeB(destination) + " has been created.</p>\n" +
                "<hr />\n" +
                "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + request.Url.Host + " Port " + request.Url.Port + "</address>\n" +
                "</body></html>\n";

            WriteToResponse(response, responseContent);
        }
    }
}