using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.Handlers;
using SvnBridge.Interfaces;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;
using SvnBridge.Utility;

namespace SvnBridge.Infrastructure
{
    public class UpdateReportService : IUpdateReportService
    {
        private readonly HttpContextHandlerBase handler;
        private readonly ISourceControlProvider sourceControlProvider;
    	private readonly string basePath;

    	public UpdateReportService(HttpContextHandlerBase handler, ISourceControlProvider sourceControlProvider, string basePath)
        {
            this.handler = handler;
            this.sourceControlProvider = sourceControlProvider;
        	this.basePath = basePath;
        }

        public void ProcessUpdateReportForFile(UpdateReportData updateReportRequest,
                                               ItemMetaData item,
                                               StreamWriter output)
        {
            if (item is DeleteMetaData)
            {
                output.Write("<S:delete-entry name=\"" + Helper.EncodeB(GetFileName(item.Name)) + "\"/>\n");
            }
            else
            {
                bool existingFile = false;
				int clientRevisionForItem = updateReportRequest.GetClientRevisionFor(item.StripBasePath(basePath));
            	if (!updateReportRequest.IsCheckOut &&
					updateReportRequest.IsMissing(handler.GetLocalPathFromUrl(updateReportRequest.SrcPath), item.Name) == false &&
                    sourceControlProvider.ItemExists(item.Name, clientRevisionForItem))
                {
                    existingFile = true;
                }

                if (existingFile)
                {
                    output.Write("<S:open-file name=\"" + Helper.EncodeB(GetFileName(item.Name)) + "\" rev=\"" +
                                 updateReportRequest.Entries[0].Rev + "\">\n");
                }
                else
                {
                    output.Write("<S:add-file name=\"" + Helper.EncodeB(GetFileName(item.Name)) + "\">\n");
                }

            	string localPath = handler.GetLocalPath( "/!svn/ver/" + item.Revision  + "/" +
            	                                    Helper.Encode(item.Name, true));
            	output.Write("<D:checked-in><D:href>" + localPath + "</D:href></D:checked-in>\n");
                output.Write("<S:set-prop name=\"svn:entry:committed-rev\">" + item.Revision  +
                             "</S:set-prop>\n");
                output.Write("<S:set-prop name=\"svn:entry:committed-date\">" + Helper.FormatDate(item.LastModifiedDate) +
                             "</S:set-prop>\n");
                output.Write("<S:set-prop name=\"svn:entry:last-author\">" + item.Author + "</S:set-prop>\n");
                output.Write("<S:set-prop name=\"svn:entry:uuid\">" + sourceControlProvider.GetRepositoryUuid() + "</S:set-prop>\n");
                foreach (KeyValuePair<string, string> property in item.Properties)
                {
                    output.Write("<S:set-prop name=\"" + property.Key.Replace("__COLON__", ":") + "\">" + property.Value +
                                 "</S:set-prop>\n");
                }
                
                while(item.DataLoaded==false)
                    Thread.Sleep(100);

                byte[] fileData = item.Data.Value;

                item.DataLoaded = false;
                item.Data = null;

                SvnDiff svnDiff;
                try
                {
                    svnDiff = SvnDiffEngine.CreateReplaceDiff(fileData);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException("Could not create diff for " + item.Name, e);
                }
                MemoryStream svnDiffStream = new MemoryStream();
                SvnDiffParser.WriteSvnDiff(svnDiff, svnDiffStream);
                byte[] svnDiffData = svnDiffStream.ToArray();

                output.Write("<S:txdelta>");
                output.Write(Convert.ToBase64String(svnDiffData));
                output.Write("\n</S:txdelta>");
                output.Write("<S:prop><V:md5-checksum>" + Helper.GetMd5Checksum(fileData) +
                             "</V:md5-checksum></S:prop>\n");
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
                output.Write("<S:delete-entry name=\"" + Helper.EncodeB(GetFileName(folder.Name)) + "\"/>\n");
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
                	string localPath = handler.GetLocalPathFromUrl(updateReportRequest.SrcPath);

					int clientRevisionForItem = updateReportRequest.GetClientRevisionFor(folder.StripBasePath(basePath));
					if (!updateReportRequest.IsCheckOut &&
						updateReportRequest.IsMissing(localPath, folder.Name) == false &&
						sourceControlProvider.ItemExists(folder.Name, clientRevisionForItem))
                    {
                        existingFolder = true;
                    }

                    if (existingFolder)
                    {
                        output.Write("<S:open-directory name=\"" + Helper.EncodeB(GetFileName(folder.Name)) +
                                     "\" rev=\"" + updateReportRequest.Entries[0].Rev + "\">\n");
                    }
                    else
                    {
                        output.Write("<S:add-directory name=\"" + Helper.EncodeB(GetFileName(folder.Name)) +
                                     "\" bc-url=\"" + handler.GetLocalPath("/!svn/bc/" + folder.Revision + "/" + Helper.Encode(folder.Name, true)) +
                                     "\">\n");
                    }
                }
                if (!rootFolder || updateReportRequest.UpdateTarget == null)
                {
                	string svnVer = handler.GetLocalPath("/!svn/ver/" + folder.Revision + "/" +
                	                                           Helper.Encode(folder.Name, true));
                    output.Write("<D:checked-in><D:href>" + svnVer + "</D:href></D:checked-in>\n");
                    output.Write("<S:set-prop name=\"svn:entry:committed-rev\">" + folder.Revision  +
                                 "</S:set-prop>\n");
                    output.Write("<S:set-prop name=\"svn:entry:committed-date\">" +
                                 Helper.FormatDate(folder.LastModifiedDate) + "</S:set-prop>\n");
                    output.Write("<S:set-prop name=\"svn:entry:last-author\">" + folder.Author + "</S:set-prop>\n");
                    output.Write("<S:set-prop name=\"svn:entry:uuid\">" + sourceControlProvider.GetRepositoryUuid() + "</S:set-prop>\n");
                    foreach (KeyValuePair<string, string> property in folder.Properties)
                    {
                        output.Write("<S:set-prop name=\"" + property.Key.Replace("__COLON__", ":") + "\">" +
                                     property.Value +
                                     "</S:set-prop>\n");
                    }
                }

                for (int i = 0; i < folder.Items.Count; i++)
                {
                    ItemMetaData item = folder.Items[i];
                    if (item.ItemType == ItemType.Folder)
                    {
                        ProcessUpdateReportForDirectory(updateReportRequest, (FolderMetaData) item, output, false);
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

        private static string GetFileName(string path)
        {
            int slashIndex = path.LastIndexOfAny(new char[] {'/', '\\'});
            return path.Substring(slashIndex + 1);
        }
    }
}
