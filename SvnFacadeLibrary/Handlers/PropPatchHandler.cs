using System.IO;
using System.Text;
using SvnBridge.Net;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class PropPatchHandler : HttpContextHandlerBase
    {
        public override string MethodToHandle
        {
            get { return "proppatch"; }
        }

        protected override void Handle(IHttpContext context, ISourceControlProvider sourceControlProvider)
        {
            IHttpRequest request = context.Request;
            IHttpResponse response = context.Response;

            string path = GetPath(request);

            WebDavService webDavService = new WebDavService(sourceControlProvider);

            PropertyUpdateData data = Helper.DeserializeXml<PropertyUpdateData>(request.InputStream);

            SetResponseSettings(response, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 207);

            using (StreamWriter output = new StreamWriter(response.OutputStream))
            {
                webDavService.PropPatch(data, path, output);
            }
        }
    }
}