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
        public void TestGetChangedItemsWithAddedFile()
        {
            int versionFrom = _provider.GetLatestVersion();
            WriteFile(_testPath + "/TestFile.txt", "Fun text", true);
            int versionTo = _provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();

            FolderMetaData folder = _provider.GetChangedItems(_testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(_testPath.Substring(1), folder.Name);
            Assert.AreEqual(_testPath.Substring(1) + "/TestFile.txt", folder.Items[0].Name);
            Assert.IsNotNull(folder.Items[0].DownloadUrl);
        }

        [Test]
        public void TestGetChangedItemsWithDeletedFile()
        {
            string path = _testPath + "/TestFile.txt";
            WriteFile(path, "Test file contents", true);
            int versionFrom = _provider.GetLatestVersion();
            DeleteItem(path, true);
            int versionTo = _provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();

            FolderMetaData folder = _provider.GetChangedItems(_testPath, versionFrom, versionTo, reportData);

            Assert.IsTrue(folder.Items[0] is DeleteMetaData);
            Assert.AreEqual(path.Substring(1), folder.Items[0].Name);
        }

        [Test]
        public void TestGetChangedItemsWithDeletedFolder()
        {
            string path = _testPath + "/Test Folder";
            CreateFolder(path, true);
            int versionFrom = _provider.GetLatestVersion();
            DeleteItem(path, true);
            int versionTo = _provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();

            FolderMetaData folder = _provider.GetChangedItems(_testPath, versionFrom, versionTo, reportData);

            Assert.IsTrue(folder.Items[0] is DeleteFolderMetaData);
            Assert.AreEqual(path.Substring(1), folder.Items[0].Name);
        }

        [Test]
        public void TestGetChangedItemsWithRenamedFile()
        {
            WriteFile(_testPath + "/Fun.txt", "Fun text", true);
            int versionFrom = _provider.GetLatestVersion();
            MoveItem(_testPath + "/Fun.txt", _testPath + "/FunRename.txt", true);
            int versionTo = _provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();

            FolderMetaData folder = _provider.GetChangedItems(_testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(_testPath.Substring(1), folder.Name);
            Assert.AreEqual(2, folder.Items.Count);
            Assert.AreEqual(_testPath.Substring(1) + "/Fun.txt", folder.Items[0].Name);
            Assert.IsTrue(folder.Items[0] is DeleteMetaData);
            Assert.AreEqual(_testPath.Substring(1) + "/FunRename.txt", folder.Items[1].Name);
            Assert.IsTrue(folder.Items[1] is ItemMetaData);
        }

        [Test]
        public void TestGetChangedItemsWithSameFileUpdatedTwice()
        {
            string path = _testPath + "/TestFile.txt";
            WriteFile(path, "Fun text", true);
            int versionFrom = _provider.GetLatestVersion();
            UpdateFile(path, "Fun text 2", true);
            UpdateFile(path, "Fun text 3", true);
            int versionTo = _provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();

            FolderMetaData folder = _provider.GetChangedItems(_testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(1, folder.Items.Count);
            Assert.AreEqual(path.Substring(1), folder.Items[0].Name);
            Assert.AreEqual("Fun text 3", GetString(ReadFile(path)));
        }

        [Test]
        public void TestGetChangedItemsWithNewFileInNewFolderInSameChangeset()
        {
            int versionFrom = _provider.GetLatestVersion();
            CreateFolder(_testPath + "/New Folder", false);
            WriteFile(_testPath + "/New Folder/New File.txt", "Fun text", true);
            int versionTo = _provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();

            FolderMetaData folder = _provider.GetChangedItems(_testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(1, folder.Items.Count);
            Assert.AreEqual(_testPath.Substring(1) + "/New Folder", folder.Items[0].Name);
            Assert.AreEqual(1, ((FolderMetaData)folder.Items[0]).Items.Count);
            Assert.AreEqual(_testPath.Substring(1) + "/New Folder/New File.txt", ((FolderMetaData)folder.Items[0]).Items[0].Name);
        }

        [Test]
        public void TestGetChangedItemsForFileThatWasDeleted()
        {
            CreateFolder(_testPath + "/New Folder", false);
            WriteFile(_testPath + "/New Folder/New File.txt", "Fun text", true);
            int versionFrom = _provider.GetLatestVersion();
            DeleteItem(_testPath + "/New Folder", true);
            int versionTo = _provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();
            reportData.UpdateTarget = "New File.txt";

            FolderMetaData folder = _provider.GetChangedItems(_testPath + "/New Folder", versionFrom, versionTo, reportData);

            Assert.AreEqual(1, folder.Items.Count);
            Assert.AreEqual("New File.txt", folder.Items[0].Name);
            Assert.AreEqual(typeof(DeleteMetaData), folder.Items[0].GetType());
        }

        [Test]
        public void TestGetChangedItemsWithAddedFileReturnsNothingWhenClientStateAlreadyCurrent()
        {
            string path = _testPath + "/TestFile.txt";
            WriteFile(path, "Fun text", true);
            int versionFrom = _provider.GetLatestVersion();
            UpdateFile(path, "Fun text 2", true);
            int versionTo = _provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();
            reportData.Entries = new List<EntryData>();
            EntryData entry = new EntryData();
            entry.Rev = versionTo.ToString();
            entry.path = "TestFile.txt";
            reportData.Entries.Add(entry);

            FolderMetaData folder = _provider.GetChangedItems(_testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(0, folder.Items.Count);
        }

        [Test]
        public void TestGetChangedItemsWithDeletedFolderReturnsNothingWhenClientStateAlreadyCurrent()
        {
            string path = _testPath + "/FolderA";
            CreateFolder(path, true);
            int versionFrom = _provider.GetLatestVersion();
            DeleteItem(path, true);
            int versionTo = _provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();
            reportData.Entries = new List<EntryData>();
            EntryData entry = new EntryData();
            entry.Rev = versionFrom.ToString();
            reportData.Entries.Add(entry);
            reportData.Missing = new List<string>();
            reportData.Missing.Add("FolderA");

            FolderMetaData folder = _provider.GetChangedItems(_testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(0, folder.Items.Count);
        }

        [Test]
        public void TestGetChangedItemsWithDeletedFolderContainingFilesReturnsNothingWhenClientStateAlreadyCurrent()
        {
            CreateFolder(_testPath + "/FolderA", false);
            WriteFile(_testPath + "/FolderA/Test1.txt", "filedata", true);
            int versionFrom = _provider.GetLatestVersion();
            DeleteItem(_testPath + "/FolderA", true);
            int versionTo = _provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();
            reportData.Entries = new List<EntryData>();
            EntryData entry = new EntryData();
            entry.Rev = versionFrom.ToString();
            reportData.Entries.Add(entry);
            reportData.Missing = new List<string>();
            reportData.Missing.Add("FolderA");

            FolderMetaData folder = _provider.GetChangedItems(_testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(0, folder.Items.Count);
        }

        [Test]
        public void TestGetChangedItemsWithDeletedFileReturnsNothingWhenClientStateAlreadyCurrent()
        {
            string path = _testPath + "/TestFile.txt";
            WriteFile(path, "Fun text", true);
            int versionFrom = _provider.GetLatestVersion();
            DeleteItem(path, true);
            int versionTo = _provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();
            reportData.Missing = new List<string>();
            reportData.Missing.Add("TestFile.txt");

            FolderMetaData folder = _provider.GetChangedItems(_testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(0, folder.Items.Count);
        }

        [Test]
        public void TestGetChangedItemsWithRenamedFileReturnsNothingWhenClientStateAlreadyCurrent()
        {
            CreateFolder(_testPath + "/Folder", false);
            WriteFile(_testPath + "/Fun.txt", "Fun text", true);
            int versionFrom = _provider.GetLatestVersion();
            MoveItem(_testPath + "/Fun.txt", _testPath + "/FunRenamed.txt", true);
            int versionTo = _provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();
            reportData.Missing = new List<string>();
            reportData.Missing.Add("Fun.txt");
            reportData.Entries = new List<EntryData>();
            EntryData entry = new EntryData();
            entry.Rev = versionTo.ToString();
            entry.path = "FunRenamed.txt";
            reportData.Entries.Add(entry);

            FolderMetaData folder = _provider.GetChangedItems(_testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(0, folder.Items.Count);
        }

        [Test]
        public void TestGetChangedItemsWithDeletedAndReAddedItemReturnsNothingWhenClientStateAlreadyCurrent()
        {
            CreateFolder(_testPath + "/New Folder", true);
            int versionFrom = _provider.GetLatestVersion();
            DeleteItem(_testPath + "/New Folder", true);
            CreateFolder(_testPath + "/New Folder", true);
            int versionTo = _provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();
            EntryData entry = new EntryData();
            reportData.Entries = new List<EntryData>();
            entry.Rev = versionFrom.ToString();
            reportData.Entries.Add(entry);
            entry = new EntryData();
            entry.path = "New Folder";
            entry.Rev = versionTo.ToString();
            reportData.Entries.Add(entry);

            FolderMetaData folder = _provider.GetChangedItems(_testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(0, folder.Items.Count);
        }

        [Test]
        public void TestGetChangedItemsWithUpdatedFolderProperty()
        {
            CreateFolder(_testPath + "/Folder1", true);
            int versionFrom = _provider.GetLatestVersion();
            SetProperty(_testPath + "/Folder1", "prop1", "prop1value", true);
            int versionTo = _provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();

            FolderMetaData folder = _provider.GetChangedItems(_testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(1, folder.Items.Count);
            Assert.AreEqual(1, folder.Items[0].Properties.Count);
            Assert.AreEqual(_testPath.Substring(1) + "/Folder1", folder.Items[0].Name);
            Assert.AreEqual("prop1value", folder.Items[0].Properties["prop1"]);
        }

        [Test]
        public void TestGetChangedItemsWithUpdatedFileProperty()
        {
            WriteFile(_testPath + "/Test1.txt", "filedata", true);
            int versionFrom = _provider.GetLatestVersion();
            SetProperty(_testPath + "/Test1.txt", "prop1", "prop1value", true);
            int versionTo = _provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();

            FolderMetaData folder = _provider.GetChangedItems(_testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(1, folder.Items.Count);
            Assert.AreEqual(1, folder.Items[0].Properties.Count);
            Assert.AreEqual(_testPath.Substring(1) + "/Test1.txt", folder.Items[0].Name);
            Assert.AreEqual("prop1value", folder.Items[0].Properties["prop1"]);
        }

        [Test]
        public void TestGetChangedItemsWithAddedFileContainingProperty()
        {
            int versionFrom = _provider.GetLatestVersion();
            WriteFile(_testPath + "/Test1.txt", "filedata", false);
            SetProperty(_testPath + "/Test1.txt", "prop1", "prop1value", true);
            int versionTo = _provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();

            FolderMetaData folder = _provider.GetChangedItems(_testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(1, folder.Items.Count);
            Assert.AreEqual(1, folder.Items[0].Properties.Count);
            Assert.AreEqual(_testPath.Substring(1) + "/Test1.txt", folder.Items[0].Name);
            Assert.AreEqual("prop1value", folder.Items[0].Properties["prop1"]);
        }

        [Test]
        public void TestGetChangedItemsWithAddedFolderContainingProperty()
        {
            int versionFrom = _provider.GetLatestVersion();
            CreateFolder(_testPath + "/Folder1", false);
            SetProperty(_testPath + "/Folder1", "prop1", "prop1value", true);
            int versionTo = _provider.GetLatestVersion();
            UpdateReportData reportData = new UpdateReportData();

            FolderMetaData folder = _provider.GetChangedItems(_testPath, versionFrom, versionTo, reportData);

            Assert.AreEqual(1, folder.Items.Count);
            Assert.AreEqual(1, folder.Items[0].Properties.Count);
            Assert.AreEqual(_testPath.Substring(1) + "/Folder1", folder.Items[0].Name);
            Assert.AreEqual("prop1value", folder.Items[0].Properties["prop1"]);
        }
    }
}
