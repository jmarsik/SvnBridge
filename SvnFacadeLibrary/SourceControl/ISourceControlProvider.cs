using CodePlex.TfsLibrary.ObjectModel;
using SvnBridge.Protocol;

namespace SvnBridge.SourceControl
{
    public interface ISourceControlProvider
    {
        void DeleteActivity(string activityId);

        void DeleteItem(string activityId,
                        string path);

        FolderMetaData GetChangedItems(string path,
                                       int versionFrom,
                                       int VersionTo,
                                       UpdateReportData reportData);

        ItemMetaData GetItems(int version,
                              string path,
                              Recursion recursion);

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

        void WriteFile(string activityId,
                       string path,
                       byte[] fileData);
    }
}