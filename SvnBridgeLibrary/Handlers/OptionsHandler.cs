using System.Text;
using SvnBridge.Net;
using SvnBridge.SourceControl;
using System.IO;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class OptionsHandler : HttpContextHandlerBase
    {
        protected override void Handle(IHttpContext context, ISourceControlProvider sourceControlProvider)
        {
            IHttpRequest request = context.Request;
            IHttpResponse response = context.Response;

            string path = GetPath(request);

            SetResponseSettings(response, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 200);
            response.AppendHeader("DAV", "1,2");
            response.AppendHeader("DAV", "version-control,checkout,working-resource");
            response.AppendHeader("DAV", "merge,baseline,activity,version-controlled-collection");
            response.AppendHeader("MS-Author-Via", "DAV");
            response.AppendHeader("Allow", "OPTIONS,GET,HEAD,POST,DELETE,TRACE,PROPFIND,PROPPATCH,COPY,MOVE,LOCK,UNLOCK,CHECKOUT");
            Options(sourceControlProvider, path, response.OutputStream);
        }

        private void Options(ISourceControlProvider sourceControlProvider, string path, Stream outputStream)
        {
            sourceControlProvider.ItemExists(Helper.Decode(path)); // Verify permissions to access
            using (StreamWriter output = new StreamWriter(outputStream))
            {
                output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
                output.Write("<D:options-response xmlns:D=\"DAV:\">\n");
                output.Write("<D:activity-collection-set><D:href>/!svn/act/</D:href></D:activity-collection-set></D:options-response>\n");
            }
        }
    }
}