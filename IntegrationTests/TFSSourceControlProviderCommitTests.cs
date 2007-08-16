using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.SourceControl;
using CodePlex.TfsLibrary.ObjectModel;

namespace Tests
{
    public class TFSSourceControlProviderCommitTests : TFSSourceControlProviderTestsBase
    {
        [Test]
        public void TestCommitNewFile()
        {
            byte[] testFile = GetBytes("Test file contents");

            bool created = _provider.WriteFile(_activityId, _testPath + "/TestFile.txt", testFile);
            MergeActivityResponse response = Commit();

            Assert.AreEqual(GetString(testFile), ReadFile(_testPath + "/TestFile.txt"));
            Assert.AreEqual(true, created);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath + "/TestFile.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitUpdatedFile()
        {
            WriteFile(_testPath + "/TestFile.txt", "Test file contents", true);
            byte[] testFile = GetBytes("Test file contents\r\nUpdated");

            bool created = _provider.WriteFile(_activityId, _testPath + "/TestFile.txt", testFile);
            MergeActivityResponse response = Commit();

            Assert.AreEqual(GetString(testFile), ReadFile(_testPath + "/TestFile.txt"));
            Assert.AreEqual(false, created);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(1, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath + "/TestFile.txt", ItemType.File));
        }

        [Test]
        public void TestCommitNewFolder()
        {
            _provider.MakeCollection(_activityId, _testPath + "/TestFolder");
            MergeActivityResponse response = Commit();

            Assert.IsTrue(_provider.ItemExists(_testPath + "/TestFolder"));
            Assert.AreEqual(ItemType.Folder, _provider.GetItems(-1, _testPath + "/TestFolder", Recursion.None).ItemType);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath + "/TestFolder", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitMultipleNewFiles()
        {
            byte[] testFile = GetBytes("Test file contents");

            _provider.WriteFile(_activityId, _testPath + "/TestFile1.txt", testFile);
            _provider.WriteFile(_activityId, _testPath + "/TestFile2.txt", testFile);
            _provider.WriteFile(_activityId, _testPath + "/TestFile3.txt", testFile);
            MergeActivityResponse response = Commit();

            Assert.AreEqual(GetString(testFile), ReadFile(_testPath + "/TestFile1.txt"));
            Assert.AreEqual(GetString(testFile), ReadFile(_testPath + "/TestFile2.txt"));
            Assert.AreEqual(GetString(testFile), ReadFile(_testPath + "/TestFile3.txt"));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(4, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath + "/TestFile1.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, _testPath + "/TestFile2.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, _testPath + "/TestFile3.txt", ItemType.File));
        }

        [Test]
        public void TestCommitNewSubFolderInNewFolder()
        {
            _provider.MakeCollection(_activityId, _testPath + "/TestFolder");
            _provider.MakeCollection(_activityId, _testPath + "/TestFolder/SubFolder");
            MergeActivityResponse response = Commit();

            Assert.IsTrue(_provider.ItemExists(_testPath + "/TestFolder/SubFolder"));
            Assert.AreEqual(ItemType.Folder, _provider.GetItems(-1, _testPath + "/TestFolder/SubFolder", Recursion.None).ItemType);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(3, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath + "/TestFolder", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, _testPath + "/TestFolder/SubFolder", ItemType.Folder));
        }

