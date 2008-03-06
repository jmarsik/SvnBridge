using System;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SvnBridge.Protocol
{
    [XmlRoot("update-report", Namespace = WebDav.Namespaces.SVN)]
    public class UpdateReportData
    {
        [XmlElement("entry", Namespace = WebDav.Namespaces.SVN)] public List<EntryData> Entries = null;

        [XmlElement("missing", Namespace = WebDav.Namespaces.SVN)] public List<string> Missing = null;

        [XmlAttribute("send-all", DataType = "boolean", Form = XmlSchemaForm.Unqualified)] public bool SendAll = false;

        [XmlElement("src-path", Namespace = WebDav.Namespaces.SVN, DataType = "string")] public string SrcPath = null;

        [XmlElement("target-revision", Namespace = WebDav.Namespaces.SVN, DataType = "string")] public string
            TargetRevision = null;

        [XmlElement("update-target", Namespace = WebDav.Namespaces.SVN, DataType = "string")] public string UpdateTarget
            = null;

        private bool isCheckOut;

        public bool IsCheckOut
        {
            get { return Entries[0].StartEmpty && Entries.Count == 1; }
        }

        public bool IsMissing(string name)
        {
            string path = new Uri(SrcPath).LocalPath;
            if (path.StartsWith("/"))
                path = path.Substring(1);
            if (path.EndsWith("/") == false)
                path += "/";
            if (name.StartsWith(path))
                name = name.Substring(path.Length);
            return Missing != null && Missing.Contains(name);
        }
    }
}