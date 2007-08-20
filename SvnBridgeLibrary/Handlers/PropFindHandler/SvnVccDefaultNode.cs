using System;
using System.Xml;
using SvnBridge.SourceControl;

namespace SvnBridge.Nodes
{
    // Node: <server>/!svn/vcc/default
    public class SvnVccDefaultNode : INode
    {
        ISourceControlProvider sourceControlProvider;
        string path;
        string label;

        public SvnVccDefaultNode(ISourceControlProvider sourceControlProvider,
                                 string path,
                                 string label)
        {
            this.sourceControlProvider = sourceControlProvider;
            this.path = path;
            this.label = label;
        }

        public string Href()
        {
            if (label == null)
                return path;
            else
                return "/!svn/bln/" + label;
        }

        public string GetProperty(XmlElement property)
        {
            switch (property.LocalName)
            {
                case "checked-in":
                    return GetCheckedIn(property);
                case "baseline-collection":
                    return GetBaselineCollection(property);
                case "version-name":
                    return GetVersionName(property);
                default:
                    throw new Exception("Property not found: " + property.LocalName);
            }
        }

        string GetCheckedIn(XmlElement property)
        {
            int maxVersion = sourceControlProvider.GetLatestVersion();
            return "<lp1:checked-in><D:href>/!svn/bln/" + maxVersion.ToString() + "</D:href></lp1:checked-in>";
        }

        string GetBaselineCollection(XmlElement property)
        {
            return "<lp1:baseline-collection><D:href>/!svn/bc/" + label + "/</D:href></lp1:baseline-collection>";
        }

        string GetVersionName(XmlElement property)
        {
            return "<lp1:version-name>" + label + "</lp1:version-name>";
        }
    }
}