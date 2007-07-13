using System.IO;
using System.Text;
using SvnBridge.Protocol;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class MergeHandler : RequestHandlerBase
    {
        public override string Method
        {
            get { return "merge"; }
        }

        protected override void Handle(IHttpRequest request, WebDavService webDavService)
        {
            MergeData data = Helper.DeserializeXml<MergeData>(request.InputStream);

            SetResponseSettings(request, "text/xml", Encoding.UTF8, 200);

            request.AddHeader("Cache-Control", "no-cache");
            
            SendChunked(request);
            
            using (StreamWriter output = new StreamWriter(request.OutputStream))
            {
                webDavService.Merge(data, request.Path, output);
            }
        }
    }
}
