using System;
using System.Threading;
using CodePlex.TfsLibrary.ObjectModel;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;

namespace SvnBridge.Interfaces
{
	public interface ISourceControlProvider
	{
		string ServerUrl { get; }

		void CopyItem(string activityId,
		              string path,
		              string targetPath);

		void DeleteActivity(string activityId);

		bool DeleteItem(string activityId,
		                string path);

		FolderMetaData GetChangedItems(string path,
		                               int versionFrom,
		                               int VersionTo,
		                               UpdateReportData reportData);

		ItemMetaData GetItems(int version,
		                      string path,
		                      Recursion recursion);


		ItemMetaData GetItemsWithoutProperties(int version,
		                                       string path,
		                                       Recursion recursion);

		ItemMetaData GetItemInActivity(string activityId,
		                               string path);

		int GetLatestVersion();

		LogItem GetLog(string path,
		               int versionFrom,
		               int versionTo,
		               Recursion recursion,
		               int maxCount);

		bool IsDirectory(int version,
		                 string path);

		bool ItemExists(string path);

		bool ItemExists(string path,
		                int version);

		void MakeActivity(string activityId);

		void MakeCollection(string activityId,
		                    string path);

		MergeActivityResponse MergeActivity(string activityId);
		byte[] ReadFile(ItemMetaData item);

		void SetActivityComment(string activityId,
		                        string comment);

		void SetProperty(string activityId,
		                 string path,
		                 string property,
		                 string value);

		void RemoveProperty(string activityId,
		                    string path,
		                    string property);

		bool WriteFile(string activityId,
		               string path,
		               byte[] fileData);

		void ReadFileAsync(ItemMetaData item);

		Guid GetRepositoryUuid();

		int GetVersionForDate(DateTime date);
	}
}