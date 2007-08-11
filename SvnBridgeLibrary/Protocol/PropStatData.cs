using System;
using System.Xml.Serialization;

namespace SvnBridge.Protocol
{
    [Serializable]
    public class PropStatData
    {
        public PropStatData() {}

        [XmlElement("prop", Namespace = WebDav.Namespaces.DAV)]
        public PropData Prop = null;

        [XmlElement("status", Namespace = WebDav.Namespaces.DAV, DataType = "string")]
        public string Status = null;
    }
}