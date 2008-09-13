using System.Xml;
using SvnBridge.Handlers;

namespace SvnBridge.Nodes
{
    public interface INode
    {
        string Href(HandlerBase handler);
        string GetProperty(HandlerBase handler, XmlElement property);
    }
}