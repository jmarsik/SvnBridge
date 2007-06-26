using System;
using System.Collections.Generic;
using System.Text;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using NUnit.Framework;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;

namespace Tests
{
    [TestFixture]
    public class TFSSourceControlProviderTests
    {
        const string SERVER_NAME = "http://codeplex-tfs2:8080";
        const string PROJECT_NAME = "Test05011252";
        string activityId;
        string testPath;
        TFSSourceControlProvider provider;

        [SetUp]
        public void SetUp()
        {
            provider = new TFSSourceControlProvider(SERVER_NAME, null);
            activityId = Guid.NewGuid().ToString();
            testPath = "/" + PROJECT_NAME + "/Test" + DateTime.Now.ToString("yyyyMMddHHmmss");
            provider.MakeActivity(activityId);
            provider.MakeCollection(activityId, testPath);
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);

            activityId = Guid.NewGuid().ToString();
        }

        [TearDown]
        public void TearDown()
        {
            provider.MakeActivity(activityId);
            provider.DeleteItem(activityId, testPath);
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);
        }

        [Test]
        public void TestCommitFolder()
        {
            provider.MakeActivity(activityId);
            provider.MakeCollection(activityId, testPath + "/TestFolder");
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);

            Assert.IsTrue(provider.ItemExists(testPath + "/TestFolder"));
            Assert.AreEqual(ItemType.Folder, provider.GetItems(-1, testPath + "/TestFolder", Recursion.None).ItemType);
        }

        [Test]
        public void TestCommitFolderAndSubFolder()
        {
            provider.MakeActivity(activityId);
            provider.MakeCollection(activityId, testPath + "/TestFolder");
            provider.MakeCollection(activityId, testPath + "/TestFolder/SubFolder");
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);

            Assert.IsTrue(provider.ItemExists(testPath + "/TestFolder/SubFolder"));
            Assert.AreEqual(ItemType.Folder, provider.GetItems(-1, testPath + "/TestFolder/SubFolder", Recursion.None).ItemType);
        }

        [Test]
        public void TestCommitNewFile()
        {
            byte[] testFile = Encoding.Default.GetBytes("Test file contents");

            provider.MakeActivity(activityId);
            provider.WriteFile(activityId, testPath + "/TestFile.txt", testFile);
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);

            byte[] actual = ReadFile(testPath + "/TestFile.txt");
            Assert.AreEqual(Encoding.Default.GetString(testFile), Encoding.Default.GetString(actual));
        }

        [Test]
        public void TestCommitMultipleNewFiles()
        {
            byte[] testFile = Encoding.Default.GetBytes("Test file contents");

            provider.MakeActivity(activityId);
            provider.WriteFile(activityId, testPath + "/TestFile1.txt", testFile);
            provider.WriteFile(activityId, testPath + "/TestFile2.txt", testFile);
            provider.WriteFile(activityId, testPath + "/TestFile3.txt", testFile);
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);

            byte[] actual = ReadFile(testPath + "/TestFile1.txt");
            Assert.AreEqual(Encoding.Default.GetString(testFile), Encoding.Default.GetString(actual));
            actual = ReadFile(testPath + "/TestFile2.txt");
            Assert.AreEqual(Encoding.Default.GetString(testFile), Encoding.Default.GetString(actual));
            actual = ReadFile(testPath + "/TestFile3.txt");
            Assert.AreEqual(Encoding.Default.GetString(testFile), Encoding.Default.GetString(actual));
        }

        [Test]
        public void TestCommitNewFileInNewSubFolder()
        {
            byte[] testFile = Encoding.Default.GetBytes("Test file contents");

            provider.MakeActivity(activityId);
            provider.MakeCollection(activityId, testPath + "/TestFolder");
            provider.WriteFile(activityId, testPath + "/TestFolder/TestFile.txt", testFile);
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);

            byte[] actual = ReadFile(testPath + "/TestFolder/TestFile.txt");
            Assert.AreEqual(Encoding.Default.GetString(testFile), Encoding.Default.GetString(actual));
        }

        [Test]
        public void TestCommitUpdatedFile()
        {
            CreateFile(testPath + "/TestFile.txt", "Test file contents");
            byte[] testUpdatedFile = Encoding.Default.GetBytes("Test file contents\r\nUpdated");

            provider.MakeActivity(activityId);
            provider.WriteFile(activityId, testPath + "/TestFile.txt", testUpdatedFile);
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);

            byte[] actual = ReadFile(testPath + "/TestFile.txt");
            Assert.AreEqual(Encoding.Default.GetString(testUpdatedFile), Encoding.Default.GetString(actual));
        }

        [Test]
        public void TestGetItemsReturnsIgnoreInfo()
        {
            string ignore = "*.bad\n";
            provider.MakeActivity(activityId);
            provider.SetProperty(activityId, testPath, "ignore", ignore);
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);

            FolderMetaData item = (FolderMetaData)provider.GetItems(-1, testPath, Recursion.OneLevel);

            Assert.AreEqual(ignore, item.Properties["ignore"]);
        }

        [Test]
        public void TestSetIgnoreListOnFolder()
        {
            string ignore = "*.bad\n";

            provider.MakeActivity(activityId);
            provider.SetProperty(activityId, testPath, "ignore", ignore);
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);

            FolderMetaData item = (FolderMetaData)provider.GetItems(-1, testPath, Recursion.OneLevel);
            Assert.AreEqual(ignore, item.Properties["ignore"]);
        }

        [Test]
        public void TestUpdateIgnoreListOnFolder()
        {
            string ignore1 = "*.bad\n";
            string ignore2 = "*.good\n";

            provider.MakeActivity(activityId);
            provider.SetProperty(activityId, testPath, "ignore", ignore1);
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);

            provider.MakeActivity(activityId);
            provider.SetProperty(activityId, testPath, "ignore", ignore2);
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);

            FolderMetaData item = (FolderMetaData)provider.GetItems(-1, testPath, Recursion.OneLevel);
            Assert.AreEqual(ignore2, item.Properties["ignore"]);
        }

        [Test]
        public void TestDeleteFile()
        {
            string path = testPath + "/TestFile.txt";
            CreateFile(path, "Test file contents");

            provider.MakeActivity(activityId);
            provider.DeleteItem(activityId, path);
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);

            Assert.IsFalse(provider.ItemExists(path));
        }

        [Test]
        public void TestDeleteFolder()
        {
            string path = testPath + "/TestFolder";
            CreateFolder(path);

            provider.MakeActivity(activityId);
            provider.DeleteItem(activityId, path);
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);

            Assert.IsFalse(provider.ItemExists(path));
        }

        [Test]
        public void TestGetLog()
        {
            int versionFrom = provider.GetLatestVersion();
            CreateFile(testPath + "/TestFile.txt", "Fun text");
            int versionTo = provider.GetLatestVersion();

            LogItem logItem = provider.GetLog(testPath, versionFrom, versionTo, Recursion.Full, Int32.MaxValue);

            Assert.AreEqual(2, logItem.History.Length);
        }

        [Test]
        public void TestGetChangedItemsWithOneNewFile()
        {
            int versionFrom = provider.GetLatestVersion();
            CreateFile(testPath + "/TestFile.txt", "Fun text");
            int versionTo = provider.GetLatestVersion();

            UpdateReportData reportData = new UpdateReportData();
            FolderMetaData folder = provider.GetChangedItems(testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(testPath.Substring(1), folder.Name);
            Assert.AreEqual(testPath.Substring(1) + "/TestFile.txt", folder.Items[0].Name);
            Assert.IsNotNull(folder.Items[0].DownloadUrl);
        }

        [Test]
        public void TestGetChangedItemsWithDeletedFile()
        {
            string path = testPath + "/TestFile.txt";
            CreateFile(path, "Test file contents");
            int versionFrom = provider.GetLatestVersion();
            DeleteItem(path);
            int versionTo = provider.GetLatestVersion();

            UpdateReportData reportData = new UpdateReportData();
            FolderMetaData folder = provider.GetChangedItems(testPath, versionFrom, versionTo, reportData);

            Assert.IsTrue(folder.Items[0] is DeleteMetaData);
            Assert.AreEqual(path.Substring(1), folder.Items[0].Name);
        }

        [Test]
        public void TestGetChangedItemsWithDeletedFolder()
        {
            string path = testPath + "/Test Folder";
            CreateFolder(path);
            int versionFrom = provider.GetLatestVersion();
            DeleteItem(path);
            int versionTo = provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();

            FolderMetaData folder = provider.GetChangedItems(testPath, versionFrom, versionTo, reportData);

            Assert.IsTrue(folder.Items[0] is DeleteFolderMetaData);
            Assert.AreEqual(path.Substring(1), folder.Items[0].Name);
        }

        [Test]
        public void TestGetChangedItemsWithSameFileUpdatedTwice()
        {
            string path = testPath + "/TestFile.txt";
            CreateFile(path, "Fun text");
            int versionFrom = provider.GetLatestVersion();
            UpdateFile(path, "Fun text 2");
            UpdateFile(path, "Fun text 3");
            int versionTo = provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();

            FolderMetaData folder = provider.GetChangedItems(testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(1, folder.Items.Count);
            Assert.AreEqual(path.Substring(1), folder.Items[0].Name);
            Assert.AreEqual("Fun text 3", Encoding.Default.GetString(ReadFile(path)));
        }

        [Test]
        public void TestGetItemsReturnsMimeTypeInfo()
        {
            string mimeType = "application/octet-stream";
            string path = testPath + "/TestFile.txt";
            CreateFile(path, "Fun text");
            provider.MakeActivity(activityId);
            provider.SetProperty(activityId, path, "mime-type", mimeType);
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);

            FolderMetaData item = (FolderMetaData)provider.GetItems(-1, testPath, Recursion.OneLevel);

            Assert.AreEqual(mimeType, item.Items[0].Properties["mime-type"]);
        }

        [Test]
        public void TestSetMimeTypeOnFile()
        {
            string mimeType = "application/octet-stream";
            string path = testPath + "/TestFile.txt";
            CreateFile(path, "Fun text");

            provider.MakeActivity(activityId);
            provider.SetProperty(activityId, path, "mime-type", mimeType);
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);

            FolderMetaData item = (FolderMetaData)provider.GetItems(-1, testPath, Recursion.OneLevel);
            Assert.AreEqual(mimeType, item.Items[0].Properties["mime-type"]);
        }

        [Test]
        public void TestUpdateMimeTypeOnFile()
        {
            string mimeType1 = "application/octet-stream1";
            string mimeType2 = "application/octet-stream2";
            string path = testPath + "/TestFile.txt";
            CreateFile(path, "Fun text");

            provider.MakeActivity(activityId);
            provider.SetProperty(activityId, path, "mime-type", mimeType1);
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);

            provider.MakeActivity(activityId);
            provider.SetProperty(activityId, path, "mime-type", mimeType2);
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);

            FolderMetaData item = (FolderMetaData)provider.GetItems(-1, testPath, Recursion.OneLevel);
            Assert.AreEqual(mimeType2, item.Items[0].Properties["mime-type"]);
        }

        [Test]
        public void TestGetChangedItemsDoesNotReturnItemIfInLocalEntriesList()
        {
            string path = testPath + "/TestFile.txt";
            CreateFile(path, "Fun text");
            int versionFrom = provider.GetLatestVersion();
            UpdateFile(path, "Fun text 2");
            int versionTo = provider.GetLatestVersion();

            UpdateReportData reportData = new UpdateReportData();
            reportData.Entries = new List<EntryData>();
            EntryData entry = new EntryData();
            entry.Rev = versionTo.ToString();
            entry.path = "TestFile.txt";
            reportData.Entries.Add(entry);
            FolderMetaData folder = provider.GetChangedItems(testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(0, folder.Items.Count);
        }

        [Test]
        public void TestGetChangedItemsDoesNotReturnDeletedItemIfInLocalState()
        {
            string path = testPath + "/TestFile.txt";
            CreateFile(path, "Fun text");
            int versionFrom = provider.GetLatestVersion();
            DeleteItem(path);
            int versionTo = provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();
            reportData.Missing = new List<string>();
            reportData.Missing.Add("TestFile.txt");

            FolderMetaData folder = provider.GetChangedItems(testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(0, folder.Items.Count);
        }

        [Test]
        public void CommitWithNoItemsReturnsLatestChangeset()
        {
            int startVersion = provider.GetLatestVersion();

            provider.MakeActivity(activityId);
            MergeActivityResponse response = provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);

            int endVersion = provider.GetLatestVersion();
            Assert.AreEqual(startVersion, response.Version);
            Assert.AreEqual(startVersion, endVersion);
        }

        void UpdateFile(string path,
                        string fileData)
        {
            byte[] data = Encoding.Default.GetBytes(fileData);
            string activityId = Guid.NewGuid().ToString();
            provider.MakeActivity(activityId);
            provider.WriteFile(activityId, path, data);
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);
        }

        void CreateFile(string path,
                        string fileData)
        {
            byte[] data = Encoding.Default.GetBytes(fileData);
            string activityId = Guid.NewGuid().ToString();
            provider.MakeActivity(activityId);
            provider.WriteFile(activityId, path, data);
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);
        }

        void DeleteItem(string path)
        {
            string activityId = Guid.NewGuid().ToString();
            provider.MakeActivity(activityId);
            provider.DeleteItem(activityId, path);
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);
        }

        void CreateFolder(string path)
        {
            string activityId = Guid.NewGuid().ToString();
            provider.MakeActivity(activityId);
            provider.MakeCollection(activityId, path);
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);
        }

        byte[] ReadFile(string path)
        {
            ItemMetaData item = provider.GetItems(-1, path, Recursion.None);
            return provider.ReadFile(item);
        }
    }
}
