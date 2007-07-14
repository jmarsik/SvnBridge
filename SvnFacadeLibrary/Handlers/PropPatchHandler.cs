using System.IO;
using System.Text;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class PropPatchHandler : RequestHandlerBase
    {
        public override string Method
        {
            get { return "proppatch"; }
        }

        protected override void Handle(IHttpRequest request, ISourceControlProvider sourceControlProvider)
        {
            WebDavService webDavService = new WebDavService(sourceControlProvider);

            PropertyUpdateData data = Helper.DeserializeXml<PropertyUpdateData>(request.InputStream);
            
            SetResponseSettings(request, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 207);

            using (StreamWriter output = new StreamWriter(request.OutputStream))
            {
                webDavService.PropPatch(data, request.Path, output);
            }
        }
    }
}
