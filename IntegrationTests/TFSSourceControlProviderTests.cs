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
            Commit();
        }

        [TearDown]
        public void TearDown()
        {
            DeleteItem(testPath, false);
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);
        }

        [Test]
        public void TestCommitFolder()
        {
            CreateFolder(testPath + "/TestFolder", true);

            Assert.IsTrue(provider.ItemExists(testPath + "/TestFolder"));
            Assert.AreEqual(ItemType.Folder, provider.GetItems(-1, testPath + "/TestFolder", Recursion.None).ItemType);
        }

        [Test]
        public void TestCommitFolderAndSubFolder()
        {
            CreateFolder(testPath + "/TestFolder", false);
            CreateFolder(testPath + "/TestFolder/SubFolder", true);

            Assert.IsTrue(provider.ItemExists(testPath + "/TestFolder/SubFolder"));
            Assert.AreEqual(ItemType.Folder, provider.GetItems(-1, testPath + "/TestFolder/SubFolder", Recursion.None).ItemType);
        }

        [Test]
        public void TestCommitNewFile()
        {
            byte[] testFile = Encoding.Default.GetBytes("Test file contents");

            CreateFile(testPath + "/TestFile.txt", testFile, true);

            byte[] actual = ReadFile(testPath + "/TestFile.txt");
            Assert.AreEqual(Encoding.Default.GetString(testFile), Encoding.Default.GetString(actual));
        }

        [Test]
        public void TestCommitMultipleNewFiles()
        {
            byte[] testFile = Encoding.Default.GetBytes("Test file contents");

            CreateFile(testPath + "/TestFile1.txt", testFile, false);
            CreateFile(testPath + "/TestFile2.txt", testFile, false);
            CreateFile(testPath + "/TestFile3.txt", testFile, true);

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

            CreateFolder(testPath + "/TestFolder", false);
            CreateFile(testPath + "/TestFolder/TestFile.txt", testFile, true);

            byte[] actual = ReadFile(testPath + "/TestFolder/TestFile.txt");
            Assert.AreEqual(Encoding.Default.GetString(testFile), Encoding.Default.GetString(actual));
        }

        [Test]
        public void TestCommitUpdatedFile()
        {
            CreateFile(testPath + "/TestFile.txt", "Test file contents", true);
            byte[] testUpdatedFile = Encoding.Default.GetBytes("Test file contents\r\nUpdated");

            CreateFile(testPath + "/TestFile.txt", testUpdatedFile, true);

            byte[] actual = ReadFile(testPath + "/TestFile.txt");
            Assert.AreEqual(Encoding.Default.GetString(testUpdatedFile), Encoding.Default.GetString(actual));
        }

        [Test]
        public void TestGetItemsReturnsIgnoreInfo()
        {
            string ignore = "*.bad\n";
            SetProperty(testPath, "ignore", ignore, true);

            FolderMetaData item = (FolderMetaData)provider.GetItems(-1, testPath, Recursion.OneLevel);

            Assert.AreEqual(ignore, item.Properties["ignore"]);
        }

        [Test]
        public void TestSetIgnoreListOnFolder()
        {
            string ignore = "*.bad\n";

            SetProperty(testPath, "ignore", ignore, true);

            FolderMetaData item = (FolderMetaData)provider.GetItems(-1, testPath, Recursion.OneLevel);
            Assert.AreEqual(ignore, item.Properties["ignore"]);
        }

        [Test]
        public void TestUpdateIgnoreListOnFolder()
        {
            string ignore1 = "*.bad\n";
            string ignore2 = "*.good\n";
            SetProperty(testPath, "ignore", ignore1, true);

            SetProperty(testPath, "ignore", ignore2, true);

            FolderMetaData item = (FolderMetaData)provider.GetItems(-1, testPath, Recursion.OneLevel);
            Assert.AreEqual(ignore2, item.Properties["ignore"]);
        }

        [Test]
        public void TestDeleteFile()
        {
            string path = testPath + "/TestFile.txt";
            CreateFile(path, "Test file contents", true);

            DeleteItem(path, true);

            Assert.IsFalse(provider.ItemExists(path));
        }

        [Test]
        public void TestDeleteFolder()
        {
            string path = testPath + "/TestFolder";
            CreateFolder(path, true);

            DeleteItem(path, true);

            Assert.IsFalse(provider.ItemExists(path));
        }

        [Test]
        public void TestGetLog()
        {
            int versionFrom = provider.GetLatestVersion();
            CreateFile(testPath + "/TestFile.txt", "Fun text", true);
            int versionTo = provider.GetLatestVersion();

            LogItem logItem = provider.GetLog(testPath, versionFrom, versionTo, Recursion.Full, Int32.MaxValue);

            Assert.AreEqual(2, logItem.History.Length);
        }

        [Test]
        public void TestGetChangedItemsWithOneNewFile()
        {
            int versionFrom = provider.GetLatestVersion();
            CreateFile(testPath + "/TestFile.txt", "Fun text", true);
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
            CreateFile(path, "Test file contents", true);
            int versionFrom = provider.GetLatestVersion();
            DeleteItem(path, true);
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
            CreateFolder(path, true);
            int versionFrom = provider.GetLatestVersion();
            DeleteItem(path, true);
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
            CreateFile(path, "Fun text", true);
            int versionFrom = provider.GetLatestVersion();
            UpdateFile(path, "Fun text 2", true);
            UpdateFile(path, "Fun text 3", true);
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
            CreateFile(path, "Fun text", false);
            SetProperty(path, "mime-type", mimeType, true);

            FolderMetaData item = (FolderMetaData)provider.GetItems(-1, testPath, Recursion.OneLevel);

            Assert.AreEqual(mimeType, item.Items[0].Properties["mime-type"]);
        }

        [Test]
        public void TestSetMimeTypeOnFile()
        {
            string mimeType = "application/octet-stream";
            string path = testPath + "/TestFile.txt";
            CreateFile(path, "Fun text", true);

            SetProperty(path, "mime-type", mimeType, true);

            FolderMetaData item = (FolderMetaData)provider.GetItems(-1, testPath, Recursion.OneLevel);
            Assert.AreEqual(mimeType, item.Items[0].Properties["mime-type"]);
        }

        [Test]
        public void TestUpdateMimeTypeOnFile()
        {
            string mimeType1 = "application/octet-stream1";
            string mimeType2 = "application/octet-stream2";
            string path = testPath + "/TestFile.txt";
            CreateFile(path, "Fun text", false);
            SetProperty(path, "mime-type", mimeType1, true);

            SetProperty(path, "mime-type", mimeType2, true);

            FolderMetaData item = (FolderMetaData)provider.GetItems(-1, testPath, Recursion.OneLevel);
            Assert.AreEqual(mimeType2, item.Items[0].Properties["mime-type"]);
        }

        [Test]
        public void TestGetChangedItemsDoesNotReturnItemIfInLocalEntriesList()
        {
            string path = testPath + "/TestFile.txt";
            CreateFile(path, "Fun text", true);
            int versionFrom = provider.GetLatestVersion();
            UpdateFile(path, "Fun text 2", true);
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
            CreateFile(path, "Fun text", true);
            int versionFrom = provider.GetLatestVersion();
            DeleteItem(path, true);
            int versionTo = provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();
            reportData.Missing = new List<string>();
            reportData.Missing.Add("TestFile.txt");

            FolderMetaData folder = provider.GetChangedItems(testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(0, folder.Items.Count);
        }

        [Test]
        public void TestGetChangedItemsWithNewFileInNewFolderHasCorrectPaths()
        {
            int versionFrom = provider.GetLatestVersion();
            CreateFolder(testPath + "/New Folder", false);
            CreateFile(testPath + "/New Folder/New File.txt", "Fun text", true);
            int versionTo = provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();

            FolderMetaData folder = provider.GetChangedItems(testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(1, folder.Items.Count);
            Assert.AreEqual(testPath.Substring(1) + "/New Folder", folder.Items[0].Name);
            Assert.AreEqual(1, ((FolderMetaData)folder.Items[0]).Items.Count);
            Assert.AreEqual(testPath.Substring(1) + "/New Folder/New File.txt", ((FolderMetaData)folder.Items[0]).Items[0].Name);
        }

        [Test]
        public void CommitWithNoItemsReturnsLatestChangeset()
        {
            int startVersion = provider.GetLatestVersion();

            MergeActivityResponse response = provider.MergeActivity(activityId);

            int endVersion = provider.GetLatestVersion();
            Assert.AreEqual(startVersion, response.Version);
            Assert.AreEqual(startVersion, endVersion);
        }

        [Test]
        public void TestGetItemsForRootSucceeds()
        {
            FolderMetaData item = (FolderMetaData)provider.GetItems(-1, "", Recursion.OneLevel);
        }

        [Test]
        public void TestGetChangedItemsForADeletedFileReturnsCorrectResult()
        {
            CreateFolder(testPath + "/New Folder", false);
            CreateFile(testPath + "/New Folder/New File.txt", "Fun text", true);
            int versionFrom = provider.GetLatestVersion();
            DeleteItem(testPath + "/New Folder", true);
            int versionTo = provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();
            reportData.UpdateTarget = "New File.txt";

            FolderMetaData folder = provider.GetChangedItems(testPath + "/New Folder", versionFrom, versionTo, reportData);

            Assert.AreEqual(1, folder.Items.Count);
            Assert.AreEqual("New File.txt", folder.Items[0].Name);
            Assert.AreEqual(typeof(DeleteMetaData), folder.Items[0].GetType());
        }

        [Test]
        public void TestGetChangedItemsReturnsNothingWhenClientStateContainsDeletedItem()
        {
            CreateFolder(testPath + "/New Folder", true);
            int versionFrom = provider.GetLatestVersion();
            DeleteItem(testPath + "/New Folder", true);
            CreateFolder(testPath + "/New Folder", true);
            int versionTo = provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();
            EntryData entry = new EntryData();
            reportData.Entries = new List<EntryData>();
            entry.Rev = versionFrom.ToString();
            reportData.Entries.Add(entry);
            entry = new EntryData();
            entry.path = "New Folder";
            entry.Rev = versionTo.ToString();
            reportData.Entries.Add(entry);

            FolderMetaData folder = provider.GetChangedItems(testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(0, folder.Items.Count);
        }

        [Test, ExpectedException(typeof(FolderAlreadyExistsException))]
        public void TestAddFolderThatAlreadyExistsThrowsException()
        {
            CreateFolder(testPath + "/New Folder", true);

            provider.MakeCollection(activityId, testPath + "/New Folder");
        }

        void UpdateFile(string path,
                        string fileData,
                        bool commit)
        {
            byte[] data = Encoding.Default.GetBytes(fileData);
            provider.WriteFile(activityId, path, data);
            if (commit)
                Commit();
        }

        void CreateFile(string path,
                        string fileData,
                        bool commit)
        {
            byte[] data = Encoding.Default.GetBytes(fileData);
            CreateFile(path, data, commit);
        }

        void CreateFile(string path,
                        byte[] fileData,
                        bool commit)
        {
            provider.WriteFile(activityId, path, fileData);
            if (commit)
                Commit();
        }

        void Commit()
        {
            provider.MergeActivity(activityId);
            provider.DeleteActivity(activityId);
            provider.MakeActivity(activityId);
        }

        void DeleteItem(string path,
                        bool commit)
        {
            provider.DeleteItem(activityId, path);
            if (commit)
                Commit();
        }

        void CreateFolder(string path,
                        bool commit)
        {
            provider.MakeCollection(activityId, path);
            if (commit)
                Commit();
        }

        byte[] ReadFile(string path)
        {
            ItemMetaData item = provider.GetItems(-1, path, Recursion.None);
            return provider.ReadFile(item);
        }

        void SetProperty(string path, string name, string value, bool commit)
        {
            provider.SetProperty(activityId, path, name, value);
            if (commit)
                Commit();
        }
    }
}
