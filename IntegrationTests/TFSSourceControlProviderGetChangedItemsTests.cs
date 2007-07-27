using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using SvnBridge.Protocol;
using SvnBridge.SourceControl;

namespace Tests
{
    [TestFixture]
    public class TFSSourceControlProviderGetChangedItemsTests : TFSSourceControlProviderTestsBase
    {
        [Test]
        public void TestGetChangedItemsWithOneNewFile()
        {
            int versionFrom = provider.GetLatestVersion();
            WriteFile(testPath + "/TestFile.txt", "Fun text", true);
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
            WriteFile(path, "Test file contents", true);
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
            WriteFile(path, "Fun text", true);
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
        public void TestGetChangedItemsDoesNotReturnItemIfInLocalEntriesList()
        {
            string path = testPath + "/TestFile.txt";
            WriteFile(path, "Fun text", true);
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
            WriteFile(path, "Fun text", true);
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
            WriteFile(testPath + "/New Folder/New File.txt", "Fun text", true);
            int versionTo = provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();

            FolderMetaData folder = provider.GetChangedItems(testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(1, folder.Items.Count);
            Assert.AreEqual(testPath.Substring(1) + "/New Folder", folder.Items[0].Name);
            Assert.AreEqual(1, ((FolderMetaData)folder.Items[0]).Items.Count);
            Assert.AreEqual(testPath.Substring(1) + "/New Folder/New File.txt", ((FolderMetaData)folder.Items[0]).Items[0].Name);
        }

        [Test]
        public void TestGetChangedItemsForADeletedFileReturnsCorrectResult()
        {
            CreateFolder(testPath + "/New Folder", false);
            WriteFile(testPath + "/New Folder/New File.txt", "Fun text", true);
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

        [Test]
        public void TestGetChangedItemsWithRenamedFile()
        {
            WriteFile(testPath + "/Fun.txt", "Fun text", true);
            int versionFrom = provider.GetLatestVersion();
            MoveItem(testPath + "/Fun.txt", testPath + "/FunRename.txt", true);
            int versionTo = provider.GetLatestVersion();

            UpdateReportData reportData = new UpdateReportData();
            FolderMetaData folder = provider.GetChangedItems(testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(testPath.Substring(1), folder.Name);
            Assert.AreEqual(2, folder.Items.Count);
            Assert.AreEqual(testPath.Substring(1) + "/Fun.txt", folder.Items[0].Name);
            Assert.IsTrue(folder.Items[0] is DeleteMetaData);
            Assert.AreEqual(testPath.Substring(1) + "/FunRename.txt", folder.Items[1].Name);
            Assert.IsTrue(folder.Items[1] is ItemMetaData);
        }
    }
}
