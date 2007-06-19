using System.Xml.Serialization;

namespace SvnBridge.Protocol
{
    [XmlRoot("activity-collection-set", Namespace = WebDav.Namespaces.DAV)]
    public class ActivityCollectionSetData
    {
        public ActivityCollectionSetData() {}

        [XmlElement("href", Namespace = WebDav.Namespaces.DAV)]
        public string href = "/!svn/act/";
    }
}