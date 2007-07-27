using System;
using System.Collections.Generic;
using System.Text;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using NUnit.Framework;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;
using SvnBridge.Exceptions;

namespace Tests
{
    [TestFixture]
    public class TFSSourceControlProviderTestsBase
    {
        const string SERVER_NAME = "http://codeplex-tfs2:8080";
        const string PROJECT_NAME = "Test05011252";
        protected string activityId;
        protected string testPath;
        protected TFSSourceControlProvider provider;

        [SetUp]
        public void SetUp()
        {
            provider = new TFSSourceControlProvider(SERVER_NAME, null);
            activityId = Guid.NewGuid().ToString();
            testPath = "/" + PROJECT_NAME + "/Test" + DateTime.Now.ToString("yyyyMMddHHmmss");
            provider.MakeActivity(activityId);
            provider.MakeCollection(activityId, testPath);
            Commit();
        }

        [TearDown]
        public void TearDown()
        {
            DeleteItem(testPath, false);
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);
        }

        protected void UpdateFile(string path,
                        string fileData,
                        bool commit)
        {
            byte[] data = Encoding.Default.GetBytes(fileData);
            provider.WriteFile(activityId, path, data);
            if (commit)
                Commit();
        }

        protected bool WriteFile(string path,
                        string fileData,
                        bool commit)
        {
            byte[] data = Encoding.Default.GetBytes(fileData);
            return WriteFile(path, data, commit);
        }

        protected bool WriteFile(string path,
                        byte[] fileData,
                        bool commit)
        {
            bool created = provider.WriteFile(activityId, path, fileData);
            if (commit)
                Commit();
            return created;
        }

        protected void Commit()
        {
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);
            provider.MakeActivity(activityId);
        }

        protected void DeleteItem(string path,
                        bool commit)
        {
            provider.DeleteItem(activityId, path);
            if (commit)
                Commit();
        }

        protected void CopyItem(string path, string newPath, bool commit)
        {
            provider.CopyItem(activityId, path, newPath);
            if (commit)
                Commit();
        }

        protected void MoveItem(string path, string newPath, bool commit)
        {
            DeleteItem(path, false);
            CopyItem(testPath + "/Fun.txt", testPath + "/FunRename.txt", false);
            if (commit)
                Commit();
        }

        protected void CreateFolder(string path,
                        bool commit)
        {
            provider.MakeCollection(activityId, path);
            if (commit)
                Commit();
        }

        protected byte[] ReadFile(string path)
        {
            ItemMetaData item = provider.GetItems(-1, path, Recursion.None);
            return provider.ReadFile(item);
        }

        protected void SetProperty(string path, string name, string value, bool commit)
        {
            provider.SetProperty(activityId, path, name, value);
            if (commit)
                Commit();
        }
    }
}