using System.IO;
using System.Text;
using SvnBridge.Infrastructure;
using SvnBridge.Net;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class PropPatchHandler : HttpContextHandlerBase
    {
        protected override void Handle(IHttpContext context,
                                       ISourceControlProvider sourceControlProvider)
        {
            IHttpRequest request = context.Request;
            IHttpResponse response = context.Response;

            string path = GetPath(request);
            string correctXml;
            using(StreamReader sr = new StreamReader(request.InputStream))
            {
                correctXml = BrokenXml.Escape(sr.ReadToEnd());
            }
            PropertyUpdateData data = Helper.DeserializeXml<PropertyUpdateData>(correctXml);
            SetResponseSettings(response, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 207);

            using (StreamWriter output = new StreamWriter(response.OutputStream))
            {
                PropPatch(sourceControlProvider, data, path, output);
            }
        }

        private static void PropPatch(ISourceControlProvider sourceControlProvider,
                               PropertyUpdateData request,
                               string path,
                               TextWriter output)
        {
            string activityPath = path.Substring(10);
            if (activityPath.StartsWith("/"))
            {
                activityPath = activityPath.Substring(1);
            }

            string activityId = activityPath.Split('/')[0];
            switch (request.Set.Prop.Properties[0].LocalName)
            {
                case "log":
                    sourceControlProvider.SetActivityComment(activityId, request.Set.Prop.Properties[0].InnerText);
                    output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
                    output.Write(
                        "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/svn/\" xmlns:ns0=\"DAV:\">\n");
                    output.Write("<D:response>\n");
                    output.Write("<D:href>/" + path + "</D:href>\n");
                    output.Write("<D:propstat>\n");
                    output.Write("<D:prop>\n");
                    output.Write("<ns1:log/>\r\n");
                    output.Write("</D:prop>\n");
                    output.Write("<D:status>HTTP/1.1 200 OK</D:status>\n");
                    output.Write("</D:propstat>\n");
                    output.Write("</D:response>\n");
                    output.Write("</D:multistatus>\n");
                    break;
                default:
                    string itemPath = Helper.Decode(activityPath.Substring(activityPath.IndexOf('/')));
                    string propertyName = request.Set.Prop.Properties[0].LocalName;
                    if (request.Set.Prop.Properties[0].NamespaceURI == WebDav.Namespaces.TIGRISSVN)
                        propertyName = "svn:" + propertyName;
                    sourceControlProvider.SetProperty(activityId,
                                                      itemPath,
                                                      propertyName,
                                                      request.Set.Prop.Properties[0].InnerText);
                    output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
                    output.Write(
                        "<D:multistatus xmlns:D=\"DAV:\" xmlns:ns3=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:ns2=\"http://subversion.tigris.org/xmlns/custom/\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/svn/\" xmlns:ns0=\"DAV:\">\n");
                    output.Write("<D:response>\n");
                    output.Write("<D:href>/" + Helper.Encode(path) + "</D:href>\n");
                    output.Write("<D:propstat>\n");
                    output.Write("<D:prop>\n");
                    output.Write("<ns1:" + request.Set.Prop.Properties[0].LocalName + "/>\r\n");
                    output.Write("</D:prop>\n");
                    output.Write("<D:status>HTTP/1.1 200 OK</D:status>\n");
                    output.Write("</D:propstat>\n");
                    output.Write("</D:response>\n");
                    output.Write("</D:multistatus>\n");
                    break;
            }
        }
    }
}