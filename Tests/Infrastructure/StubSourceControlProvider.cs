using System;
using System.Collections.Generic;
using CodePlex.TfsLibrary.ObjectModel;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;

namespace Tests
{
    public class StubSourceControlProvider : ISourceControlProvider
    {
        public virtual void MakeActivity(string activityId)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual void SetActivityComment(string activityId,
                                               string comment)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual void WriteFile(string activityId,
                                      string path,
                                      byte[] fileData)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual MergeActivityResponse MergeActivity(string activityId)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual void DeleteActivity(string activityId)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual byte[] ReadFile(ItemMetaData item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual bool ItemExists(string path)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual LogItem GetLog(string path,
                                      int versionFrom,
                                      int versionTo,
                                      Recursion recursion,
                                      int maxCount)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual ItemMetaData GetItems(int version,
                                             string path,
                                             Recursion recursion)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual bool IsDirectory(int version,
                                        string path)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual int GetLatestVersion()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual void MakeCollection(string activityId,
                                           string path)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual bool ItemExists(string path,
                                       int version)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual void SetProperty(string activityId,
                                        string path,
                                        string property,
                                        string value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual void DeleteItem(string activityId,
                                       string path)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual FolderMetaData GetChangedItems(string path, int versionFrom, int VersionTo, UpdateReportData reportData)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public virtual void CopyItem(string activityId, string path, string targetPath)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}