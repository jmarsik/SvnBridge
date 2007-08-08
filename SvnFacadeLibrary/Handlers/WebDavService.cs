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
        public const string VccPath = "/!svn/vcc/default";
        ISourceControlProvider sourceControlProvider;

        public WebDavService(ISourceControlProvider sourceControlProvider)
        {
            this.sourceControlProvider = sourceControlProvider;
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

        public void LogReport(LogReportData logreport, string path, StreamWriter output)
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
                output.Write("<D:comment>" + Helper.EncodeB(history.Comment) + "</D:comment>\n");

                foreach (SourceItemChange change in history.Changes)
                {
                    if ((change.ChangeType & ChangeType.Add) == ChangeType.Add)
                        output.Write("<S:added-path>" + Helper.EncodeB(change.Item.RemoteName.Substring(1)) + "</S:added-path>\n");
                    else if ((change.ChangeType & ChangeType.Edit) == ChangeType.Edit)
                        output.Write("<S:modified-path>" + Helper.EncodeB(change.Item.RemoteName.Substring(1)) + "</S:modified-path>\n");
                    else if ((change.ChangeType & ChangeType.Delete) == ChangeType.Delete)
                        output.Write("<S:deleted-path>" + Helper.EncodeB(change.Item.RemoteName.Substring(1)) + "</S:deleted-path>\n");
                    else if ((change.ChangeType & ChangeType.Rename) == ChangeType.Rename)
                    {
                        RenamedSourceItem renamedItem = (RenamedSourceItem)change.Item;
                        output.Write("<S:added-path copyfrom-path=\"" + Helper.EncodeB(renamedItem.OriginalRemoteName.Substring(1)) + "\" copyfrom-rev=\"" + renamedItem.OriginalRevision + "\">" + Helper.EncodeB(change.Item.RemoteName.Substring(1)) + "</S:added-path>\n");
                        output.Write("<S:deleted-path>" + Helper.EncodeB(renamedItem.OriginalRemoteName.Substring(1)) + "</S:deleted-path>\n");
                    }
                    else
                        throw new InvalidOperationException("Unrecognized change type " + change.ChangeType);
                }

                output.Write("</S:log-item>\n");
            }

            output.Write("</S:log-report>\n");
        }

        public static string FormatDate(DateTime date)
        {
            string result = date.ToUniversalTime().ToString("o");
            return result.Remove(result.Length - 2, 1);
        }
    }
}
