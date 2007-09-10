using System.Xml;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Nodes
{
    public class BcFileNode : INode
    {
        readonly int requestVersion;
        readonly FileNode node;
        readonly ItemMetaData item;

        public BcFileNode(int requestVersion, ItemMetaData item, ISourceControlProvider sourceControlProvider)
        {
            this.requestVersion = requestVersion;
            this.item = item;
            node = new FileNode(item, sourceControlProvider);
            
        }

        public string Href()
        {
            string path = item.Name;

            if (!path.StartsWith("/"))
                path = "/" + path;

            string href = "/!svn/bc/" + requestVersion + path;

            if (item.ItemType == ItemType.Folder && ((href.Length == 0) || (href[href.Length - 1] != '/')))
                href += "/";

            return Helper.Encode(href);
        }

        public string GetProperty(XmlElement property)
        {
            return node.GetProperty(property);
        }
    }
}