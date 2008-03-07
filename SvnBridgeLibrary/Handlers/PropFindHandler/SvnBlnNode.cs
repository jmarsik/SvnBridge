using System;
using System.Xml;
using SvnBridge.Handlers;

namespace SvnBridge.Nodes
{
    public class SvnBlnNode : INode
    {
        private string path;
        private int version;

        public SvnBlnNode(string path,
                          int version)
        {
            this.path = path;
            this.version = version;
        }

        #region INode Members

        public string Href(HttpContextHandlerBase handler)
        {
            return handler.ApplicationPath + path;
        }

        public string GetProperty(HttpContextHandlerBase handler, XmlElement property)
        {
            switch (property.LocalName)
            {
                case "baseline-collection":
                    return GetBaselineCollection(handler);
                case "version-name":
                    return GetVersionName(property);
                default:
                    throw new Exception("Property not found: " + property.LocalName);
            }
        }

        #endregion

        private string GetBaselineCollection(HttpContextHandlerBase handler)
        {
            return
                "<lp1:baseline-collection><D:href>"+handler.ApplicationPath+"/!svn/bc/" + version.ToString() +
                "/</D:href></lp1:baseline-collection>";
        }

        private string GetVersionName(XmlElement property)
        {
            return "<lp1:version-name>" + version.ToString() + "</lp1:version-name>";
        }
    }
}