        [Test]
        public void TestCommitNewFileInNewFolder()
        {
            byte[] fileData = GetBytes("Test file contents");

            _provider.MakeCollection(_activityId, _testPath + "/TestFolder");
            _provider.WriteFile(_activityId, _testPath + "/TestFolder/TestFile.txt", fileData);
            MergeActivityResponse response = Commit();

            Assert.AreEqual(GetString(fileData), ReadFile(_testPath + "/TestFolder/TestFile.txt"));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(3, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath + "/TestFolder/TestFile.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, _testPath + "/TestFolder", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitDeleteFile()
        {
            string path = _testPath + "/TestFile.txt";
            WriteFile(path, "Test file contents", true);

            _provider.DeleteItem(_activityId, path);
            MergeActivityResponse response = Commit();

            Assert.IsFalse(_provider.ItemExists(path));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(1, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitDeleteFolder()
        {
            string path = _testPath + "/TestFolder";
            CreateFolder(path, true);

            _provider.DeleteItem(_activityId, path);
            MergeActivityResponse response = Commit();

            Assert.IsFalse(_provider.ItemExists(path));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(1, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitNewMimeTypePropertyOnFile()
        {
            string mimeType = "application/octet-stream";
            string path = _testPath + "/TestFile.txt";
            WriteFile(path, "Fun text", true);

            _provider.SetProperty(_activityId, path, "mime-type", mimeType);
            MergeActivityResponse response = Commit();

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.Full);
            Assert.AreEqual(mimeType, item.Items[0].Properties["mime-type"]);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(1, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath + "/TestFile.txt", ItemType.File));
        }

        [Test]
        public void TestCommitNewIgnoreListPropertyOnFolder()
        {
            string ignore = "*.bad\n";

            _provider.SetProperty(_activityId, _testPath, "ignore", ignore);
            MergeActivityResponse response = Commit();

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.Full);
            Assert.AreEqual(ignore, item.Properties["ignore"]);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(1, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitUpdateMimeTypePropertyOnFile()
        {
            string mimeType1 = "application/octet-stream1";
            string mimeType2 = "application/octet-stream2";
            string path = _testPath + "/TestFile.txt";
            WriteFile(path, "Fun text", false);
            SetProperty(path, "mime-type", mimeType1, true);

            _provider.SetProperty(_activityId, path, "mime-type", mimeType2);
            MergeActivityResponse response = Commit();

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.Full);
            Assert.AreEqual(mimeType2, item.Items[0].Properties["mime-type"]);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(1, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath + "/TestFile.txt", ItemType.File));
        }

        [Test]
        public void TestCommitUpdateIgnoreListPropertyOnFolder()
        {
            string ignore1 = "*.bad\n";
            string ignore2 = "*.good\n";
            SetProperty(_testPath, "ignore", ignore1, true);

            _provider.SetProperty(_activityId, _testPath, "ignore", ignore2);
            MergeActivityResponse response = Commit();

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.Full);
            Assert.AreEqual(ignore2, item.Properties["ignore"]);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(1, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitMultipleNewPropertiesOnMultipleFiles()
        {
            WriteFile(_testPath + "/TestFile1.txt", "Fun text", false);
            WriteFile(_testPath + "/TestFile2.txt", "Fun text", true);

            _provider.SetProperty(_activityId, _testPath + "/TestFile1.txt", "mime-type1", "mime1");
            _provider.SetProperty(_activityId, _testPath + "/TestFile1.txt", "mime-type2", "mime2");
            _provider.SetProperty(_activityId, _testPath + "/TestFile2.txt", "mime-type3", "mime3");
            _provider.SetProperty(_activityId, _testPath + "/TestFile2.txt", "mime-type4", "mime4");
            MergeActivityResponse response = Commit();

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.Full);
            Assert.AreEqual("mime1", item.Items[0].Properties["mime-type1"]);
            Assert.AreEqual("mime2", item.Items[0].Properties["mime-type2"]);
            Assert.AreEqual("mime3", item.Items[1].Properties["mime-type3"]);
            Assert.AreEqual("mime4", item.Items[1].Properties["mime-type4"]);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath + "/TestFile1.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, _testPath + "/TestFile2.txt", ItemType.File));
        }

        [Test]
        public void TestCommitMultipleNewPropertiesOnMultipleFolders()
        {
            CreateFolder(_testPath + "/Folder1", false);
            CreateFolder(_testPath + "/Folder2", true);

            _provider.SetProperty(_activityId, _testPath + "/Folder1", "mime-type1", "mime1");
            _provider.SetProperty(_activityId, _testPath + "/Folder1", "mime-type2", "mime2");
            _provider.SetProperty(_activityId, _testPath + "/Folder2", "mime-type3", "mime3");
            _provider.SetProperty(_activityId, _testPath + "/Folder2", "mime-type4", "mime4");
            MergeActivityResponse response = Commit();

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.Full);
            Assert.AreEqual("mime1", item.Items[0].Properties["mime-type1"]);
            Assert.AreEqual("mime2", item.Items[0].Properties["mime-type2"]);
            Assert.AreEqual("mime3", item.Items[1].Properties["mime-type3"]);
            Assert.AreEqual("mime4", item.Items[1].Properties["mime-type4"]);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath + "/Folder1", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, _testPath + "/Folder2", ItemType.Folder));
        }

