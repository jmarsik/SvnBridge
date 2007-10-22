using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using SvnBridge.Net;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;
using SvnBridge.Utility;
using System;
using System.Threading;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using System.Collections.Generic;
using System.Collections;

namespace SvnBridge.Handlers
{
    public class ReportHandler : HttpContextHandlerBase
    {
        protected override void Handle(IHttpContext context, ISourceControlProvider sourceControlProvider)
        {
            IHttpRequest request = context.Request;
            IHttpResponse response = context.Response;
            string path = GetPath(request);

            using (XmlReader reader = XmlReader.Create(request.InputStream, Helper.InitializeNewXmlReaderSettings()))
            {
                reader.MoveToContent();
                if (reader.NamespaceURI == WebDav.Namespaces.SVN && reader.LocalName == "get-locks-report")
                {
                    SetResponseSettings(response, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 200);
                    response.SendChunked = true;
                    using (StreamWriter writer = new StreamWriter(response.OutputStream))
                    {
                        GetLocksReport(writer);
                    }
                }
                else if (reader.NamespaceURI == WebDav.Namespaces.SVN && reader.LocalName == "update-report")
                {
                    UpdateReportData data = Helper.DeserializeXml<UpdateReportData>(reader);
                    SetResponseSettings(response, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 200);
                    response.SendChunked = true;
                    using (StreamWriter output = new StreamWriter(response.OutputStream))
                    {
                        UpdateReport(sourceControlProvider, data, output);
                    }
                }
                else if (reader.NamespaceURI == WebDav.Namespaces.SVN && reader.LocalName == "log-report")
                {
                    LogReportData data = Helper.DeserializeXml<LogReportData>(reader);
                    SetResponseSettings(response, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 200);
                    response.SendChunked = true;
                    using (StreamWriter output = new StreamWriter(response.OutputStream))
                    {
                        LogReport(sourceControlProvider, data, path, output);
                    }
                }
                else if (reader.NamespaceURI == WebDav.Namespaces.SVN && reader.LocalName == "get-locations")
                {
                    GetLocationsReportData data = Helper.DeserializeXml<GetLocationsReportData>(reader);
                    SetResponseSettings(response, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 200);
                    response.SendChunked = true;
                    using (StreamWriter output = new StreamWriter(response.OutputStream))
                    {
                        GetLocationsReport(sourceControlProvider, data, path, output);
                    }
                }
                else
                {

                    throw new Exception("Unrecognized report name: " + reader.LocalName);
                }
            }
        }

        private void GetLocksReport(StreamWriter writer)
        {
            writer.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
            writer.Write("<S:get-locks-report xmlns:S=\"svn:\" xmlns:D=\"DAV:\">\n");
            writer.Write("</S:get-locks-report>\n");

        }
        
        private void GetLocationsReport(ISourceControlProvider sourceControlProvider, GetLocationsReportData getLocationsReport, string path, StreamWriter output)
        {
            if (path.IndexOf('/', 9) > -1)
                path = path.Substring(path.IndexOf('/', 9));
            else
                path = "/";

            output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
            output.Write("<S:get-locations-report xmlns:S=\"svn:\" xmlns:D=\"DAV:\">\n");
            output.Write("<S:location rev=\"" + getLocationsReport.LocationRevision + "\" path=\"" + path + "\"/>\n");
            output.Write("</S:get-locations-report>\n");
        }

        private void UpdateReport(ISourceControlProvider sourceControlProvider, UpdateReportData updatereport, StreamWriter output)
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
                loadData.Start(new object[] { metadata, sourceControlProvider });
            }
            else
            {
                metadata = sourceControlProvider.GetChangedItems(basePath, int.Parse(updatereport.Entries[0].Rev), int.Parse(updatereport.TargetRevision), updatereport);
                Thread loadData = new Thread(LoadAllFiles);
                loadData.Start(new object[] { metadata, sourceControlProvider });
            }

            UpdateReportService updateReportService = new UpdateReportService(sourceControlProvider);

