using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class ReportHandler : RequestHandlerBase
    {
        public override string Method
        {
            get { return "report"; }
        }

        protected override void Handle(IHttpRequest request, ISourceControlProvider sourceControlProvider)
        {
            WebDavService webDavService = new WebDavService(sourceControlProvider);

            using (XmlReader reader = XmlReader.Create(request.InputStream, Helper.InitializeNewXmlReaderSettings()))
            {
                reader.MoveToContent();
                if (reader.NamespaceURI == WebDav.Namespaces.SVN && reader.LocalName == "get-locks-report")
                {
                    //GetLocksReportData getlocksreport = Helper.DeserializeXml<GetLocksReportData>(reader);
                    SetResponseSettings(request, "text/xml", Encoding.UTF8, 200);
                    XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                    ns.Add("D", WebDav.Namespaces.DAV);
                    //SerializeResponse<GetLocksReportData>(getlocksreport, ns, _context.OutputStream);
                }
                else if (reader.NamespaceURI == WebDav.Namespaces.SVN && reader.LocalName == "update-report")
                {
                    UpdateReportData data = Helper.DeserializeXml<UpdateReportData>(reader);
                    SetResponseSettings(request, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 200);
                    SendChunked(request);
                    using (StreamWriter output = new StreamWriter(request.OutputStream))
                    {
                        webDavService.UpdateReport(data, output);
                    }
                }
                else if (reader.NamespaceURI == WebDav.Namespaces.SVN && reader.LocalName == "log-report")
                {
                    LogReportData data = Helper.DeserializeXml<LogReportData>(reader);
                    SetResponseSettings(request, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 200);
                    SendChunked(request);
                    using (StreamWriter output = new StreamWriter(request.OutputStream))
                    {
                        webDavService.LogReport(data, request.Path, output);
                    }
                }
            }
        }
    }
}
