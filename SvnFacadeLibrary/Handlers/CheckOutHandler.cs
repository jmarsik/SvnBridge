using System.Text;
using SvnBridge.Exceptions;
using SvnBridge.Protocol;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class CheckOutHandler : RequestHandlerBase
    {
        public override string Method
        {
            get { return "checkout"; }
        }

        protected override void Handle(IHttpRequest request, WebDavService webDavService)
        {
            CheckoutData data = Helper.DeserializeXml<CheckoutData>(request.InputStream);

            try
            {
                string location = webDavService.CheckOut(data, request.Path, request.Headers["Host"]);
                string server = request.Headers["Host"].Split(':')[0];
                string port = request.Headers["Host"].Split(':')[1];
                SetResponseSettings(request, "text/html", Encoding.UTF8, 201);
                request.AddHeader("Cache-Control", "no-cache");
                request.AddHeader("Location", "http://" + request.Headers["Host"] + location);
                string response =
                    "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                    "<html><head>\n" +
                    "<title>201 Created</title>\n" +
                    "</head><body>\n" +
                    "<h1>Created</h1>\n" +
                    "<p>Checked-out resource " + location + " has been created.</p>\n" +
                    "<hr />\n" +
                    "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + server + " Port " + port + "</address>\n" +
                    "</body></html>\n";
                request.Write(response);
            }
            catch (ConflictException)
            {
                SetResponseSettings(request, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 409);
                string response =
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                    "<D:error xmlns:D=\"DAV:\" xmlns:m=\"http://apache.org/dav/xmlns\" xmlns:C=\"svn:\">\n" +
                    "<C:error/>\n" +
                    "<m:human-readable errcode=\"160024\">\n" +
                    "The version resource does not correspond to the resource within the transaction.  Either the requested version resource is out of date (needs to be updated), or the requested version resource is newer than the transaction root (restart the commit).\n" +
                    "</m:human-readable>\n" +
                    "</D:error>\n";
                request.Write(response);
            }
        }
    }
}
