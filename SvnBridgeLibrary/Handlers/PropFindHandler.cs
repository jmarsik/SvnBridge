using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.Net;
using SvnBridge.Nodes;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class PropFindHandler : HttpContextHandlerBase
    {
        protected override void Handle(IHttpContext context, ISourceControlProvider sourceControlProvider)
        {
            IHttpRequest request = context.Request;
            IHttpResponse response = context.Response;

            string requestPath = GetPath(request);

            PropFindData propfind = Helper.DeserializeXml<PropFindData>(request.InputStream);

            string depthHeader = request.Headers["Depth"];
            string labelHeader = request.Headers["Label"];

            try
            {
                SetResponseSettings(response, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 207);

                if (request.Headers["Label"] != null)
                {
                    response.AppendHeader("Vary", "Label");
                }

                if (propfind.AllProp != null)
                {
                    HandleAllProp(sourceControlProvider, requestPath, response.OutputStream);
                }
                else if (propfind.Prop != null)
                {
                    HandleProp(sourceControlProvider, requestPath, depthHeader, labelHeader, propfind.Prop, response.OutputStream);
                }
                else
                {
                    throw new InvalidOperationException("Only <allprop> and <prop> are currently supported.");
                }
            }
            catch (FileNotFoundException)
            {
                WriteFileNotFoundResponse(request, response);
            }
        }

        private static void WriteFileNotFoundResponse(IHttpRequest request, IHttpResponse response)
        {
            response.StatusCode = (int) HttpStatusCode.NotFound;
            response.ContentType = "text/html; charset=iso-8859-1";

            string responseContent =
                "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                "<html><head>\n" +
                "<title>404 Not Found</title>\n" +
                "</head><body>\n" +
                "<h1>Not Found</h1>\n" +
                "<p>The requested URL " + Helper.Decode(GetPath(request)) +
                " was not found on this server.</p>\n" +
                "<hr>\n" +
                "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + request.Url.Host +
                " Port " + request.Url.Port + "</address>\n" +
                "</body></html>\n";

            WriteToResponse(response, responseContent);
        }

        private static FolderMetaData GetFolderInfo(ISourceControlProvider sourceControlProvider, string depth, string path, int? version)
        {
            if (depth == "0")
            {
                FolderMetaData folderInfo = new FolderMetaData();
                ItemMetaData item = GetItems(sourceControlProvider, version.HasValue ? version.Value : -1, path, Recursion.None);
                folderInfo.Items.Add(item);
                return folderInfo;
            }
            else if (depth == "1")
            {
                return (FolderMetaData)GetItems(sourceControlProvider, version.Value, path, Recursion.OneLevel);
            }
            else
                throw new InvalidOperationException(String.Format("Depth not supported: {0}", depth));
        }

        private static ItemMetaData GetItems(ISourceControlProvider sourceControlProvider, int version, string path, Recursion recursion)
        {
            // Make sure path is decoded
            return sourceControlProvider.GetItems(version, Helper.Decode(path), recursion);
        }

        private static void HandleAllProp(ISourceControlProvider sourceControlProvider, string requestPath, Stream outputStream)
        {
            string revision = requestPath.Split('/')[3];
            string path = requestPath.Substring(9 + revision.Length);

            ItemMetaData item = GetItems(sourceControlProvider, int.Parse(revision), path, Recursion.None);

            using (StreamWriter writer = new StreamWriter(outputStream))
            {
                if (item.ItemType == ItemType.Folder)
                    WriteAllPropForFolder(writer, requestPath, item);
                else
                    WriteAllPropForItem(writer, requestPath, item, sourceControlProvider.ReadFile(item));
            }
        }

        private static void WriteAllPropForFolder(TextWriter writer, string requestPath, ItemMetaData item)
        {
            writer.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
            writer.Write("<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n");
            writer.Write("<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n");
            writer.Write("<D:href>" + requestPath + "</D:href>\n");
            writer.Write("<D:propstat>\n");
            writer.Write("<D:prop>\n");
            writer.Write("<lp1:getcontenttype>text/html; charset=UTF-8</lp1:getcontenttype>\n");
            writer.Write("<lp1:getetag>W/\"" + item.Revision + "//" + item.Name + "\"</lp1:getetag>\n");
            writer.Write("<lp1:creationdate>" + item.LastModifiedDate.ToUniversalTime().ToString("o") + "</lp1:creationdate>\n");
            writer.Write("<lp1:getlastmodified>" + item.LastModifiedDate.ToUniversalTime().ToString("R") + "</lp1:getlastmodified>\n");
            writer.Write("<lp1:checked-in><D:href>/!svn/ver/" + item.Revision + "/" + Helper.Encode(item.Name) + "</D:href></lp1:checked-in>\n");
            writer.Write("<lp1:version-controlled-configuration><D:href>" + Constants.VccPath + "</D:href></lp1:version-controlled-configuration>\n");
            writer.Write("<lp1:version-name>" + item.Revision + "</lp1:version-name>\n");
            writer.Write("<lp1:creator-displayname>" + item.Author + "</lp1:creator-displayname>\n");
            writer.Write("<lp2:baseline-relative-path>" + item.Name + "</lp2:baseline-relative-path>\n");
            writer.Write("<lp2:repository-uuid>" + Constants.RepositoryUuid + "</lp2:repository-uuid>\n");
            writer.Write("<lp2:deadprop-count>0</lp2:deadprop-count>\n");
            writer.Write("<lp1:resourcetype><D:collection/></lp1:resourcetype>\n");
            writer.Write("<D:lockdiscovery/>\n");
            writer.Write("</D:prop>\n");
            writer.Write("<D:status>HTTP/1.1 200 OK</D:status>\n");
            writer.Write("</D:propstat>\n");
            writer.Write("</D:response>\n");
            writer.Write("</D:multistatus>\n");
        }

        private static void WriteAllPropForItem(TextWriter writer, string requestPath, ItemMetaData item, byte[] itemData)
        {
            writer.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
            writer.Write("<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n");
            writer.Write("<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n");
            writer.Write("<D:href>" + requestPath + "</D:href>\n");
            writer.Write("<D:propstat>\n");
            writer.Write("<D:prop>\n");
            writer.Write("<lp1:getcontenttype>text/plain</lp1:getcontenttype>\n");
            writer.Write("<lp1:getetag>W/\"" + item.Revision + "//" + item.Name + "\"</lp1:getetag>\n");
            writer.Write("<lp1:creationdate>" + item.LastModifiedDate.ToUniversalTime().ToString("o") + "</lp1:creationdate>\n");
            writer.Write("<lp1:getlastmodified>" + item.LastModifiedDate.ToUniversalTime().ToString("R") + "</lp1:getlastmodified>\n");
            writer.Write("<lp1:checked-in><D:href>/!svn/ver/" + item.Revision + "/" + Helper.Encode(item.Name) + "</D:href></lp1:checked-in>\n");
            writer.Write("<lp1:version-controlled-configuration><D:href>" + Constants.VccPath + "</D:href></lp1:version-controlled-configuration>\n");
            writer.Write("<lp1:version-name>" + item.Revision + "</lp1:version-name>\n");
            writer.Write("<lp1:creator-displayname>" + item.Author + "</lp1:creator-displayname>\n");
            writer.Write("<lp2:baseline-relative-path>" + item.Name + "</lp2:baseline-relative-path>\n");
            writer.Write("<lp2:md5-checksum>" + Helper.GetMd5Checksum(itemData) + "</lp2:md5-checksum>\n");
            writer.Write("<lp2:repository-uuid>" + Constants.RepositoryUuid + "</lp2:repository-uuid>\n");
            writer.Write("<lp2:deadprop-count>0</lp2:deadprop-count>\n");
            writer.Write("<lp1:resourcetype/>\n");
            writer.Write("<D:supportedlock>\n");
            writer.Write("<D:lockentry>\n");
            writer.Write("<D:lockscope><D:exclusive/></D:lockscope>\n");
            writer.Write("<D:locktype><D:write/></D:locktype>\n");
            writer.Write("</D:lockentry>\n");
            writer.Write("</D:supportedlock>\n");
            writer.Write("<D:lockdiscovery/>\n");
            writer.Write("</D:prop>\n");
            writer.Write("<D:status>HTTP/1.1 200 OK</D:status>\n");
            writer.Write("</D:propstat>\n");
            writer.Write("</D:response>\n");
            writer.Write("</D:multistatus>\n");
        }

        private static void HandleProp(
            ISourceControlProvider sourceControlProvider,
            string requestPath,
            string depthHeader,
            string labelHeader,
            PropData data,
            Stream outputStream)
        {
            if (requestPath == "/!svn/vcc/default")
                WriteVccResponse(sourceControlProvider, requestPath, labelHeader, data, outputStream);
            else if (requestPath.StartsWith("/!svn/bln/"))
                WriteBlnResponse(requestPath, data, outputStream);
            else if (requestPath.StartsWith("/!svn/bc/"))
                WriteBcResponse(sourceControlProvider, requestPath, depthHeader, data, outputStream);
            else
                WritePathResponse(sourceControlProvider, requestPath, depthHeader, data, outputStream);
        }

        private static void WriteVccResponse(ISourceControlProvider sourceControlProvider, string requestPath, string label, PropData data, Stream outputStream)
        {
            INode node = new SvnVccDefaultNode(sourceControlProvider, requestPath, label);

            using (StreamWriter writer = new StreamWriter(outputStream))
            {
                writer.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
                writer.Write("<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n");
                WriteProperties(node, data.Properties, writer);
                writer.Write("</D:multistatus>\n");
            }
        }

        private static void WriteBlnResponse(string requestPath, PropData data, Stream outputStream)
        {
            INode node = new SvnBlnNode(requestPath, int.Parse(requestPath.Substring(10)));

            using (StreamWriter writer = new StreamWriter(outputStream))
            {
                writer.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
                writer.Write("<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n");
                WriteProperties(node, data.Properties, writer);
                writer.Write("</D:multistatus>\n");
            }
        }

        private static void WriteBcResponse(ISourceControlProvider sourceControlProvider, string requestPath, string depthHeader, PropData data, Stream outputStream)
        {
            int version = int.Parse(requestPath.Split('/')[3]);
            string path = requestPath.Substring(9 + version.ToString().Length);

            if (!sourceControlProvider.ItemExists(Helper.Decode(path), version))
                throw new FileNotFoundException();

            FolderMetaData folderInfo = GetFolderInfo(sourceControlProvider, depthHeader, path, version);

            using (StreamWriter writer = new StreamWriter(outputStream))
            {
                writer.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");

                WriteMultiStatusStart(writer, data.Properties.Count > 1);


                if (depthHeader == "1")
                {
                    INode node = new BcFileNode(version, folderInfo, sourceControlProvider);

                    WriteProperties(node, data.Properties, writer, folderInfo.ItemType == ItemType.Folder);
                }

                foreach (ItemMetaData item in folderInfo.Items)
                {
                    INode node = new BcFileNode(version, item, sourceControlProvider);

                    WriteProperties(node, data.Properties, writer, item.ItemType == ItemType.Folder);
                }

                writer.Write("</D:multistatus>\n");
            }
        }

        private static void WritePathResponse(ISourceControlProvider sourceControlProvider, string requestPath, string depth, PropData data, Stream outputStream)
        {
            if (!sourceControlProvider.ItemExists(Helper.Decode(requestPath), -1))
                throw new FileNotFoundException();

            FolderMetaData folderInfo = GetFolderInfo(sourceControlProvider, depth, requestPath, null);

            using (StreamWriter writer = new StreamWriter(outputStream))
            {
                writer.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");

                WriteMultiStatusStart(writer, data.Properties.Count > 1);

                foreach (ItemMetaData item in folderInfo.Items)
                {
                    INode node = new FileNode(item, sourceControlProvider);

                    WriteProperties(node, data.Properties, writer, item.ItemType == ItemType.Folder);
                }

                writer.Write("</D:multistatus>\n");
            }
        }

        private static void WriteMultiStatusStart(TextWriter writer, bool hasMultipleProperties)
        {
            if (hasMultipleProperties)
                writer.Write("<D:multistatus xmlns:D=\"DAV:\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:ns0=\"DAV:\">\n");
            else
                writer.Write("<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n");
        }

        private static void WriteProperties(INode node, IEnumerable<XmlElement> properties, TextWriter output)
        {
            WriteProperties(node, properties, output, false);
        }

        private static void WriteProperties(INode node, IEnumerable<XmlElement> properties, TextWriter output, bool isFolder)
        {
            bool writeGetContentLengthForFolder = isFolder && PropertiesContains(properties, "getcontentlength");

            output.Write("<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\"");
            if (writeGetContentLengthForFolder)
                output.Write(" xmlns:g0=\"DAV:\"");
            output.Write(">\n");
            output.Write("<D:href>" + node.Href() + "</D:href>\n");

            XmlDocument doc = new XmlDocument();
            List<string> propertyResults = new List<string>();

            foreach (XmlElement prop in properties)
            {
                XmlElement property = doc.CreateElement(prop.LocalName, prop.NamespaceURI);
                if (!(isFolder && prop.LocalName == "getcontentlength"))
                    propertyResults.Add(node.GetProperty(property));
            }

            output.Write("<D:propstat>\n");
            output.Write("<D:prop>\n");
            foreach (string propertyResult in propertyResults)
            {
                output.Write(propertyResult + "\n");
            }
            output.Write("</D:prop>\n");
            output.Write("<D:status>HTTP/1.1 200 OK</D:status>\n");
            output.Write("</D:propstat>\n");

            if (writeGetContentLengthForFolder)
            {
                output.Write("<D:propstat>\n");
                output.Write("<D:prop>\n");
                output.Write("<g0:getcontentlength/>\n");
                output.Write("</D:prop>\n");
                output.Write("<D:status>HTTP/1.1 404 Not Found</D:status>\n");
                output.Write("</D:propstat>\n");
            }

            output.Write("</D:response>\n");
        }

        private static bool PropertiesContains(IEnumerable<XmlElement> properties, string propertyName)
        {
            foreach (XmlElement property in properties)
            {
                if (property.LocalName == propertyName)
                    return true;
            }

            return false;
        }
    }
}