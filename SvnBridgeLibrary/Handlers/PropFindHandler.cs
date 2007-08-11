using System.IO;
using System.Net;
using System.Text;
using SvnBridge.Net;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;
using SvnBridge.Utility;
using SvnBridge.Nodes;
using System.Xml;
using System.Collections.Generic;

namespace SvnBridge.Handlers
{
    public class PropFindHandler : HttpContextHandlerBase
    {
        protected override void Handle(IHttpContext context, ISourceControlProvider sourceControlProvider)
        {
            IHttpRequest request = context.Request;
            IHttpResponse response = context.Response;

            string path = GetPath(request);
            PropFindData propfind = Helper.DeserializeXml<PropFindData>(request.InputStream);

            try
            {
                SetResponseSettings(response, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 207);

                if (request.Headers["Label"] != null)
                {
                    response.AppendHeader("Vary", "Label");
                }

                PropFind(sourceControlProvider, propfind, path, request.Headers["Depth"], request.Headers["Label"], response.OutputStream);
            }
            catch (FileNotFoundException)
            {
                response.StatusCode = (int) HttpStatusCode.NotFound;
                response.ContentType = "text/html; charset=iso-8859-1";

                string responseContent = "<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">\n" +
                                         "<html><head>\n" +
                                         "<title>404 Not Found</title>\n" +
                                         "</head><body>\n" +
                                         "<h1>Not Found</h1>\n" +
                                         "<p>The requested URL " + Helper.Decode(path) + " was not found on this server.</p>\n" +
                                         "<hr>\n" +
                                         "<address>Apache/2.0.59 (Win32) SVN/1.4.2 DAV/2 Server at " + request.Url.Host + " Port " + request.Url.Port + "</address>\n" +
                                         "</body></html>\n";

                WriteToResponse(response, responseContent);
            }
        }

        private void PropFind(ISourceControlProvider sourceControlProvider, PropFindData propfind, string path, string depth, string label, Stream outputStream)
        {
            if (path == "/!svn/vcc/default")
            {
                INode node = new SvnVccDefaultNode(sourceControlProvider, path, label);
                using (StreamWriter output = new StreamWriter(outputStream))
                {
                    output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
                    output.Write("<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n");
                    ProcessPropFind(node, path, propfind.Prop.Properties, output);
                    output.Write("</D:multistatus>\n");
                }
            }
            else if (path.StartsWith("/!svn/bln/"))
            {
                INode node = new SvnBlnNode(path, int.Parse(path.Substring(10)));
                using (StreamWriter output = new StreamWriter(outputStream))
                {
                    output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
                    output.Write("<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n");
                    ProcessPropFind(node, path, propfind.Prop.Properties, output);
                    output.Write("</D:multistatus>\n");
                }
            }
            else
            {
                if (path.StartsWith("/!svn/bc/"))
                {
                    string version = path.Split('/')[3];
                    if (!sourceControlProvider.ItemExists(Helper.Decode(path.Substring(9 + version.Length)), int.Parse(version)))
                    {
                        throw new FileNotFoundException();
                    }
                }
                else
                {
                    if (!sourceControlProvider.ItemExists(Helper.Decode(path)))
                    {
                        throw new FileNotFoundException();
                    }
                }

                FolderMetaData folderInfo = null;
                if (depth == "0")
                {
                    folderInfo = new FolderMetaData();
                    ItemMetaData item = new ItemMetaData();
                    item.Name = path;
                    folderInfo.Items.Add(item);
                }
                else
                {
                    folderInfo = (FolderMetaData)sourceControlProvider.GetItems(sourceControlProvider.GetLatestVersion(), path, Recursion.OneLevel);
                }

                using (StreamWriter output = new StreamWriter(outputStream))
                {
                    output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
                    if (propfind.Prop.Properties.Count > 1)
                    {
                        output.Write("<D:multistatus xmlns:D=\"DAV:\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:ns0=\"DAV:\">\n");
                    }
                    else
                    {
                        output.Write("<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n");
                    }
                    if (path.StartsWith("/!svn/bc/"))
                    {
                        foreach (ItemMetaData item in folderInfo.Items)
                        {
                            INode node = new BcFileNode(Constants.VccPath, item.Name, sourceControlProvider, Constants.RepositoryUuid);
                            ProcessPropFind(node, item.Name, propfind.Prop.Properties, output);
                        }
                    }
                    else
                    {
                        foreach (ItemMetaData item in folderInfo.Items)
                        {
                            INode node = new FileNode(Constants.VccPath, item.Name, sourceControlProvider, Constants.RepositoryUuid);
                            ProcessPropFind(node, item.Name, propfind.Prop.Properties, output);
                        }
                    }
                    output.Write("</D:multistatus>\n");
                }
            }
        }

        private void ProcessPropFind(INode node, string path, List<XmlElement> properties, StreamWriter output)
        {
            output.Write("<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n");
            output.Write("<D:href>" + node.Href() + "</D:href>\n");

            XmlDocument doc = new XmlDocument();
            List<string> propertyResults = new List<string>();

            foreach (XmlElement prop in properties)
            {
                XmlElement property = doc.CreateElement(prop.LocalName, prop.NamespaceURI);
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
            output.Write("</D:response>\n");
        }
    }
}