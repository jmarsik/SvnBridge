using System.IO;
using System.Text;
using SvnBridge.Net;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;
using SvnBridge.Utility;
using CodePlex.TfsLibrary.RepositoryWebSvc;

namespace SvnBridge.Handlers
{
    public class MergeHandler : HttpContextHandlerBase
    {
        protected override void Handle(IHttpContext context, ISourceControlProvider sourceControlProvider)
        {
            IHttpRequest request = context.Request;
            IHttpResponse response = context.Response;

            string path = GetPath(request);
            MergeData data = Helper.DeserializeXml<MergeData>(request.InputStream);
            SetResponseSettings(response, "text/xml", Encoding.UTF8, 200);
            response.AppendHeader("Cache-Control", "no-cache");
            response.SendChunked = true;

            using (StreamWriter output = new StreamWriter(response.OutputStream))
            {
                Merge(sourceControlProvider, data, path, output);
            }
        }

        private void Merge(ISourceControlProvider sourceControlProvider, MergeData request, string path, StreamWriter output)
        {
            string activityId = request.Source.Href.Substring(10);
            MergeActivityResponse mergeResponse = sourceControlProvider.MergeActivity(activityId);

            output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
            output.Write("<D:merge-response xmlns:D=\"DAV:\">\n");
            output.Write("<D:updated-set>\n");
            output.Write("<D:response>\n");
            output.Write("<D:href>" + WebDavService.VccPath + "</D:href>\n");
            output.Write("<D:propstat><D:prop>\n");
            output.Write("<D:resourcetype><D:baseline/></D:resourcetype>\n");
            output.Write("\n");
            output.Write("<D:version-name>" + mergeResponse.Version.ToString() + "</D:version-name>\n");
            output.Write("<D:creationdate>" + WebDavService.FormatDate(mergeResponse.CreationDate) + "</D:creationdate>\n");
            output.Write("<D:creator-displayname>" + mergeResponse.Creator + "</D:creator-displayname>\n");
            output.Write("</D:prop>\n");
            output.Write("<D:status>HTTP/1.1 200 OK</D:status>\n");
            output.Write("</D:propstat>\n");
            output.Write("</D:response>\n");

            foreach (MergeActivityResponseItem item in mergeResponse.Items)
            {
                output.Write("<D:response>\n");
                output.Write("<D:href>" + Helper.Encode(item.Path) + "</D:href>\n");
                output.Write("<D:propstat><D:prop>\n");
                if (item.Type == ItemType.Folder)
                {
                    output.Write("<D:resourcetype><D:collection/></D:resourcetype>\n");
                }
                else
                {
                    output.Write("<D:resourcetype/>\n");
                }
                output.Write("<D:checked-in><D:href>/!svn/ver/" + mergeResponse.Version.ToString() + Helper.Encode(item.Path) + "</D:href></D:checked-in>\n");
                output.Write("</D:prop>\n");
                output.Write("<D:status>HTTP/1.1 200 OK</D:status>\n");
                output.Write("</D:propstat>\n");
                output.Write("</D:response>\n");
            }
            output.Write("</D:updated-set>\n");
            output.Write("</D:merge-response>\n");
        }
    }
}