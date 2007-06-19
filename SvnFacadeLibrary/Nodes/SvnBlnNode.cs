using System;
using System.Xml;

namespace SvnBridge.Nodes
{
    public class SvnBlnNode : INode
    {
        string path;
        int version;

        public SvnBlnNode(string path,
                          int version)
        {
            this.path = path;
            this.version = version;
        }

        public string Href()
        {
            return path;
        }

        public string GetProperty(XmlElement property)
        {
            switch (property.LocalName)
            {
                case "baseline-collection":
                    return GetBaselineCollection(property);
                case "version-name":
                    return GetVersionName(property);
                default:
                    throw new Exception("Property not found: " + property.LocalName);
            }
        }

        string GetBaselineCollection(XmlElement property)
        {
            return "<lp1:baseline-collection><D:href>/!svn/bc/" + version.ToString() + "/</D:href></lp1:baseline-collection>";
        }

        string GetVersionName(XmlElement property)
        {
            return "<lp1:version-name>" + version.ToString() + "</lp1:version-name>";
        }
    }
}