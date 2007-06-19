using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SvnBridge.Protocol
{
    [Serializable]
    public class ResponseData
    {
        public ResponseData() {}

        [XmlElement("href", Namespace = WebDav.Namespaces.DAV, DataType = "string")]
        public string Href = null;

        [XmlElement("status", Namespace = WebDav.Namespaces.DAV, DataType = "string")]
        public string Status = null;

        [XmlElement("propstat", Namespace = WebDav.Namespaces.DAV)]
        public List<PropStatData> PropStat = new List<PropStatData>();
    }
}