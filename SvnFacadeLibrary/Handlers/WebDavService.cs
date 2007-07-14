using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.Exceptions;
using SvnBridge.Nodes;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class WebDavService
    {
        public const string RepositoryUuid = "81a5aebe-f34e-eb42-b435-ac1ecbb335f7";
        const string VccPath = "/!svn/vcc/default";
        ISourceControlProvider sourceControlProvider;

        public WebDavService(ISourceControlProvider sourceControlProvider)
        {
            this.sourceControlProvider = sourceControlProvider;
        }

        void ProcessPropFind(INode node,
                             string path,
                             List<XmlElement> properties,
                             StreamWriter output)
        {
            output.Write("<D:response xmlns:lp1=\"DAV:\" xmlns:lp2=\"http://subversion.tigris.org/xmlns/dav/\">\n");
            output.Write("<D:href>" + node.Href() + "</D:href>\n");

            XmlDocument doc = new XmlDocument();
            List<string> propertyResults = new List<string>();

            foreach (XmlElement prop in properties)
            {
                XmlElement property = doc.CreateElement(prop.LocalName, prop.NamespaceURI);
                propertyResults.Add(node.GetProperty(property));
            }

            output.Write("<D:propstat>\n");
            output.Write("<D:prop>\n");
            foreach (string propertyResult in propertyResults)
            {
                output.Write(propertyResult + "\n");
            }
            output.Write("</D:prop>\n");
            output.Write("<D:status>HTTP/1.1 200 OK</D:status>\n");
            output.Write("</D:propstat>\n");
            output.Write("</D:response>\n");
        }

        public void PropFind(PropFindData propfind,
                             string path,
                             string depth,
                             string label,
                             Stream outputStream)
        {
            if (path == "/!svn/vcc/default")
            {
                INode node = new SvnVccDefaultNode(sourceControlProvider, path, label);
                using (StreamWriter output = new StreamWriter(outputStream))
                {
                    output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
                    output.Write("<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n");
                    ProcessPropFind(node, path, propfind.Prop.Properties, output);
                    output.Write("</D:multistatus>\n");
                }
            }
            else if (path.StartsWith("/!svn/bln/"))
            {
                INode node = new SvnBlnNode(path, int.Parse(path.Substring(10)));
                using (StreamWriter output = new StreamWriter(outputStream))
                {
                    output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
                    output.Write("<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n");
                    ProcessPropFind(node, path, propfind.Prop.Properties, output);
                    output.Write("</D:multistatus>\n");
                }
            }
            else
            {
                if (path.StartsWith("/!svn/bc/"))
                {
                    string version = path.Split('/')[3];
                    if (!sourceControlProvider.ItemExists(Helper.Decode(path.Substring(9 + version.Length)), int.Parse(version)))
                    {
                        throw new FileNotFoundException();
                    }
                }
                else
                {
                    if (!sourceControlProvider.ItemExists(Helper.Decode(path)))
                    {
                        throw new FileNotFoundException();
                    }
                }

                FolderMetaData folderInfo = null;
                if (depth == "0")
                {
                    folderInfo = new FolderMetaData();
                    ItemMetaData item = new ItemMetaData();
                    item.Name = path;
                    folderInfo.Items.Add(item);
                }
                else
                {
                    folderInfo = (FolderMetaData)sourceControlProvider.GetItems(sourceControlProvider.GetLatestVersion(), path, Recursion.OneLevel);
                }

                using (StreamWriter output = new StreamWriter(outputStream))
                {
                    output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
                    if (propfind.Prop.Properties.Count > 1)
                    {
                        output.Write("<D:multistatus xmlns:D=\"DAV:\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:ns0=\"DAV:\">\n");
                    }
                    else
                    {
                        output.Write("<D:multistatus xmlns:D=\"DAV:\" xmlns:ns0=\"DAV:\">\n");
                    }
                    if (path.StartsWith("/!svn/bc/"))
                    {
                        foreach (ItemMetaData item in folderInfo.Items)
                        {
                            INode node = new BcFileNode(VccPath, item.Name, sourceControlProvider, RepositoryUuid);
                            ProcessPropFind(node, item.Name, propfind.Prop.Properties, output);
                        }
                    }
                    else
                    {
                        foreach (ItemMetaData item in folderInfo.Items)
                        {
                            INode node = new FileNode(VccPath, item.Name, sourceControlProvider, RepositoryUuid);
                            ProcessPropFind(node, item.Name, propfind.Prop.Properties, output);
                        }
                    }
                    output.Write("</D:multistatus>\n");
                }
            }
        }

        public void PropPatch(PropertyUpdateData request,
                              string path,
                              StreamWriter output)
        {
            string activityId = path.Substring(11, path.Length - (path.Length - path.IndexOf('/', 11)) - 11);
            switch (request.Set.Prop.Properties[0].LocalName)
            {
                case "log":
                    sourceControlProvider.SetActivityComment(activityId, request.Set.Prop.Properties[0].InnerText);
                    output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
                    output.Write("<D:multistatus xmlns:D=\"DAV:\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/svn/\" xmlns:ns0=\"DAV:\">\n");
                    output.Write("<D:response>\n");
                    output.Write("<D:href>" + path + "</D:href>\n");
                    output.Write("<D:propstat>\n");
                    output.Write("<D:prop>\n");
                    output.Write("<ns1:log/>\r\n");
                    output.Write("</D:prop>\n");
                    output.Write("<D:status>HTTP/1.1 200 OK</D:status>\n");
                    output.Write("</D:propstat>\n");
                    output.Write("</D:response>\n");
                    output.Write("</D:multistatus>\n");
                    break;
                default:
                    string itemPath = Helper.Decode(path.Substring(path.IndexOf('/', 11)));
                    sourceControlProvider.SetProperty(activityId, itemPath, request.Set.Prop.Properties[0].LocalName, request.Set.Prop.Properties[0].InnerText);
                    output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
                    output.Write("<D:multistatus xmlns:D=\"DAV:\" xmlns:ns3=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:ns2=\"http://subversion.tigris.org/xmlns/custom/\" xmlns:ns1=\"http://subversion.tigris.org/xmlns/svn/\" xmlns:ns0=\"DAV:\">\n");
                    output.Write("<D:response>\n");
                    output.Write("<D:href>" + path + "</D:href>\n");
                    output.Write("<D:propstat>\n");
                    output.Write("<D:prop>\n");
                    output.Write("<ns1:" + request.Set.Prop.Properties[0].LocalName + "/>\r\n");
                    output.Write("</D:prop>\n");
                    output.Write("<D:status>HTTP/1.1 200 OK</D:status>\n");
                    output.Write("</D:propstat>\n");
                    output.Write("</D:response>\n");
                    output.Write("</D:multistatus>\n");
                    break;
            }
        }

        private class LoadingQueue
        {
            public ItemMetaData[] Files = new ItemMetaData[3] { null, null, null };
            public bool Continue = true;
        }
        
        void LoadAllFiles(object folderInfo)
        {
            const int LOADING_THREADS = 3;
            LoadingQueue[] queues = new LoadingQueue[LOADING_THREADS];
            Thread[] loadingThreads = new Thread[LOADING_THREADS];
            for (int i = 0; i < queues.Length; i++)
            {
                queues[i] = new LoadingQueue();
                loadingThreads[i] = new Thread(FileLoader);
                loadingThreads[i].Start(queues[i]);
            }

            LoadFolderFiles(new object[] { folderInfo, queues });

            for (int i = 0; i < queues.Length; i++)
            {
                queues[i].Continue = false;
            }
        }

        void LoadFolderFiles(object[] parameters)
        {
            FolderMetaData info = (FolderMetaData)parameters[0];
            LoadingQueue[] queues = (LoadingQueue[])parameters[1];
            for (int i = 0; i < info.Items.Count; i++)
            {
                ItemMetaData item = info.Items[i];
                if (item.ItemType == ItemType.Folder)
                {
                    LoadFolderFiles(new object[] { item, queues });
                }
                else if (!(item is DeleteMetaData))
                {
                    while (!AddFileToLoadingQueue(queues, item))
                    {
                        Thread.Sleep(50);
                    }
                }
            }
        }

        bool AddFileToLoadingQueue(LoadingQueue[] queues, ItemMetaData item)
        {
            for (int i = 0; i < queues[0].Files.Length; i++)
            {
                foreach (LoadingQueue queue in queues)
                {
                    if (queue.Files[i] == null || queue.Files[i].DataLoaded == true)
                    {
                        queue.Files[i] = item;
                        return true;
                    }
                }
            }
            return false;
        }

        void FileLoader(object parameters)
        {
            LoadingQueue queue = (LoadingQueue)parameters;
            while (true)
            {
                bool loadedFile = false;
                for (int i = 0; i < queue.Files.Length; i++)
                {
                    if (queue.Files[i] != null && queue.Files[i].DataLoaded == false)
                    {
                        queue.Files[i].Data = sourceControlProvider.ReadFile(queue.Files[i]);
                        queue.Files[i].DataLoaded = true;
                        loadedFile = true;
                    }
                }
                if (!loadedFile)
                {
                    if (!queue.Continue)
                    {
                        return;
                    }
                    Thread.Sleep(100);
                }
            }
        }

        public void UpdateReport(UpdateReportData updatereport,
                                 StreamWriter output)
        {
            Uri srcPathUri = null;
            if (!String.IsNullOrEmpty(updatereport.SrcPath))
                srcPathUri = new Uri(updatereport.SrcPath);
            else
                srcPathUri = new Uri("/");

            string basePath = "/" + srcPathUri.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped);

            FolderMetaData metadata;
            if (updatereport.Entries[0].StartEmpty)
            {
                metadata = (FolderMetaData)sourceControlProvider.GetItems(int.Parse(updatereport.TargetRevision), basePath, Recursion.Full);
                Thread loadData = new Thread(LoadAllFiles);
                loadData.Start(metadata);
            }
            else
            {
                metadata = sourceControlProvider.GetChangedItems(basePath, int.Parse(updatereport.Entries[0].Rev), int.Parse(updatereport.TargetRevision), updatereport);
                Thread loadData = new Thread(LoadAllFiles);
                loadData.Start(metadata);
            }

            UpdateReportService updateReportService = new UpdateReportService(sourceControlProvider);

            output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
            output.Write("<S:update-report xmlns:S=\"svn:\" xmlns:V=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:D=\"DAV:\" send-all=\"true\">\n");
            output.Write("<S:target-revision rev=\"" + updatereport.TargetRevision + "\"/>\n");
            updateReportService.ProcessUpdateReportForDirectory(updatereport, metadata, output, true);
            output.Write("</S:update-report>\n");
        }

        public void LogReport(LogReportData logreport,
                              string path,
                              StreamWriter output)
        {
            string serverPath = path.Substring(9);
            serverPath = serverPath.Substring(serverPath.IndexOf('/'));

            LogItem logItem = sourceControlProvider.GetLog(serverPath, int.Parse(logreport.EndRevision), int.Parse(logreport.StartRevision), Recursion.Full, int.Parse(logreport.Limit));
            output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
            output.Write("<S:log-report xmlns:S=\"svn:\" xmlns:D=\"DAV:\">\n");

            foreach (SourceItemHistory history in logItem.History)
            {
                output.Write("<S:log-item>\n");
                output.Write("<D:version-name>" + history.ChangeSetID + "</D:version-name>\n");
                output.Write("<D:creator-displayname>" + history.Username + "</D:creator-displayname>\n");
                output.Write("<S:date>" + FormatDate(history.CommitDateTime) + "</S:date>\n");
                output.Write("<D:comment>" + history.Comment + "</D:comment>\n");

                foreach (SourceItemChange change in history.Changes)
                {
                    if ((change.ChangeType & ChangeType.Add) == ChangeType.Add)
                        output.Write("<S:added-path>" + change.Item.RemoteName.Substring(1) + "</S:added-path>\n");
                    else if ((change.ChangeType & ChangeType.Edit) == ChangeType.Edit)
                        output.Write("<S:modified-path>" + change.Item.RemoteName.Substring(1) + "</S:modified-path>\n");
                    else if ((change.ChangeType & ChangeType.Delete) == ChangeType.Delete)
                        output.Write("<S:deleted-path>" + change.Item.RemoteName.Substring(1) + "</S:deleted-path>\n");
                    else
                        throw new InvalidOperationException("Unrecognized change type " + change.ChangeType);
                }

                output.Write("</S:log-item>\n");
            }

            output.Write("</S:log-report>\n");
        }

        public void Options(string path,
                            Stream outputStream)
        {
            sourceControlProvider.ItemExists(Helper.Decode(path)); // Verify permissions to access
            using (StreamWriter output = new StreamWriter(outputStream))
            {
                output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
                output.Write("<D:options-response xmlns:D=\"DAV:\">\n");
                output.Write("<D:activity-collection-set><D:href>/!svn/act/</D:href></D:activity-collection-set></D:options-response>\n");
            }
        }

        public void MkActivity(string activityPath)
        {
            string activityId = activityPath.Substring(10);
            sourceControlProvider.MakeActivity(activityId);
        }

        public void Put(string path,
                        Stream inputStream,
                        string baseHash,
                        string resultHash)
        {
            string activityId = path.Substring(11, path.IndexOf('/', 11) - 11);
            string serverPath = Helper.Decode(path.Substring(11 + activityId.Length));
            SvnDiff[] diffs = SvnDiffParser.ParseSvnDiff(inputStream);
            if (diffs.Length > 0)
            {
                ItemMetaData item = sourceControlProvider.GetItems(-1, serverPath, Recursion.None);
                byte[] sourceData = new byte[0];
                if (item != null)
                {
                    sourceData = sourceControlProvider.ReadFile(item);
                    if (Helper.GetMd5Checksum(sourceData) != baseHash)
                    {
                        throw new Exception("Checksum mismatch with base file");
                    }
                }

                byte[] fileData = new byte[0];
                int sourceDataStartIndex = 0;
                foreach (SvnDiff diff in diffs)
                {
                    byte[] newData = SvnDiffEngine.ApplySvnDiff(diff, sourceData, sourceDataStartIndex);
                    sourceDataStartIndex += newData.Length;
                    Helper.ReDim(ref fileData, fileData.Length + newData.Length);
                    Array.Copy(newData, 0, fileData, fileData.Length - newData.Length, newData.Length);
                }
                if (Helper.GetMd5Checksum(fileData) != resultHash)
                {
                    throw new Exception("Checksum mismatch with new file");
                }
                sourceControlProvider.WriteFile(activityId, serverPath, fileData);
            }
            else
            {
                sourceControlProvider.WriteFile(activityId, serverPath, new byte[0]);
            }
        }

        public string CheckOut(CheckoutData request,
                               string path,
                               string host)
        {
            string location = null;
            string activityId = request.ActivitySet.href.Split('/')[3];

            switch (path.Split('/')[2])
            {
                case "bln":
                    location = "//!svn/wbl/" + activityId + path.Substring(9);
                    break;
                case "ver":
                    string itemPath = path.Substring(path.IndexOf('/', 10));
                    int version = int.Parse(path.Split('/')[3]);
                    location = "//!svn/wrk/" + activityId + itemPath;
                    ItemMetaData item = sourceControlProvider.GetItems(-1, Helper.Decode(itemPath), Recursion.None);
                    if (item.Revision > version)
                    {
                        throw new ConflictException();
                    }
                    break;
            }
            return location;
        }

        public void Merge(MergeData request,
                          string path,
                          StreamWriter output)
        {
            string activityId = request.Source.Href.Substring(10);
            MergeActivityResponse mergeResponse = sourceControlProvider.MergeActivity(activityId);

            output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
            output.Write("<D:merge-response xmlns:D=\"DAV:\">\n");
            output.Write("<D:updated-set>\n");
            output.Write("<D:response>\n");
            output.Write("<D:href>" + VccPath + "</D:href>\n");
            output.Write("<D:propstat><D:prop>\n");
            output.Write("<D:resourcetype><D:baseline/></D:resourcetype>\n");
            output.Write("\n");
            output.Write("<D:version-name>" + mergeResponse.Version.ToString() + "</D:version-name>\n");
            output.Write("<D:creationdate>" + FormatDate(mergeResponse.CreationDate) + "</D:creationdate>\n");
            output.Write("<D:creator-displayname>" + mergeResponse.Creator + "</D:creator-displayname>\n");
            output.Write("</D:prop>\n");
            output.Write("<D:status>HTTP/1.1 200 OK</D:status>\n");
            output.Write("</D:propstat>\n");
            output.Write("</D:response>\n");

            foreach (MergeActivityResponseItem item in mergeResponse.Items)
            {
                output.Write("<D:response>\n");
                output.Write("<D:href>" + Helper.Encode(item.Path) + "</D:href>\n");
                output.Write("<D:propstat><D:prop>\n");
                if (item.Type == ItemType.Folder)
                {
                    output.Write("<D:resourcetype><D:collection/></D:resourcetype>\n");
                }
                else
                {
                    output.Write("<D:resourcetype/>\n");
                }
                output.Write("<D:checked-in><D:href>/!svn/ver/" + mergeResponse.Version.ToString() + Helper.Encode(item.Path) + "</D:href></D:checked-in>\n");
                output.Write("</D:prop>\n");
                output.Write("<D:status>HTTP/1.1 200 OK</D:status>\n");
                output.Write("</D:propstat>\n");
                output.Write("</D:response>\n");
            }
            output.Write("</D:updated-set>\n");
            output.Write("</D:merge-response>\n");
        }

        public bool Delete(string path)
        {
            if (path.StartsWith("/!svn/act/"))
            {
                string activityId = path.Substring(10);
                sourceControlProvider.DeleteActivity(activityId);
            }
            else if (path.StartsWith("//!svn/wrk/"))
            {
                string activityId = path.Substring(11, path.IndexOf('/', 11) - 11);
                string filePath = path.Substring(path.IndexOf('/', 11));
                if (!sourceControlProvider.ItemExists(Helper.Decode(filePath)))
                {
                    return false;
                }
                sourceControlProvider.DeleteItem(activityId, Helper.Decode(filePath));
            }
            return true;
        }

        public static string FormatDate(DateTime date)
        {
            string result = date.ToUniversalTime().ToString("o");
            return result.Remove(result.Length - 2, 1);
        }
    }
}
