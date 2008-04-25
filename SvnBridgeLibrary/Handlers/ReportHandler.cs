using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.Infrastructure;
using SvnBridge.Interfaces;
using SvnBridge.Net;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class ReportHandler : HttpContextHandlerBase
    {
        protected override void Handle(IHttpContext context,
                                       ISourceControlProvider sourceControlProvider)
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
                        UpdateReport(request, sourceControlProvider, data, output);
                    }
                }
                else if (reader.NamespaceURI == WebDav.Namespaces.SVN && reader.LocalName == "replay-report")
                {
                    ReplayReportData relayReport = Helper.DeserializeXml<ReplayReportData>(reader);
                    UpdateReportData data = new UpdateReportData();
                    data.SrcPath = request.Url.AbsoluteUri;
                    data.TargetRevision = relayReport.Revision.ToString();
                    data.Entries = new List<EntryData>();
                    EntryData item = new EntryData();
                    item.Rev = (relayReport.Revision - 1).ToString();
                    data.Entries.Add(item);
                    SetResponseSettings(response, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 200);
                    response.SendChunked = true;
                    using (StreamWriter output = new StreamWriter(response.OutputStream))
                    {
                        UpdateReport(request, sourceControlProvider, data, output);
                    }
                }
                else if (reader.NamespaceURI == WebDav.Namespaces.SVN && reader.LocalName == "log-report")
                {
                    LogReportData data = Helper.DeserializeXml<LogReportData>(reader);
                    SetResponseSettings(response, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 200);
                    response.SendChunked = true;
                    response.BufferOutput = false;
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
                else if (reader.NamespaceURI == WebDav.Namespaces.SVN && reader.LocalName == "dated-rev-report")
                {
                    DatedRevReportData data = Helper.DeserializeXml<DatedRevReportData>(reader);
                    SetResponseSettings(response, "text/xml; charset=\"utf-8\"", Encoding.UTF8, 200);
                    using (StreamWriter output = new StreamWriter(response.OutputStream))
                    {
                        GetDatedRevReport(sourceControlProvider, data, output);
                    }
                }
                else if (reader.NamespaceURI == WebDav.Namespaces.SVN && reader.LocalName == "file-revs-report")
                {
                    FileRevsReportData data = Helper.DeserializeXml<FileRevsReportData>(reader);
                    string serverPath = "/";
                    if (path.IndexOf('/', 9) > -1)
                    {
                        serverPath = path.Substring(path.IndexOf('/', 9));
                    }
                    SendBlameResponse(request, response, sourceControlProvider, serverPath, data);
                    return;
                }
                else
                {
                    throw new Exception("Unrecognized report name: " + reader.LocalName);
                }
            }
        }

        private void SendBlameResponse(IHttpRequest request, IHttpResponse response, ISourceControlProvider sourceControlProvider, string serverPath, FileRevsReportData data)
        {
            LogItem log = sourceControlProvider.GetLog(
                serverPath,
                data.StartRevision,
                data.EndRevision,
                Recursion.Full,
                data.EndRevision - data.StartRevision);

            if (log.History.Length == 0)
            {
                WriteFileNotFoundResponse(request, response);
            }

            foreach (SourceItemHistory history in log.History)
            {
                foreach (SourceItemChange change in history.Changes)
                {
                    if(change.Item.ItemType==ItemType.Folder)
                    {
                        SendErrorResponseCannotRunBlameOnFolder(response, serverPath);
                        return;
                    }
                }
            }
            using (StreamWriter output = new StreamWriter(response.OutputStream))
            {
                response.StatusCode = (int) HttpStatusCode.OK;   
                output.Write(@"<?xml version=""1.0"" encoding=""utf-8""?>
<S:file-revs-report xmlns:S=""svn:"" xmlns:D=""DAV:"">");

                foreach (SourceItemHistory history in Helper.SortHistories(true, log.History))
                {
                    foreach (SourceItemChange change in history.Changes)
                    {
                        ItemMetaData items = sourceControlProvider.GetItems(change.Item.RemoteChangesetId, change.Item.RemoteName, Recursion.None);
                        MemoryStream svnDiffStream = new MemoryStream();
                        SvnDiff svnDiff = SvnDiffEngine.CreateReplaceDiff(
                            sourceControlProvider.ReadFile(items)
                            );
                        SvnDiffParser.WriteSvnDiff(svnDiff, svnDiffStream);
                        byte[] svnDiffData = svnDiffStream.ToArray();


                        output.Write(@"<S:file-rev path="""+ change.Item.RemoteName + @""" rev="""+ change.Item.RemoteChangesetId +@""">
<S:rev-prop name=""svn:log"">" + history.Comment +@"</S:rev-prop>
<S:rev-prop name=""svn:author"">" + history.Username + @"</S:rev-prop>
<S:rev-prop name=""svn:date"">" + Helper.FormatDate(change.Item.RemoteDate) + @"</S:rev-prop>
<S:txdelta>" + Convert.ToBase64String(svnDiffData) + 
@"</S:txdelta></S:file-rev>");
                    }
                }
             output.Write("</S:file-revs-report>");
  }
        }

        private static void SendErrorResponseCannotRunBlameOnFolder(IHttpResponse response, string serverPath)
        {
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.ContentType = "text/xml; charset=\"utf-8\"";
            WriteToResponse(response, @"<?xml version=""1.0"" encoding=""utf-8""?>
<D:error xmlns:D=""DAV:"" xmlns:m=""http://apache.org/dav/xmlns"" xmlns:C=""svn:"">
<C:error/>
<m:human-readable errcode=""160017"">
'" + serverPath + @"' is not a file
</m:human-readable>
</D:error>");
        }

        private void GetDatedRevReport(ISourceControlProvider sourceControlProvider, DatedRevReportData data, TextWriter output)
        {
            int targetRevision = sourceControlProvider.GetVersionForDate(data.CreationDate);

            output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
            output.Write("<S:dated-rev-report xmlns:S=\"svn:\" xmlns:D=\"DAV:\">\n");
            output.Write("<D:version-name>");
            output.Write(targetRevision);
            output.Write("</D:version-name></S:dated-rev-report>");
        }

        private static void GetLocksReport(StreamWriter writer)
        {
            writer.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
            writer.Write("<S:get-locks-report xmlns:S=\"svn:\" xmlns:D=\"DAV:\">\n");
            writer.Write("</S:get-locks-report>\n");
        }

        private void GetLocationsReport(ISourceControlProvider sourceControlProvider,
                                        GetLocationsReportData getLocationsReport,
                                        string path,
                                        StreamWriter output)
        {
            if (path.IndexOf('/', 9) > -1)
            {
                path = path.Substring(path.IndexOf('/', 9));
            }
            else
            {
                path = "/";
            }

            ItemMetaData item = sourceControlProvider.GetItemsWithoutProperties(
                int.Parse(getLocationsReport.LocationRevision),
                path,
                Recursion.None);

            output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
            output.Write("<S:get-locations-report xmlns:S=\"svn:\" xmlns:D=\"DAV:\">\n");
            if (item != null)
            {
                output.Write("<S:location rev=\"" + getLocationsReport.LocationRevision + "\" path=\"" +
                    path +
                    "\"/>\n");
            }
            output.Write("</S:get-locations-report>\n");
        }

        private void UpdateReport(IHttpRequest request,
                                  ISourceControlProvider sourceControlProvider,
                                  UpdateReportData updatereport,
                                  StreamWriter output)
        {
            string basePath = PathParser.GetLocalPath(request, updatereport.SrcPath);
            FolderMetaData metadata;
            int targetRevision;
            if (updatereport.TargetRevision != null)
            {
                targetRevision = int.Parse(updatereport.TargetRevision);
            }
            else
            {
                targetRevision = sourceControlProvider.GetLatestVersion();
            }
            if (updatereport.IsCheckOut)
            {
                metadata = (FolderMetaData)sourceControlProvider.GetItemsWithoutProperties(targetRevision, basePath, Recursion.Full);
            }
            else
            {
                metadata =
                    sourceControlProvider.GetChangedItems(basePath,
                                                          int.Parse(updatereport.Entries[0].Rev),
                                                          targetRevision,
                                                          updatereport);
            }
            if (metadata == null)
                throw new InvalidOperationException("Could not find " + basePath + " in revision " + targetRevision);

            ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                new AsyncItemLoader(metadata, sourceControlProvider).Start();
            });

            IUpdateReportService updateReportService = new UpdateReportService(this, sourceControlProvider);

            output.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
            output.Write(
                "<S:update-report xmlns:S=\"svn:\" xmlns:V=\"http://subversion.tigris.org/xmlns/dav/\" xmlns:D=\"DAV:\" send-all=\"true\">\n");
            output.Write("<S:target-revision rev=\"" + targetRevision + "\"/>\n");
            updateReportService.ProcessUpdateReportForDirectory(updatereport, metadata, output, true);
            output.Write("</S:update-report>\n");
        }

        private static void LogReport(ISourceControlProvider sourceControlProvider,
                               LogReportData logreport,
                               string path,
                               TextWriter output)
        {
            string serverPath = "/";
            if (path.IndexOf('/', 9) > -1)
            {
                serverPath = path.Substring(path.IndexOf('/', 9));
            }

            LogItem logItem = sourceControlProvider.GetLog(
                serverPath,
                int.Parse(logreport.EndRevision),
                int.Parse(logreport.StartRevision),
                Recursion.Full,
                int.Parse(logreport.Limit ?? "1000000"));
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
                    {
                        output.Write("<S:added-path>/" + Helper.EncodeB(change.Item.RemoteName) + "</S:added-path>\n");
                    }
                    else if ((change.ChangeType & ChangeType.Edit) == ChangeType.Edit)
                    {
                        output.Write("<S:modified-path>/" + Helper.EncodeB(change.Item.RemoteName) +
                                     "</S:modified-path>\n");
                    }
                    else if ((change.ChangeType & ChangeType.Delete) == ChangeType.Delete)
                    {
                        output.Write("<S:deleted-path>/" + Helper.EncodeB(change.Item.RemoteName) +
                                     "</S:deleted-path>\n");
                    }
                    else if ((change.ChangeType & ChangeType.Rename) == ChangeType.Rename)
                    {
                        RenamedSourceItem renamedItem = (RenamedSourceItem)change.Item;
                        output.Write("<S:added-path copyfrom-path=\"/" + Helper.EncodeB(renamedItem.OriginalRemoteName) +
                                     "\" copyfrom-rev=\"" + renamedItem.OriginalRevision + "\">/" +
                                     Helper.EncodeB(change.Item.RemoteName) + "</S:added-path>\n");
                        output.Write("<S:deleted-path>/" + Helper.EncodeB(renamedItem.OriginalRemoteName) +
                                     "</S:deleted-path>\n");
                    }
                    else if ((change.ChangeType & ChangeType.Branch) == ChangeType.Branch)
                    {
                        RenamedSourceItem renamedItem = (RenamedSourceItem)change.Item;
                        output.Write("<S:added-path copyfrom-path=\"/" + Helper.EncodeB(renamedItem.OriginalRemoteName) +
                                     "\" copyfrom-rev=\"" + renamedItem.OriginalRevision + "\">/" +
                                     Helper.EncodeB(change.Item.RemoteName) + "</S:added-path>\n");
                    }
                    else
                    {
                        throw new InvalidOperationException("Unrecognized change type " + change.ChangeType);
                    }
                }

                output.Write("</S:log-item>\n");
            }

            output.Write("</S:log-report>\n");
        }
    }
}
