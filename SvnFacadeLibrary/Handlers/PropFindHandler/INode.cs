using System.Xml;

namespace SvnBridge.Nodes
{
    public interface INode
    {
        string Href();
        string GetProperty(XmlElement property);
    }
}