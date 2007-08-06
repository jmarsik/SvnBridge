using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using SvnBridge.Net;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class ReportHandler : HttpContextHandlerBase
    {
        protected override void Handle(IHttpContext context, ISourceControlProvider sourceControlProvider)
        {
            IHttpRequest request = context.Request;
            IHttpResponse response = context.Response;

            string path = GetPath(request);

            WebDavService webDavService = new WebDavService(sourceControlProvider);

            using (XmlReader reader = XmlReader.Create(request.InputStream, Helper.InitializeNewXmlReaderSettings()))
            {
                reader.MoveToContent();
                if (reader.NamespaceURI == WebDav.Namespaces.SVN && reader.LocalName == "get-locks-report")
                {
                    //GetLocksReportData getlocksreport = Helper.DeserializeXml<GetLocksReportData>(reader);
                    SetResponseSettings(response, "text/xml", Encoding.UTF8, 200);
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add("D", WebDav.Namespaces.DAV);
                    //SerializeResponse<GetLocksReportData>(getlocksreport, ns, _context.OutputStream);
                }
                else if (reader.NamespaceURI == WebDav.Namespaces.SVN && reader.LocalName == "update-report")
                {
                    UpdateReportData data = Helper.DeserializeXml<UpdateReportData>(reader);
                    SetResponseSettings(response, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 200);
                    response.SendChunked = true;
                    using (StreamWriter output = new StreamWriter(response.OutputStream))
                    {
                        webDavService.UpdateReport(data, output);
                    }
                }
                else if (reader.NamespaceURI == WebDav.Namespaces.SVN && reader.LocalName == "log-report")
                {
                    LogReportData data = Helper.DeserializeXml<LogReportData>(reader);
                    SetResponseSettings(response, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 200);
                    response.SendChunked = true;
                    using (StreamWriter output = new StreamWriter(response.OutputStream))
                    {
                        webDavService.LogReport(data, path, output);
                    }
                }
            }
        }
    }
}