            output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
            output.Write("<S:update-report xmlns:S=\"svn:\" xmlns:V=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:D=\"DAV:\" send-all=\"true\">\n");
            output.Write("<S:target-revision rev=\"" + updatereport.TargetRevision + "\"/>\n");
            updateReportService.ProcessUpdateReportForDirectory(updatereport, metadata, output, true);
            output.Write("</S:update-report>\n");
        }

        private void LogReport(ISourceControlProvider sourceControlProvider, LogReportData logreport, string path, StreamWriter output)
        {
            string serverPath = "/";
            if (path.IndexOf('/',9) > -1)
                serverPath = path.Substring(path.IndexOf('/', 9));

            LogItem logItem = sourceControlProvider.GetLog(serverPath, int.Parse(logreport.EndRevision), int.Parse(logreport.StartRevision), Recursion.Full, int.Parse(logreport.Limit));
            output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
            output.Write("<S:log-report xmlns:S=\"svn:\" xmlns:D=\"DAV:\">\n");

            foreach (SourceItemHistory history in logItem.History)
            {
                output.Write("<S:log-item>\n");
                output.Write("<D:version-name>" + history.ChangeSetID + "</D:version-name>\n");
                output.Write("<D:creator-displayname>" + history.Username + "</D:creator-displayname>\n");
                output.Write("<S:date>" + Helper.FormatDate(history.CommitDateTime) + "</S:date>\n");
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
                    else if ((change.ChangeType & ChangeType.Branch) == ChangeType.Branch)
                    {
                        RenamedSourceItem renamedItem = (RenamedSourceItem)change.Item;
                        output.Write("<S:added-path copyfrom-path=\"" + Helper.EncodeB(renamedItem.OriginalRemoteName.Substring(1)) + "\" copyfrom-rev=\"" + renamedItem.OriginalRevision + "\">" + Helper.EncodeB(change.Item.RemoteName.Substring(1)) + "</S:added-path>\n");
                    }
                    else
                        throw new InvalidOperationException("Unrecognized change type " + change.ChangeType);
                }

                output.Write("</S:log-item>\n");
            }

            output.Write("</S:log-report>\n");
        }

        void LoadAllFiles(object parameters)
        {
            FolderMetaData folderInfo = (FolderMetaData)((object[])parameters)[0];
            ISourceControlProvider sourceControlProvider = (ISourceControlProvider)((object[])parameters)[1];
            Queue<ItemMetaData> loadingQueue = new Queue<ItemMetaData>();

            QueueItemsInFolder(loadingQueue, folderInfo);

            const int LOADING_THREADS = 5;
            ItemLoader[] itemLoaders = new ItemLoader[LOADING_THREADS];
            Thread[] loadingThreads = new Thread[LOADING_THREADS];
            for (int i = 0; i < itemLoaders.Length; i++)
            {
                itemLoaders[i] = new ItemLoader(loadingQueue, sourceControlProvider);
                loadingThreads[i] = new Thread(itemLoaders[i].Start);
                loadingThreads[i].Start();
            }

            bool threadsActive;
            do
            {
                Thread.Sleep(100);

                threadsActive = false;
                foreach (Thread thread in loadingThreads)
                    if (thread.ThreadState != ThreadState.Stopped)
                        threadsActive = true;

                if (_cancel)
                    foreach (ItemLoader itemLoader in itemLoaders)
                        itemLoader.Cancel();

            } while (threadsActive);
        }

        void QueueItemsInFolder(Queue<ItemMetaData> loadingQueue, FolderMetaData folder)
        {
            foreach (ItemMetaData item in folder.Items)
            {
                if (item.ItemType == ItemType.Folder)
                {
                    QueueItemsInFolder(loadingQueue, (FolderMetaData)item);
                }
                else if (!(item is DeleteMetaData))
                {
                    lock (((ICollection)loadingQueue).SyncRoot)
                    {
                        loadingQueue.Enqueue(item);
                    }
                }
            }
        }
    }
}