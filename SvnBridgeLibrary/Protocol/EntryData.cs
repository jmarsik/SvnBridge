using System.Xml.Schema;
using System.Xml.Serialization;

namespace SvnBridge.Protocol
{
    public class EntryData
    {
        public EntryData() {}

        [XmlAttribute("rev", DataType = "string", Form = XmlSchemaForm.Unqualified)]
        public string Rev = null;

        [XmlAttribute("start-empty", DataType = "boolean", Form = XmlSchemaForm.Unqualified)]
        public bool StartEmpty = false;

        [XmlText()]
        public string path;
    }
}