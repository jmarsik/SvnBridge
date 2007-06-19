using System;
using System.Xml.Serialization;

namespace SvnBridge.Protocol
{
    [Serializable]
    public class SourceData
    {
        public SourceData() {}

        [XmlElement("href", Namespace = WebDav.Namespaces.DAV, DataType = "string")]
        public string Href = null;
    }
}