        [Test]
        public void TestCommitNewPropertyOnNewFolderInSameCommit()
        {
            _provider.MakeCollection(_activityId, _testPath + "/Folder1");
            _provider.SetProperty(_activityId, _testPath + "/Folder1", "mime-type1", "mime1");
            MergeActivityResponse response = Commit();

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.Full);
            Assert.AreEqual("mime1", item.Items[0].Properties["mime-type1"]);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath + "/Folder1", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitNewPropertyOnNewFileInSameCommit()
        {
            byte[] fileData = GetBytes("test");

            _provider.WriteFile(_activityId, _testPath + "/TestFile1.txt", fileData);
            _provider.SetProperty(_activityId, _testPath + "/TestFile1.txt", "mime-type1", "mime1");
            MergeActivityResponse response = Commit();

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.Full);
            Assert.AreEqual("mime1", item.Items[0].Properties["mime-type1"]);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath + "/TestFile1.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitWithNoItemsReturnsLatestChangeset()
        {
            int startVersion = _provider.GetLatestVersion();

            MergeActivityResponse response = Commit();

            int endVersion = _provider.GetLatestVersion();
            Assert.AreEqual(startVersion, endVersion);
            Assert.AreEqual(endVersion, response.Version);
            Assert.AreEqual(0, response.Items.Count);
        }

