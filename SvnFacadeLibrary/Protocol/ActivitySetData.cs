using System.Xml.Serialization;

namespace SvnBridge.Protocol
{
    [XmlRoot("activity-set", Namespace = WebDav.Namespaces.DAV)]
    public class ActivitySetData
    {
        public ActivitySetData() {}

        [XmlElement("href", Namespace = WebDav.Namespaces.DAV)]
        public string href = null;
    }
}