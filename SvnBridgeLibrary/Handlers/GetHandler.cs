using System;
using System.IO;
using System.Text;
using SvnBridge.Net;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class GetHandler : HttpContextHandlerBase
    {
        protected override void Handle(IHttpContext context, ISourceControlProvider sourceControlProvider)
        {
            IHttpRequest request = context.Request;
            IHttpResponse response = context.Response;

            string requestPath = GetPath(request);

            int itemVersion = int.Parse(requestPath.Split('/')[3]);
            string itemPath = Helper.Decode(requestPath.Substring(9 + itemVersion.ToString().Length));

            ItemMetaData item = sourceControlProvider.GetItems(itemVersion, itemPath, Recursion.None);
            string itemData = Encoding.Default.GetString(sourceControlProvider.ReadFile(item));

            SetResponseSettings(response, "text/plain", Encoding.Default, 200);

            using (StreamWriter writer = new StreamWriter(response.OutputStream))
            {
                writer.Write(itemData);
            }
        }
    }
}
