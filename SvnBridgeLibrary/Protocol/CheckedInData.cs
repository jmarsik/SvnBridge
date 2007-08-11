using System.Xml.Serialization;

namespace SvnBridge.Protocol
{
    public class CheckedInData
    {
        public CheckedInData() {}

        [XmlElement("href", Namespace = WebDav.Namespaces.DAV, DataType = "string")]
        public string Href = null;
    }
}