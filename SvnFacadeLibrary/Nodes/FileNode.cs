using System;
using System.Xml;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Nodes
{
    public class FileNode : INode
    {
        string vccPath;
        string path;
        ISourceControlProvider sourceControlProvider;
        string repositoryUuid;
        int version;

        public FileNode(string vccPath,
                        string path,
                        ISourceControlProvider sourceControlProvider,
                        string repositoryUuid)
            : this(vccPath, path, sourceControlProvider, repositoryUuid, -1) { }

            public FileNode(string vccPath,
                        string path,
                        ISourceControlProvider sourceControlProvider,
                        string repositoryUuid,
                        int version)
        {
            this.vccPath = vccPath;
            this.path = path;
            this.sourceControlProvider = sourceControlProvider;
            this.repositoryUuid = repositoryUuid;
            this.version = version;
        }

        public string Href()
        {
            string href = path;
            if (sourceControlProvider.IsDirectory(-1, Helper.Decode(path)) && !href.EndsWith("/"))
                href += "/";

            return href;
        }

        public string GetProperty(XmlElement property)
        {
            switch (property.LocalName)
            {
                case "version-controlled-configuration":
                    return GetVersionControlledConfiguration(property);
                case "resourcetype":
                    return GetResourceType(property);
                case "baseline-relative-path":
                    return GetBaselineRelativePath(property);
                case "repository-uuid":
                    return GetRepositoryUUID(property);
                case "checked-in":
                    return GetCheckedIn(property);
                default:
                    throw new Exception("Property not found: " + property.LocalName);
            }
        }

        string GetVersionControlledConfiguration(XmlElement property)
        {
            return "<lp1:version-controlled-configuration><D:href>" + vccPath + "</D:href></lp1:version-controlled-configuration>";
        }

        string GetResourceType(XmlElement property)
        {
            if (sourceControlProvider.IsDirectory(-1, Helper.Decode(path)))
            {
                return "<lp1:resourcetype><D:collection/></lp1:resourcetype>";
            }
            else
            {
                return "<lp1:resourcetype/>";
            }
        }

        string GetBaselineRelativePath(XmlElement property)
        {
            string brl = path;
            if ((brl.Length > 0) && (brl[0] == '/'))
                brl = brl.Substring(1);
            if ((brl.Length > 0) && (brl[brl.Length - 1] == '/'))
                brl = brl.Substring(0, brl.Length - 1);

            brl = Helper.Decode(brl);
            if (brl.Length > 0)
            {
                ItemMetaData item = sourceControlProvider.GetItems(version, Helper.Decode(path), Recursion.None);
                return "<lp2:baseline-relative-path>" + item.Name + "</lp2:baseline-relative-path>";
            }
            else
                return "<lp2:baseline-relative-path/>";
        }

        string GetRepositoryUUID(XmlElement property)
        {
            return "<lp2:repository-uuid>" + repositoryUuid + "</lp2:repository-uuid>";
        }

        string GetCheckedIn(XmlElement property)
        {
            ItemMetaData item = sourceControlProvider.GetItems(-1, Helper.Decode(path), Recursion.None);
            return "<lp1:checked-in><D:href>/!svn/ver/" + item.Revision.ToString() + "/" + Helper.Encode(item.Name) + "</D:href></lp1:checked-in>";
        }
    }
}