using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SvnBridge.Protocol
{
    public class OpenDirectoryData
    {
        public OpenDirectoryData() {}

        [XmlAttribute("rev", DataType = "string", Form = XmlSchemaForm.Unqualified)]
        public string Rev = null;

        [XmlElement("checked-in", Namespace = WebDav.Namespaces.DAV)]
        public CheckedInData CheckedIn = null;

        [XmlElement("set-prop", Namespace = WebDav.Namespaces.SVN)]
        public List<SetPropData> SetProp = new List<SetPropData>();

        [XmlElement("add-file", Namespace = WebDav.Namespaces.SVN)]
        public List<AddFileData> AddFile = new List<AddFileData>();

        [XmlElement("add-directory", Namespace = WebDav.Namespaces.SVN)]
        public List<AddDirectoryData> AddDirectory = new List<AddDirectoryData>();
    }
}