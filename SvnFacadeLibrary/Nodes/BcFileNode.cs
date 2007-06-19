using System.Text.RegularExpressions;
using System.Xml;
using SvnBridge.SourceControl;

namespace SvnBridge.Nodes
{
    public class BcFileNode : INode
    {
        FileNode node;
        string path;
        int version;
        string filePath;

        public BcFileNode(string vccPath,
                          string path,
                          ISourceControlProvider sourceControlProvider,
                          string repositoryUuid)
        {
            this.path = path;
            Match m = Regex.Match(path, @"/!svn/bc/(\d+)/?");
            version = int.Parse(m.Groups[1].Value);
            filePath = path.Substring(m.Groups[0].Value.Length);
            node = new FileNode(vccPath, filePath, sourceControlProvider, repositoryUuid);
        }

        public string Href()
        {
            string href = path;
            if ((href.Length == 0) || (href[href.Length - 1] != '/'))
                href += "/";

            return href;
        }

        public string GetProperty(XmlElement property)
        {
            return node.GetProperty(property);
        }
    }
}