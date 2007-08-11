using System.Xml.Serialization;

namespace SvnBridge.Protocol
{
    [XmlRoot("propertyupdate", Namespace = WebDav.Namespaces.DAV)]
    public class PropertyUpdateData
    {
        public PropertyUpdateData() {}

        [XmlElement("set", Namespace = WebDav.Namespaces.DAV)]
        public SetData Set = new SetData();
    }
}