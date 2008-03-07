using System.Text;
using SvnBridge.Exceptions;
using SvnBridge.Net;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class CheckOutHandler : HttpContextHandlerBase
    {
        protected override void Handle(IHttpContext context,
                                       ISourceControlProvider sourceControlProvider)
        {
            IHttpRequest request = context.Request;
            IHttpResponse response = context.Response;

            string path = GetPath(request);
            CheckoutData data = Helper.DeserializeXml<CheckoutData>(request.InputStream);

            try
            {
                string location = CheckOut(sourceControlProvider, data, path, request.Headers["Host"]);
                SetResponseSettings(response, "text/html", Encoding.UTF8, 201);
                response.AppendHeader("Cache-Control", "no-cache");
                response.AppendHeader("Location", "http://" + request.Headers["Host"] + Helper.EncodeC(location));
                string responseContent =
                    "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                    "<html><head>\n" +
                    "<title>201 Created</title>\n" +
                    "</head><body>\n" +
                    "<h1>Created</h1>\n" +
                    "<p>Checked-out resource " + Helper.Encode(location, true) + " has been created.</p>\n" +
                    "<hr />\n" +
                    "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + request.Url.Host + " Port " +
                    request.Url.Port + "</address>\n" +
                    "</body></html>\n";
                WriteToResponse(response, responseContent);
            }
            catch (ConflictException)
            {
                SetResponseSettings(response, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 409);
                string responseContent =
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                    "<D:error xmlns:D=\"DAV:\" xmlns:m=\"http://apache.org/dav/xmlns\" xmlns:C=\"svn:\">\n" +
                    "<C:error/>\n" +
                    "<m:human-readable errcode=\"160024\">\n" +
                    "The version resource does not correspond to the resource within the transaction.  Either the requested version resource is out of date (needs to be updated), or the requested version resource is newer than the transaction root (restart the commit).\n" +
                    "</m:human-readable>\n" +
                    "</D:error>\n";
                WriteToResponse(response, responseContent);
            }
        }

        private string CheckOut(ISourceControlProvider sourceControlProvider,
                                CheckoutData request,
                                string path,
                                string host)
        {
            string location = null;
            string activityId = request.ActivitySet.href.Split('/')[3];

            switch (path.Split('/')[2])
            {
                case "bln":
                    location = ApplicationPath + "//!svn/wbl/" + activityId + path.Substring(9);
                    break;
                case "ver":
                    string itemPath = path.Substring(path.IndexOf('/', 10));
                    int version = int.Parse(path.Split('/')[3]);
                    location = ApplicationPath + "//!svn/wrk/" + activityId + itemPath;
                    ItemMetaData item = sourceControlProvider.GetItems(-1, Helper.Decode(itemPath), Recursion.None);
                    if (item.Revision > version)
                    {
                        throw new ConflictException();
                    }
                    break;
            }
            return location;
        }
    }
}