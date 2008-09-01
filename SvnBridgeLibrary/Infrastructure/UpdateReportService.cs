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
	public class UpdateReportService
	{
		private readonly HttpContextHandlerBase handler;
		private readonly ISourceControlProvider sourceControlProvider;

		public UpdateReportService(HttpContextHandlerBase handler, ISourceControlProvider sourceControlProvider)
		{
			this.handler = handler;
			this.sourceControlProvider = sourceControlProvider;
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
				string srcPath = GetSrcPath(updateReportRequest);
				int clientRevisionForItem = updateReportRequest.GetClientRevisionFor(item.StripBasePath(srcPath));
				if (ItemExistsAtTheClient(item, updateReportRequest, srcPath, clientRevisionForItem))
				{
					existingFile = true;
				}

				//another item with the same name already exists, need to remove it.
				if (ShouldDeleteItemBeforeSendingToClient(item, updateReportRequest, srcPath, clientRevisionForItem, existingFile))
				{
					output.Write("<S:delete-entry name=\"" + Helper.EncodeB(GetFileName(item.Name)) + "\"/>\n");
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

				string localPath = handler.GetLocalPath("/!svn/ver/" + item.Revision + "/" +
													Helper.Encode(item.Name, true));
				output.Write("<D:checked-in><D:href>" + localPath + "</D:href></D:checked-in>\n");
				output.Write("<S:set-prop name=\"svn:entry:committed-rev\">" + item.Revision +
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

				while (item.DataLoaded == false)
					Thread.Sleep(100);

				FileData fileData = item.Data.Value;

				item.DataLoaded = false;
				item.Data = null;

				output.Write("<S:txdelta>");
				output.Write(fileData.Base64DiffData);
				output.Write("\n</S:txdelta>");
				output.Write("<S:prop><V:md5-checksum>" + fileData.Md5 +
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

		private bool ItemExistsAtTheClient(ItemMetaData item, UpdateReportData updateReportRequest, string srcPath, int clientRevisionForItem)
		{
			return updateReportRequest.IsCheckOut == false &&
			       updateReportRequest.IsMissing(srcPath, item.Name) == false &&
			       // we need to check both name and id to ensure that the item was not renamed
			       sourceControlProvider.ItemExists(item.Name, clientRevisionForItem) &&
			       sourceControlProvider.ItemExists(item.Id, clientRevisionForItem);
		}

		private string GetSrcPath(UpdateReportData updateReportRequest)
		{
			string url = handler.GetLocalPathFromUrl(updateReportRequest.SrcPath);
			if (updateReportRequest.UpdateTarget != null)
				return url + "/" + updateReportRequest.UpdateTarget;
			return url;
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
					string srcPath = GetSrcPath(updateReportRequest);
					int clientRevisionForItem = updateReportRequest.GetClientRevisionFor(folder.StripBasePath(srcPath));
					if (ItemExistsAtTheClient(folder, updateReportRequest, srcPath, clientRevisionForItem))
					{
						existingFolder = true;
					}

					//another item with the same name already exists, need to remove it.
					if (ShouldDeleteItemBeforeSendingToClient(folder, updateReportRequest, srcPath, clientRevisionForItem, existingFolder))
					{
						output.Write("<S:delete-entry name=\"" + Helper.EncodeB(GetFileName(folder.Name)) + "\"/>\n");
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
					output.Write("<S:set-prop name=\"svn:entry:committed-rev\">" + folder.Revision +
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

		private bool ShouldDeleteItemBeforeSendingToClient(ItemMetaData folder,
			UpdateReportData updateReportRequest,
			string srcPath,
			int clientRevisionForItem,
			bool existingFolder)
		{
			return existingFolder == false && updateReportRequest.IsCheckOut == false &&
				   updateReportRequest.IsMissing(srcPath, folder.Name) == false &&
				   sourceControlProvider.ItemExists(folder.Name, clientRevisionForItem);
		}


		private static string GetFileName(string path)
		{
			int slashIndex = path.LastIndexOfAny(new char[] { '/', '\\' });
			return path.Substring(slashIndex + 1);
		}
	}
}
