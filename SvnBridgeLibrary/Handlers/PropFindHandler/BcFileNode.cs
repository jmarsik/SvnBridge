using System.Text.RegularExpressions;
using System.Xml;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Nodes
{
    public class BcFileNode : INode
    {
        readonly FileNode node;
        readonly int version;
        readonly string filePath;

        public BcFileNode(string vccPath,
                          string path,
                          int version,
                          ISourceControlProvider sourceControlProvider,
                          string repositoryUuid)
        {
            this.version = version;
            filePath = path;
            node = new FileNode(vccPath, filePath, sourceControlProvider, repositoryUuid, version);
        }

        public string Href()
        {
            string path = filePath;

            if (!path.StartsWith("/"))
                path = "/" + path; 

            string href = "/!svn/bc/" + version + path;
            if ((href.Length == 0) || (href[href.Length - 1] != '/'))
                href += "/";

            return Helper.Encode(href);
        }

        public string GetProperty(XmlElement property)
        {
            return node.GetProperty(property);
        }
    }
}