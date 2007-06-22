using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Handlers
{
    public class UpdateReportService
    {
        ISourceControlProvider sourceControlProvider;

        public UpdateReportService(ISourceControlProvider sourceControlProvider)
        {
            this.sourceControlProvider = sourceControlProvider;
        }

        public void ProcessUpdateReportForFile(UpdateReportData updateReportRequest,
                                               ItemMetaData item,
                                               StreamWriter output)
        {
            if (item is DeleteMetaData)
            {
                output.Write("<S:delete-entry name=\"" + GetFileName(item.Name) + "\"/>\n");
            }
            else
            {
                bool existingFile = false;
                if (!updateReportRequest.Entries[0].StartEmpty &&
                    sourceControlProvider.ItemExists(item.Name, int.Parse(updateReportRequest.Entries[0].Rev)))
                {
                    existingFile = true;
                }
                if (existingFile)
                {
                    output.Write("<S:open-file name=\"" + GetFileName(item.Name) + "\" rev=\"" + updateReportRequest.Entries[0].Rev + "\">\n");
                }
                else
                {
                    output.Write("<S:add-file name=\"" + GetFileName(item.Name) + "\">\n");
                }
                output.Write("<D:checked-in><D:href>/!svn/ver/" + item.Revision + "/" + Helper.Encode(item.Name) + "</D:href></D:checked-in>\n");
                output.Write("<S:set-prop name=\"svn:entry:committed-rev\">" + item.Revision + "</S:set-prop>\n");
                output.Write("<S:set-prop name=\"svn:entry:committed-date\">" + WebDavService.FormatDate(item.LastModifiedDate) + "</S:set-prop>\n");
                output.Write("<S:set-prop name=\"svn:entry:last-author\">" + item.Author + "</S:set-prop>\n");
                output.Write("<S:set-prop name=\"svn:entry:uuid\">" + WebDavService.RepositoryUuid + "</S:set-prop>\n");
                foreach (KeyValuePair<string, string> property in item.Properties)
                {
                    output.Write("<S:set-prop name=\"svn:" + property.Key + "\">" + property.Value + "</S:set-prop>\n");
                }

                while (!item.DataLoaded)
                {
                    Thread.Sleep(100);
                }
                byte[] fileData = item.Data;
                item.DataLoaded = false;
                item.Data = null;
                SvnDiff svnDiff = SvnDiffEngine.CreateReplaceDiff(fileData);
                MemoryStream svnDiffStream = new MemoryStream();
                SvnDiffParser.WriteSvnDiff(svnDiff, svnDiffStream);
                byte[] svnDiffData = svnDiffStream.ToArray();

                output.Write("<S:txdelta>");
                output.Write(Convert.ToBase64String(svnDiffData) + "\n");
                output.Write("</S:txdelta>");
                output.Write("<S:prop><V:md5-checksum>" + Helper.GetMd5Checksum(fileData) + "</V:md5-checksum></S:prop>\n");
                if (existingFile)
                {
                    output.Write("</S:open-file>\n");
                }
                else
                {
                    output.Write("</S:add-file>\n");
                }
            }
        }

        public void ProcessUpdateReportForDirectory(UpdateReportData updateReportRequest,
                                                    FolderMetaData folder,
                                                    StreamWriter output,
                                                    bool rootFolder)
        {
            if (folder is DeleteFolderMetaData)
            {
                output.Write("<S:delete-entry name=\"" + GetFileName(folder.Name) + "\"/>\n");
            }
            else
            {
                bool existingFolder = false;
                if (rootFolder)
                {
                    output.Write("<S:open-directory rev=\"" + updateReportRequest.Entries[0].Rev + "\">\n");
                }
                else
                {
                    if (!updateReportRequest.Entries[0].StartEmpty &&
                        sourceControlProvider.ItemExists(folder.Name, int.Parse(updateReportRequest.Entries[0].Rev)))
                    {
                        existingFolder = true;
                    }
                    if (existingFolder)
                    {
                        output.Write("<S:open-directory name=\"" + GetFileName(folder.Name) + "\" rev=\"" + updateReportRequest.Entries[0].Rev + "\">\n");
                    }
                    else
                    {
                        output.Write("<S:add-directory name=\"" + GetFileName(folder.Name) + "\" bc-url=\"/!svn/bc/1/" + folder.Name + "\">\n");
                    }
                }
                output.Write("<D:checked-in><D:href>/!svn/ver/" + folder.Revision + "/" + Helper.Encode(folder.Name) + "</D:href></D:checked-in>\n");
                output.Write("<S:set-prop name=\"svn:entry:committed-rev\">" + folder.Revision + "</S:set-prop>\n");
                output.Write("<S:set-prop name=\"svn:entry:committed-date\">" + WebDavService.FormatDate(folder.LastModifiedDate) + "</S:set-prop>\n");
                output.Write("<S:set-prop name=\"svn:entry:last-author\">" + folder.Author + "</S:set-prop>\n");
                output.Write("<S:set-prop name=\"svn:entry:uuid\">" + WebDavService.RepositoryUuid + "</S:set-prop>\n");
                foreach (KeyValuePair<string, string> property in folder.Properties)
                {
                    output.Write("<S:set-prop name=\"svn:" + property.Key + "\">" + property.Value + "</S:set-prop>\n");
                }

                for (int i = 0; i < folder.Items.Count; i++)
                {
                    ItemMetaData item = folder.Items[i];
                    if (item.ItemType == ItemType.Folder)
                    {
                        ProcessUpdateReportForDirectory(updateReportRequest, (FolderMetaData)item, output, false);
                    }
                    else
                    {
                        ProcessUpdateReportForFile(updateReportRequest, item, output);
                    }
                }
                output.Write("<S:prop></S:prop>\n");
                if (rootFolder || existingFolder)
                {
                    output.Write("</S:open-directory>\n");
                }
                else
                {
                    output.Write("</S:add-directory>\n");
                }
            }
        }

        static string GetFileName(string path)
        {
            int slashIndex = path.LastIndexOfAny(new char[] { '/', '\\' });
            return path.Substring(slashIndex + 1);
        }
    }
}