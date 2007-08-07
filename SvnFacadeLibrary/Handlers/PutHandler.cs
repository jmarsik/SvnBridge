using System.Text;
using SvnBridge.Net;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class PutHandler : HttpContextHandlerBase
    {
        protected override void Handle(IHttpContext context, ISourceControlProvider sourceControlProvider)
        {
            IHttpRequest request = context.Request;
            IHttpResponse response = context.Response;

            string path = GetPath(request);

            WebDavService webDavService = new WebDavService(sourceControlProvider);

            bool created = webDavService.Put(path, request.InputStream, request.Headers["X-SVN-Base-Fulltext-MD5"], request.Headers["X-SVN-Result-Fulltext-MD5"]);

            if (created)
            {
                SetResponseSettings(response, "text/html", Encoding.UTF8, 201);

                response.AppendHeader("Location", "http://" + request.Headers["Host"] + Helper.Decode(path));

                string responseContent = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                                         "<html><head>\n" +
                                         "<title>201 Created</title>\n" +
                                         "</head><body>\n" +
                                         "<h1>Created</h1>\n" +
                                         "<p>Resource " + Helper.Decode(path) + " has been created.</p>\n" +
                                         "<hr />\n" +
                                         "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + request.Url.Host + " Port " + request.Url.Port + "</address>\n" +
                                         "</body></html>\n";

                WriteToResponse(response, responseContent);
            }
            else
            {
                SetResponseSettings(response, "text/plain", Encoding.UTF8, 204);
            }
        }
    }
}