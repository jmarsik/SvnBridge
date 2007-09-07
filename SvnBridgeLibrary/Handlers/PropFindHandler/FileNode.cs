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
            if (sourceControlProvider.IsDirectory(version, Helper.Decode(path)) && !href.EndsWith("/"))
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
                case "deadprop-count":
                    return "<lp2:deadprop-count>0</lp2:deadprop-count>";
                case "creator-displayname":
                    return GetCreatorDisplayName(property);
                case "creationdate":
                    return GetCreationDate(property);
                case "version-name":
                    return GetVersionName(property);
                case "getcontentlength":
                    return GetContentLength();
                case "lockdiscovery":
                    return GetLockDiscovery();
                default:
                    throw new Exception("Property not found: " + property.LocalName);
            }
        }

        private string GetContentLength()
        {
            ItemMetaData item = sourceControlProvider.GetItems(version, Helper.Decode(path), Recursion.None);
            return "<lp1:getcontentlength>" + sourceControlProvider.ReadFile(item).Length + "</lp1:getcontentlength>";
        }

        string GetVersionControlledConfiguration(XmlElement property)
        {
            return "<lp1:version-controlled-configuration><D:href>" + vccPath + "</D:href></lp1:version-controlled-configuration>";
        }

        string GetResourceType(XmlElement property)
        {
            if (sourceControlProvider.IsDirectory(version, Helper.Decode(path)))
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

        string GetCreatorDisplayName(XmlElement property)
        {
            ItemMetaData item = sourceControlProvider.GetItems(-1, Helper.Decode(path), Recursion.None);
            return "<lp1:creator-displayname>" + item.Author + "</lp1:creator-displayname>";
        }

        string GetCreationDate(XmlElement property)
        {
            ItemMetaData item = sourceControlProvider.GetItems(-1, Helper.Decode(path), Recursion.None);
            return "<lp1:creationdate>" + Helper.FormatDate(item.LastModifiedDate) + "</lp1:creationdate>";
        }

        string GetVersionName(XmlElement property)
        {
            ItemMetaData item = sourceControlProvider.GetItems(-1, Helper.Decode(path), Recursion.None);
            return "<lp1:version-name>" + item.Revision + "</lp1:version-name>";
        }

        string GetLockDiscovery()
        {
            return "<D:lockdiscovery/>";
        }
    }
}