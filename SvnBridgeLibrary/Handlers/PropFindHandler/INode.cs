using System.Xml;
using SvnBridge.Handlers;

namespace SvnBridge.Nodes
{
    public interface INode
    {
        string Href(HttpContextHandlerBase handler);
        string GetProperty(HttpContextHandlerBase handler, XmlElement property);
    }
}