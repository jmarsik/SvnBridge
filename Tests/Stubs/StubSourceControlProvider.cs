using System;
using System.Threading;
using CodePlex.TfsLibrary.ObjectModel;
using SvnBridge.Interfaces;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;
using SvnBridge.Infrastructure;

namespace Tests
{
    public class StubSourceControlProvider : TFSSourceControlProvider
    {
        public StubSourceControlProvider() : base("http://www.codeplex.com", null, null, new SourceControlServicesHub(null, null, null, null, null, null, null, null, null), null) { }

        public int GetLatestVersion_Return;

        public override ItemMetaData GetItemInActivity(string activityId, string path)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void MakeActivity(string activityId)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void SetActivityComment(string activityId, string comment)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override bool WriteFile(string activityId, string path, byte[] fileData)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void ReadFileAsync(ItemMetaData item)
        {
            throw new NotImplementedException();
        }

    	public override Guid GetRepositoryUuid()
    	{
			return new Guid("81a5aebe-f34e-eb42-b435-ac1ecbb335f7");
    	}

        public override int GetVersionForDate(DateTime date)
        {
            throw new NotImplementedException();
        }

    	public override ItemMetaData[] GetPreviousVersionOfItems(SourceItem[] items, int changeset)
    	{
    		throw new NotImplementedException();
    	}

    	public override MergeActivityResponse MergeActivity(string activityId)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void DeleteActivity(string activityId)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override byte[] ReadFile(ItemMetaData item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override bool ItemExists(string path)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override LogItem GetLog(string path, int versionFrom, int versionTo, Recursion recursion, int maxCount)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override ItemMetaData GetItems(int version, string path, Recursion recursion)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override ItemMetaData GetItemsWithoutProperties(int version, string path, Recursion recursion)
        {
            return GetItems(version, path, recursion);
        }

        public override bool IsDirectory(int version, string path)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override int GetLatestVersion()
        {
            return GetLatestVersion_Return;
        }

        public override void MakeCollection(string activityId, string path)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override bool ItemExists(string path, int version)
        {
            throw new Exception("The method or operation is not implemented.");
        }

    	public override bool ItemExists(int itemId, int version)
    	{
    		throw new NotImplementedException();
    	}

    	public override void SetProperty(string activityId, string path, string property, string value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

    	public override void RemoveProperty(string activityId, string path, string property)
    	{
    		throw new NotImplementedException();
    	}

    	public override bool DeleteItem(string activityId, string path)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override FolderMetaData GetChangedItems(string path, int versionFrom, int VersionTo, UpdateReportData reportData)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override string ServerUrl
        {
            get { throw new NotImplementedException(); }
        }

        public override void CopyItem(string activityId, string path, string targetPath)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