        [Test]
        public void TestCommitRenameFile()
        {
            WriteFile(_testPath + "/Fun.txt", "Fun text", true);

            _provider.DeleteItem(_activityId, _testPath + "/Fun.txt");
            _provider.CopyItem(_activityId, _testPath + "/Fun.txt", _testPath + "/FunRename.txt");
            MergeActivityResponse response = Commit();

            LogItem log = _provider.GetLog(_testPath + "/FunRename.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, _testPath + "/FunRename.txt", ItemType.File));
        }

        [Test]
        public void TestCommitRenameFileWithCopyBeforeDelete()
        {
            WriteFile(_testPath + "/FunRename.txt", "Fun text", true);

            _provider.CopyItem(_activityId, _testPath + "/FunRename.txt", _testPath + "/Fun.txt");
            _provider.DeleteItem(_activityId, _testPath + "/FunRename.txt");
            MergeActivityResponse response = Commit();

            LogItem log = _provider.GetLog(_testPath + "/Fun.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath + "/Fun.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitRenameFolder()
        {
            CreateFolder(_testPath + "/Fun", true);

            _provider.DeleteItem(_activityId, _testPath + "/Fun");
            _provider.CopyItem(_activityId, _testPath + "/Fun", _testPath + "/FunRename");
            MergeActivityResponse response = Commit();

            LogItem log = _provider.GetLog(_testPath + "/FunRename", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, _testPath + "/FunRename", ItemType.Folder));
        }

        [Test]
        public void TestCommitBranchFile()
        {
            WriteFile(_testPath + "/Fun.txt", "Fun text", true);

            _provider.CopyItem(_activityId, _testPath + "/Fun.txt", _testPath + "/FunBranch.txt");
            MergeActivityResponse response = Commit();

            LogItem log = _provider.GetLog(_testPath + "/FunBranch.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Branch, log.History[0].Changes[0].ChangeType & ChangeType.Branch);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(0, response.Items.Count);
        }

        [Test]
        public void TestCommitBranchFolder()
        {
            CreateFolder(_testPath + "/Fun", true);

            _provider.CopyItem(_activityId, _testPath + "/Fun", _testPath + "/FunBranch");
            MergeActivityResponse response = Commit();

            LogItem log = _provider.GetLog(_testPath + "/FunBranch", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Branch, log.History[0].Changes[0].ChangeType & ChangeType.Branch);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(0, response.Items.Count);
        }

        [Test]
        public void TestCommitRenameAndEditFile()
        {
            WriteFile(_testPath + "/Fun.txt", "Fun text", true);
            byte[] updatedText = GetBytes("Test file contents");

            _provider.DeleteItem(_activityId, _testPath + "/Fun.txt");
            _provider.CopyItem(_activityId, _testPath + "/Fun.txt", _testPath + "/FunRename.txt");
            bool created = _provider.WriteFile(_activityId, _testPath + "/FunRename.txt", updatedText);
            MergeActivityResponse response = Commit();

            LogItem log = _provider.GetLog(_testPath + "/FunRename.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename | ChangeType.Edit, log.History[0].Changes[0].ChangeType);
            Assert.AreEqual(GetString(updatedText), ReadFile(_testPath + "/FunRename.txt"));
            Assert.AreEqual(false, created);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, _testPath + "/FunRename.txt", ItemType.File));
        }

        [Test]
        public void TestCommitOfMoveFileOutOfFolderAndDeleteFolder()
        {
            CreateFolder(_testPath + "/TestFolder", false);
            bool created = WriteFile(_testPath + "/TestFolder/TestFile.txt", "Test file contents", true);

            _provider.CopyItem(_activityId, _testPath + "/TestFolder/TestFile.txt", _testPath + "/FunFile.txt");
            _provider.DeleteItem(_activityId, _testPath + "/TestFolder");
            MergeActivityResponse response = Commit();

            LogItem log = _provider.GetLog(_testPath + "/FunFile.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
            Assert.IsNull(_provider.GetItems(-1, _testPath + "/TestFolder", Recursion.None));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath + "/FunFile.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitDeleteFileAlsoDeletesPropertiesOnFile()
        {
            string mimeType = "application/octet-stream";
            string path = _testPath + "/TestFile.txt";
            WriteFile(path, "Fun text", false);
            SetProperty(path, "mime-type", mimeType, true);

            _provider.DeleteItem(_activityId, _testPath + "/TestFile.txt");
            MergeActivityResponse response = Commit();

            ItemMetaData item = _provider.GetItems(-1, _testPath, Recursion.Full);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(1, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitMovedAndUpdatedFile()
        {
            CreateFolder(_testPath + "/Nodes", false);
            WriteFile(_testPath + "/Nodes/Fun.txt", "filedata", false);
            CreateFolder(_testPath + "/Protocol", true);
            byte[] fileData = GetBytes("filedata2");

            _provider.DeleteItem(_activityId, _testPath + "/Nodes/Fun.txt");
            _provider.CopyItem(_activityId, _testPath + "/Nodes/Fun.txt", _testPath + "/Protocol/Fun.txt");
            bool created = _provider.WriteFile(_activityId, _testPath + "/Protocol/Fun.txt", fileData);
            MergeActivityResponse response = Commit();

            LogItem log = _provider.GetLog(_testPath + "/Protocol/Fun.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename | ChangeType.Edit, log.History[0].Changes[0].ChangeType);
            Assert.AreEqual(GetString(fileData), ReadFile(_testPath + "/Protocol/Fun.txt"));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(3, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath + "/Protocol/Fun.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, _testPath + "/Protocol", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, _testPath + "/Nodes", ItemType.Folder));
        }

        [Test]
        public void TestCommitMovedFolderWithUpdatedFile()
        {
            CreateFolder(_testPath + "/A", false);
            WriteFile(_testPath + "/A/Test.txt", "filedata", false);
            CreateFolder(_testPath + "/B", true);
            byte[] fileData = GetBytes("filedata2");

            _provider.DeleteItem(_activityId, _testPath + "/A");
            _provider.CopyItem(_activityId, _testPath + "/A", _testPath + "/B/A");
            bool created = _provider.WriteFile(_activityId, _testPath + "/B/A/Test.txt", fileData);
            MergeActivityResponse response = Commit();

            LogItem log = _provider.GetLog(_testPath + "/B/A/Test.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename | ChangeType.Edit, log.History[0].Changes[0].ChangeType);
            Assert.AreEqual(GetString(fileData), ReadFile(_testPath + "/B/A/Test.txt"));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(4, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath + "/B/A/Test.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, _testPath + "/B/A", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, _testPath + "/B", ItemType.Folder));
        }

        [Test]
        public void TestCommitMovedFileFromDeletedFolder()
        {
            CreateFolder(_testPath + "/A", false);
            WriteFile(_testPath + "/A/Test.txt", "filedata", true);

            _provider.DeleteItem(_activityId, _testPath + "/A");
            _provider.CopyItem(_activityId, _testPath + "/A/Test.txt", _testPath + "/Test.txt");
            MergeActivityResponse response = Commit();

            LogItem log = _provider.GetLog(_testPath + "/Test.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
            Assert.IsFalse(_provider.ItemExists(_testPath + "/A"));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath + "/Test.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitMultipleMovedFilesFromDeletedFolder()
        {
            CreateFolder(_testPath + "/A", false);
            WriteFile(_testPath + "/A/Test1.txt", "filedata", false);
            WriteFile(_testPath + "/A/Test2.txt", "filedata", true);

            _provider.DeleteItem(_activityId, _testPath + "/A");
            _provider.CopyItem(_activityId, _testPath + "/A/Test1.txt", _testPath + "/Test1.txt");
            _provider.CopyItem(_activityId, _testPath + "/A/Test2.txt", _testPath + "/Test2.txt");
            MergeActivityResponse response = Commit();

            LogItem log = _provider.GetLog(_testPath + "/Test1.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
            log = _provider.GetLog(_testPath + "/Test2.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
            Assert.IsFalse(_provider.ItemExists(_testPath + "/A"));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(3, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath + "/Test1.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, _testPath + "/Test2.txt", ItemType.File));
        }

        [Test]
        public void TestCommitMoveMultipleFilesFromDeletedFolderIntoNewFolder()
        {
            CreateFolder(_testPath + "/A", false);
            WriteFile(_testPath + "/A/Test1.txt", "filedata", false);
            WriteFile(_testPath + "/A/Test2.txt", "filedata", true);

            _provider.DeleteItem(_activityId, _testPath + "/A");
            _provider.MakeCollection(_activityId, _testPath + "/B");
            _provider.CopyItem(_activityId, _testPath + "/A/Test1.txt", _testPath + "/B/Test1.txt");
            _provider.CopyItem(_activityId, _testPath + "/A/Test2.txt", _testPath + "/B/Test2.txt");
            MergeActivityResponse response = Commit();

            LogItem log1 = _provider.GetLog(_testPath + "/B/Test1.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log1.History[0].Changes[0].ChangeType);
            LogItem log2 = _provider.GetLog(_testPath + "/B/Test2.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log2.History[0].Changes[0].ChangeType);
            Assert.IsFalse(_provider.ItemExists(_testPath + "/A"));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(4, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath + "/B/Test1.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, _testPath + "/B", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, _testPath + "/B/Test2.txt", ItemType.File));
        }

        [Test]
        public void TestCommitRenamedFolderAndDeleteFileWithinFolder()
        {
            CreateFolder(_testPath + "/A", false);
            WriteFile(_testPath + "/A/Test1.txt", "filedata", true);

            _provider.DeleteItem(_activityId, _testPath + "/A");
            _provider.CopyItem(_activityId, _testPath + "/A", _testPath + "/B");
            _provider.DeleteItem(_activityId, _testPath + "/B/Test1.txt");
            MergeActivityResponse response = Commit();

            LogItem log1 = _provider.GetLog(_testPath + "/B", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log1.History[0].Changes[0].ChangeType);
            Assert.IsFalse(_provider.ItemExists(_testPath + "/B/Test1.txt"));
            Assert.IsFalse(_provider.ItemExists(_testPath + "/A"));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath + "/B", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitRenamedFolderContainingRenamedFile()
        {
            CreateFolder(_testPath + "/A", false);
            WriteFile(_testPath + "/A/Test1.txt", "filedata", true);

            _provider.DeleteItem(_activityId, _testPath + "/A");
            _provider.CopyItem(_activityId, _testPath + "/A", _testPath + "/B");
            _provider.DeleteItem(_activityId, _testPath + "/B/Test1.txt");
            _provider.CopyItem(_activityId, _testPath + "/A/Test1.txt", _testPath + "/B/Test2.txt");
            MergeActivityResponse response = Commit();

            LogItem log1 = _provider.GetLog(_testPath + "/B", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log1.History[0].Changes[0].ChangeType);
            LogItem log2 = _provider.GetLog(_testPath + "/B/Test2.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log2.History[0].Changes[0].ChangeType);
            Assert.IsFalse(_provider.ItemExists(_testPath + "/A"));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(3, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath + "/B", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, _testPath + "/B/Test2.txt", ItemType.File));
        }

        [Test]
        public void TestCommitMovedAndUpdatedFileThenDeleteFolderThatContainedFile()
        {
            CreateFolder(_testPath + "/A", false);
            CreateFolder(_testPath + "/B", false);
            WriteFile(_testPath + "/A/Test1.txt", "filedata", true);

            _provider.DeleteItem(_activityId, _testPath + "/A");
            _provider.CopyItem(_activityId, _testPath + "/A/Test1.txt", _testPath + "/B/Test1.txt");
            _provider.WriteFile(_activityId, _testPath + "/B/Test1.txt", GetBytes("filedata2"));
            MergeActivityResponse response = Commit();

            LogItem log2 = _provider.GetLog(_testPath + "/B/Test1.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename | ChangeType.Edit, log2.History[0].Changes[0].ChangeType);
            Assert.AreEqual("filedata2", ReadFile(_testPath + "/B/Test1.txt"));
            Assert.IsFalse(_provider.ItemExists(_testPath + "/A"));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(3, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, _testPath + "/B/Test1.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, _testPath + "/B", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, _testPath, ItemType.Folder));
        }
    }
}
