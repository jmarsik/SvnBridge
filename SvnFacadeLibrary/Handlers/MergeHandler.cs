using System.IO;
using System.Text;
using SvnBridge.Net;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class MergeHandler : HttpContextHandlerBase
    {
        protected override void Handle(IHttpContext context, ISourceControlProvider sourceControlProvider)
        {
            IHttpRequest request = context.Request;
            IHttpResponse response = context.Response;

            string path = GetPath(request);

            WebDavService webDavService = new WebDavService(sourceControlProvider);

            MergeData data = Helper.DeserializeXml<MergeData>(request.InputStream);

            SetResponseSettings(response, "text/xml", Encoding.UTF8, 200);

            response.AppendHeader("Cache-Control", "no-cache");

            response.SendChunked = true;

            using (StreamWriter output = new StreamWriter(response.OutputStream))
            {
                webDavService.Merge(data, path, output);
            }
        }
    }
}