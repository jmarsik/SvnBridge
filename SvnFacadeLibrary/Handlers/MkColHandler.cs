using System.Text;
using SvnBridge.Exceptions;
using SvnBridge.Utility;
using System.Text.RegularExpressions;
using SvnBridge.SourceControl;

namespace SvnBridge.Handlers
{
    public class MkColHandler : RequestHandlerBase
    {
        public override string Method
        {
            get { return "mkcol"; }
        }

        protected override void Handle(IHttpRequest request, ISourceControlProvider sourceControlProvider)
        {
            string path = Helper.Decode(request.Path);

            string[] hostParts = request.Headers["Host"].Split(':');
            string server = hostParts[0];
            string port = hostParts[1];
            
            try
            {
                MakeCollection(path, sourceControlProvider);

                SendCreatedResponse(request, path, server, port);

            }
            catch (FolderAlreadyExistsException)
            {
                SendFailureResponse(request, path, server, port);
            }
        }

        private void MakeCollection(string path, ISourceControlProvider sourceControlProvider)
        {
            Match match = Regex.Match(path, @"//!svn/wrk/([a-zA-Z0-9\-]+)/?");
            string folderPath = path.Substring(match.Groups[0].Value.Length - 1);
            string activityId = match.Groups[1].Value;
            sourceControlProvider.MakeCollection(activityId, Helper.Decode(folderPath));
        }

        private void SendCreatedResponse(IHttpRequest request, string path, string server, string port)
        {
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

        private void SendFailureResponse(IHttpRequest request, string path, string server, string port)
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
