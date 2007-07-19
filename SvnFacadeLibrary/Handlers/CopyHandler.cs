using System;
using System.Text;
using SvnBridge.Utility;
using SvnBridge.SourceControl;

namespace SvnBridge.Handlers
{
    public class CopyHandler : RequestHandlerBase
    {
        public override string Method
        {
            get { return "copy"; }
        }

        protected override void Handle(IHttpRequest request, ISourceControlProvider sourceControlProvider)
        {
            SetResponseSettings(request, "text/html", Encoding.UTF8, 201);

            string server = request.Headers["Host"].Split(':')[0];
            string port = request.Headers["Host"].Split(':')[1];
            string destination = request.Headers["Destination"];
            destination = destination.Substring(destination.IndexOf("/", destination.IndexOf("://") + 3));

            string activityId = request.Headers["Destination"].Split('/')[6];
            string path = request.Path.Substring(request.Path.IndexOf('/', 9));
            string targetPath = destination.Substring(destination.IndexOf('/', 12));
            sourceControlProvider.CopyItem(activityId, path, targetPath);

            request.AddHeader("Location", request.Headers["Destination"]);

            string response =
                "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                "<html><head>\n" +
                "<title>201 Created</title>\n" +
                "</head><body>\n" +
                "<h1>Created</h1>\n" +
                "<p>Destination " + destination + " has been created.</p>\n" +
                "<hr />\n" +
                "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + server + " Port " + port + "</address>\n" +
                "</body></html>\n";

            request.Write(response);
        }
    }
}
