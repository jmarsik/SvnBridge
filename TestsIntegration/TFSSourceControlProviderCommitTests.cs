using System;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using NUnit.Framework;
using SvnBridge.Exceptions;
using SvnBridge.SourceControl;

namespace Tests
{
    public class TFSSourceControlProviderCommitTests : TFSSourceControlProviderTestsBase
    {
        [Test]
        public void TestCommitBranchFile()
        {
            WriteFile(testPath + "/Fun.txt", "Fun text", true);

            _provider.CopyItem(_activityId, testPath + "/Fun.txt", testPath + "/FunBranch.txt");
            MergeActivityResponse response = Commit();

            LogItem log =
                _provider.GetLog(testPath + "/FunBranch.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Branch, log.History[0].Changes[0].ChangeType & ChangeType.Branch);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(0, response.Items.Count);
        }

        [Test]
        public void TestCommitBranchFolder()
        {
            CreateFolder(testPath + "/Fun", true);

            _provider.CopyItem(_activityId, testPath + "/Fun", testPath + "/FunBranch");
            MergeActivityResponse response = Commit();

            LogItem log = _provider.GetLog(testPath + "/FunBranch", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Branch, log.History[0].Changes[0].ChangeType & ChangeType.Branch);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(0, response.Items.Count);
        }

        [Test]
        public void TestCommitDeleteFile()
        {
            string path = testPath + "/TestFile.txt";
            WriteFile(path, "Test file contents", true);

            _provider.DeleteItem(_activityId, path);
            MergeActivityResponse response = Commit();

            Assert.IsFalse(_provider.ItemExists(path));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(1, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitDeleteFileAlsoDeletesPropertiesOnFile()
        {
            string mimeType = "application/octet-stream";
            string path = testPath + "/TestFile.txt";
            WriteFile(path, "Fun text", false);
            SetProperty(path, "mime-type", mimeType, true);

            _provider.DeleteItem(_activityId, testPath + "/TestFile.txt");
            MergeActivityResponse response = Commit();

            ItemMetaData item = _provider.GetItems(-1, testPath, Recursion.Full);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(1, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitDeleteFolder()
        {
            string path = testPath + "/TestFolder";
            CreateFolder(path, true);

            _provider.DeleteItem(_activityId, path);
            MergeActivityResponse response = Commit();

            Assert.IsFalse(_provider.ItemExists(path));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(1, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitMoveAnUpdatedFileThenDeleteFolderThatContainedFile()
        {
            CreateFolder(testPath + "/A", false);
            CreateFolder(testPath + "/B", false);
            WriteFile(testPath + "/A/Test1.txt", "filedata", true);

            _provider.DeleteItem(_activityId, testPath + "/A");
            _provider.CopyItem(_activityId, testPath + "/A/Test1.txt", testPath + "/B/Test1.txt");
            _provider.WriteFile(_activityId, testPath + "/B/Test1.txt", GetBytes("filedata2"));
            MergeActivityResponse response = Commit();

            LogItem log2 =
                _provider.GetLog(testPath + "/B/Test1.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename | ChangeType.Edit, log2.History[0].Changes[0].ChangeType);
            Assert.AreEqual("filedata2", ReadFile(testPath + "/B/Test1.txt"));
            Assert.IsFalse(_provider.ItemExists(testPath + "/A"));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(3, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/B/Test1.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, testPath + "/B", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitMoveAnUpdateFile()
        {
            CreateFolder(testPath + "/Nodes", false);
            WriteFile(testPath + "/Nodes/Fun.txt", "filedata", false);
            CreateFolder(testPath + "/Protocol", true);
            byte[] fileData = GetBytes("filedata2");

            _provider.DeleteItem(_activityId, testPath + "/Nodes/Fun.txt");
            _provider.CopyItem(_activityId, testPath + "/Nodes/Fun.txt", testPath + "/Protocol/Fun.txt");
            bool created = _provider.WriteFile(_activityId, testPath + "/Protocol/Fun.txt", fileData);
            MergeActivityResponse response = Commit();

            LogItem log =
                _provider.GetLog(testPath + "/Protocol/Fun.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename | ChangeType.Edit, log.History[0].Changes[0].ChangeType);
            Assert.AreEqual(GetString(fileData), ReadFile(testPath + "/Protocol/Fun.txt"));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(3, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/Protocol/Fun.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, testPath + "/Protocol", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath + "/Nodes", ItemType.Folder));
        }

        [Test]
        public void TestCommitMoveFileFromDeletedFolder()
        {
            CreateFolder(testPath + "/A", false);
            WriteFile(testPath + "/A/Test.txt", "filedata", true);

            _provider.DeleteItem(_activityId, testPath + "/A");
            _provider.CopyItem(_activityId, testPath + "/A/Test.txt", testPath + "/Test.txt");
            MergeActivityResponse response = Commit();

            LogItem log = _provider.GetLog(testPath + "/Test.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
            Assert.IsFalse(_provider.ItemExists(testPath + "/A"));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/Test.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitMoveFileOutOfFolderAndDeleteFolder()
        {
            CreateFolder(testPath + "/TestFolder", false);
            bool created = WriteFile(testPath + "/TestFolder/TestFile.txt", "Test file contents", true);

            _provider.CopyItem(_activityId, testPath + "/TestFolder/TestFile.txt", testPath + "/FunFile.txt");
            _provider.DeleteItem(_activityId, testPath + "/TestFolder");
            MergeActivityResponse response = Commit();

            LogItem log =
                _provider.GetLog(testPath + "/FunFile.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
            Assert.IsNull(_provider.GetItems(-1, testPath + "/TestFolder", Recursion.None));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/FunFile.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitMoveFolderWithUpdatedFile()
        {
            CreateFolder(testPath + "/A", false);
            WriteFile(testPath + "/A/Test.txt", "filedata", false);
            CreateFolder(testPath + "/B", true);
            byte[] fileData = GetBytes("filedata2");

            _provider.DeleteItem(_activityId, testPath + "/A");
            _provider.CopyItem(_activityId, testPath + "/A", testPath + "/B/A");
            bool created = _provider.WriteFile(_activityId, testPath + "/B/A/Test.txt", fileData);
            MergeActivityResponse response = Commit();

            LogItem log =
                _provider.GetLog(testPath + "/B/A/Test.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename | ChangeType.Edit, log.History[0].Changes[0].ChangeType);
            Assert.AreEqual(GetString(fileData), ReadFile(testPath + "/B/A/Test.txt"));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(4, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/B/A/Test.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath + "/B/A", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath + "/B", ItemType.Folder));
        }

        [Test]
        public void TestCommitMoveMultipleFilesFromDeletedFolder()
        {
            CreateFolder(testPath + "/A", false);
            WriteFile(testPath + "/A/Test1.txt", "filedata", false);
            WriteFile(testPath + "/A/Test2.txt", "filedata", true);

            _provider.DeleteItem(_activityId, testPath + "/A");
            _provider.CopyItem(_activityId, testPath + "/A/Test1.txt", testPath + "/Test1.txt");
            _provider.CopyItem(_activityId, testPath + "/A/Test2.txt", testPath + "/Test2.txt");
            MergeActivityResponse response = Commit();

            LogItem log = _provider.GetLog(testPath + "/Test1.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
            log = _provider.GetLog(testPath + "/Test2.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
            Assert.IsFalse(_provider.ItemExists(testPath + "/A"));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(3, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/Test1.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath + "/Test2.txt", ItemType.File));
        }

        [Test]
        public void TestCommitMoveMultipleFilesFromDeletedFolderIntoNewFolder()
        {
            CreateFolder(testPath + "/A", false);
            WriteFile(testPath + "/A/Test1.txt", "filedata", false);
            WriteFile(testPath + "/A/Test2.txt", "filedata", true);

            _provider.DeleteItem(_activityId, testPath + "/A");
            _provider.MakeCollection(_activityId, testPath + "/B");
            _provider.CopyItem(_activityId, testPath + "/A/Test1.txt", testPath + "/B/Test1.txt");
            _provider.CopyItem(_activityId, testPath + "/A/Test2.txt", testPath + "/B/Test2.txt");
            MergeActivityResponse response = Commit();

            LogItem log1 =
                _provider.GetLog(testPath + "/B/Test1.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log1.History[0].Changes[0].ChangeType);
            LogItem log2 =
                _provider.GetLog(testPath + "/B/Test2.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log2.History[0].Changes[0].ChangeType);
            Assert.IsFalse(_provider.ItemExists(testPath + "/A"));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(4, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/B/Test1.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, testPath + "/B", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath + "/B/Test2.txt", ItemType.File));
        }

        [Test]
        public void TestCommitMultipleNewFiles()
        {
            byte[] testFile = GetBytes("Test file contents");

            _provider.WriteFile(_activityId, testPath + "/TestFile1.txt", testFile);
            _provider.WriteFile(_activityId, testPath + "/TestFile2.txt", testFile);
            _provider.WriteFile(_activityId, testPath + "/TestFile3.txt", testFile);
            MergeActivityResponse response = Commit();

            Assert.AreEqual(GetString(testFile), ReadFile(testPath + "/TestFile1.txt"));
            Assert.AreEqual(GetString(testFile), ReadFile(testPath + "/TestFile2.txt"));
            Assert.AreEqual(GetString(testFile), ReadFile(testPath + "/TestFile3.txt"));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(4, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/TestFile1.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath + "/TestFile2.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, testPath + "/TestFile3.txt", ItemType.File));
        }

        [Test]
        public void TestCommitMultipleNewPropertiesOnMultipleFiles()
        {
            WriteFile(testPath + "/TestFile1.txt", "Fun text", false);
            WriteFile(testPath + "/TestFile2.txt", "Fun text", true);

            _provider.SetProperty(_activityId, testPath + "/TestFile1.txt", "mime-type1", "mime1");
            _provider.SetProperty(_activityId, testPath + "/TestFile1.txt", "mime-type2", "mime2");
            _provider.SetProperty(_activityId, testPath + "/TestFile2.txt", "mime-type3", "mime3");
            _provider.SetProperty(_activityId, testPath + "/TestFile2.txt", "mime-type4", "mime4");
            MergeActivityResponse response = Commit();

            FolderMetaData item = (FolderMetaData) _provider.GetItems(-1, testPath, Recursion.Full);
            Assert.AreEqual("mime1", item.Items[0].Properties["mime-type1"]);
            Assert.AreEqual("mime2", item.Items[0].Properties["mime-type2"]);
            Assert.AreEqual("mime3", item.Items[1].Properties["mime-type3"]);
            Assert.AreEqual("mime4", item.Items[1].Properties["mime-type4"]);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/TestFile1.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, testPath + "/TestFile2.txt", ItemType.File));
        }

        [Test]
        public void TestCommitMultipleNewPropertiesOnMultipleFolders()
        {
            CreateFolder(testPath + "/Folder1", false);
            CreateFolder(testPath + "/Folder2", true);

            _provider.SetProperty(_activityId, testPath + "/Folder1", "mime-type1", "mime1");
            _provider.SetProperty(_activityId, testPath + "/Folder1", "mime-type2", "mime2");
            _provider.SetProperty(_activityId, testPath + "/Folder2", "mime-type3", "mime3");
            _provider.SetProperty(_activityId, testPath + "/Folder2", "mime-type4", "mime4");
            MergeActivityResponse response = Commit();

            FolderMetaData item = (FolderMetaData) _provider.GetItems(-1, testPath, Recursion.Full);
            Assert.AreEqual("mime1", item.Items[0].Properties["mime-type1"]);
            Assert.AreEqual("mime2", item.Items[0].Properties["mime-type2"]);
            Assert.AreEqual("mime3", item.Items[1].Properties["mime-type3"]);
            Assert.AreEqual("mime4", item.Items[1].Properties["mime-type4"]);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/Folder1", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath + "/Folder2", ItemType.Folder));
        }

        [Test]
        public void TestCommitNewFile()
        {
            byte[] testFile = GetBytes("Test file contents");

            bool created = _provider.WriteFile(_activityId, testPath + "/TestFile.txt", testFile);
            MergeActivityResponse response = Commit();

            Assert.AreEqual(GetString(testFile), ReadFile(testPath + "/TestFile.txt"));
            Assert.AreEqual(true, created);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/TestFile.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitNewFolder()
        {
            _provider.MakeCollection(_activityId, testPath + "/TestFolder");
            MergeActivityResponse response = Commit();

            Assert.IsTrue(_provider.ItemExists(testPath + "/TestFolder"));
            Assert.AreEqual(ItemType.Folder, _provider.GetItems(-1, testPath + "/TestFolder", Recursion.None).ItemType);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/TestFolder", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitNewFolderContainingNewFile()
        {
            byte[] fileData = GetBytes("Test file contents");

            _provider.MakeCollection(_activityId, testPath + "/TestFolder");
            _provider.WriteFile(_activityId, testPath + "/TestFolder/TestFile.txt", fileData);
            MergeActivityResponse response = Commit();

            Assert.AreEqual(GetString(fileData), ReadFile(testPath + "/TestFolder/TestFile.txt"));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(3, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/TestFolder/TestFile.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, testPath + "/TestFolder", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitNewPropertyOnFile()
        {
            string mimeType = "application/octet-stream";
            string path = testPath + "/TestFile.txt";
            WriteFile(path, "Fun text", true);

            _provider.SetProperty(_activityId, path, "mime-type", mimeType);
            MergeActivityResponse response = Commit();

            FolderMetaData item = (FolderMetaData) _provider.GetItems(-1, testPath, Recursion.Full);
            Assert.AreEqual(mimeType, item.Items[0].Properties["mime-type"]);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(1, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/TestFile.txt", ItemType.File));
        }

        [Test]
        public void TestCommitNewPropertyOnFolder()
        {
            string ignore = "*.bad\n";

            _provider.SetProperty(_activityId, testPath, "ignore", ignore);
            MergeActivityResponse response = Commit();

            FolderMetaData item = (FolderMetaData) _provider.GetItems(-1, testPath, Recursion.Full);
            Assert.AreEqual(ignore, item.Properties["ignore"]);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(1, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitNewPropertyOnNewFileInSameCommit()
        {
            byte[] fileData = GetBytes("test");

            _provider.WriteFile(_activityId, testPath + "/TestFile1.txt", fileData);
            _provider.SetProperty(_activityId, testPath + "/TestFile1.txt", "mime-type1", "mime1");
            MergeActivityResponse response = Commit();

            FolderMetaData item = (FolderMetaData) _provider.GetItems(-1, testPath, Recursion.Full);
            Assert.AreEqual("mime1", item.Items[0].Properties["mime-type1"]);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/TestFile1.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitNewPropertyOnNewFolderInSameCommit()
        {
            _provider.MakeCollection(_activityId, testPath + "/Folder1");
            _provider.SetProperty(_activityId, testPath + "/Folder1", "mime-type1", "mime1");
            MergeActivityResponse response = Commit();

            FolderMetaData item = (FolderMetaData) _provider.GetItems(-1, testPath, Recursion.Full);
            Assert.AreEqual("mime1", item.Items[0].Properties["mime-type1"]);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/Folder1", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitNewSubFolderInNewFolder()
        {
            _provider.MakeCollection(_activityId, testPath + "/TestFolder");
            _provider.MakeCollection(_activityId, testPath + "/TestFolder/SubFolder");
            MergeActivityResponse response = Commit();

            Assert.IsTrue(_provider.ItemExists(testPath + "/TestFolder/SubFolder"));
            Assert.AreEqual(ItemType.Folder,
                            _provider.GetItems(-1, testPath + "/TestFolder/SubFolder", Recursion.None).ItemType);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(3, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/TestFolder", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath + "/TestFolder/SubFolder", ItemType.Folder));
        }

        [Test]
        public void TestCommitRenameAndUpdateFile()
        {
            WriteFile(testPath + "/Fun.txt", "Fun text", true);
            byte[] updatedText = GetBytes("Test file contents");

            _provider.DeleteItem(_activityId, testPath + "/Fun.txt");
            _provider.CopyItem(_activityId, testPath + "/Fun.txt", testPath + "/FunRename.txt");
            bool created = _provider.WriteFile(_activityId, testPath + "/FunRename.txt", updatedText);
            MergeActivityResponse response = Commit();

            LogItem log =
                _provider.GetLog(testPath + "/FunRename.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename | ChangeType.Edit, log.History[0].Changes[0].ChangeType);
            Assert.AreEqual(GetString(updatedText), ReadFile(testPath + "/FunRename.txt"));
            Assert.AreEqual(false, created);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath + "/FunRename.txt", ItemType.File));
        }

        [Test]
        public void TestCommitRenameFile()
        {
            WriteFile(testPath + "/Fun.txt", "Fun text", true);

            _provider.DeleteItem(_activityId, testPath + "/Fun.txt");
            _provider.CopyItem(_activityId, testPath + "/Fun.txt", testPath + "/FunRename.txt");
            MergeActivityResponse response = Commit();

            LogItem log =
                _provider.GetLog(testPath + "/FunRename.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath + "/FunRename.txt", ItemType.File));
        }

        [Test]
        public void TestCommitRenameFileWithCopyBeforeDelete()
        {
            WriteFile(testPath + "/FunRename.txt", "Fun text", true);

            _provider.CopyItem(_activityId, testPath + "/FunRename.txt", testPath + "/Fun.txt");
            _provider.DeleteItem(_activityId, testPath + "/FunRename.txt");
            MergeActivityResponse response = Commit();

            LogItem log = _provider.GetLog(testPath + "/Fun.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/Fun.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitRenameFolder()
        {
            CreateFolder(testPath + "/Fun", true);

            _provider.DeleteItem(_activityId, testPath + "/Fun");
            _provider.CopyItem(_activityId, testPath + "/Fun", testPath + "/FunRename");
            MergeActivityResponse response = Commit();

            LogItem log = _provider.GetLog(testPath + "/FunRename", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath + "/FunRename", ItemType.Folder));
        }

        [Test]
        public void TestCommitRenameFolderAndDeleteFileWithinFolder()
        {
            CreateFolder(testPath + "/A", false);
            WriteFile(testPath + "/A/Test1.txt", "filedata", true);

            _provider.DeleteItem(_activityId, testPath + "/A");
            _provider.CopyItem(_activityId, testPath + "/A", testPath + "/B");
            _provider.DeleteItem(_activityId, testPath + "/B/Test1.txt");
            MergeActivityResponse response = Commit();

            LogItem log1 = _provider.GetLog(testPath + "/B", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log1.History[0].Changes[0].ChangeType);
            Assert.IsFalse(_provider.ItemExists(testPath + "/B/Test1.txt"));
            Assert.IsFalse(_provider.ItemExists(testPath + "/A"));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/B", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitRenameFolderContainingRenamedFile()
        {
            CreateFolder(testPath + "/A", false);
            WriteFile(testPath + "/A/Test1.txt", "filedata", true);

            _provider.DeleteItem(_activityId, testPath + "/A");
            _provider.CopyItem(_activityId, testPath + "/A", testPath + "/B");
            _provider.DeleteItem(_activityId, testPath + "/B/Test1.txt");
            _provider.CopyItem(_activityId, testPath + "/A/Test1.txt", testPath + "/B/Test2.txt");
            MergeActivityResponse response = Commit();

            LogItem log1 = _provider.GetLog(testPath + "/B", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log1.History[0].Changes[0].ChangeType);
            LogItem log2 =
                _provider.GetLog(testPath + "/B/Test2.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log2.History[0].Changes[0].ChangeType);
            Assert.IsFalse(_provider.ItemExists(testPath + "/A"));
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(3, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/B", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath + "/B/Test2.txt", ItemType.File));
        }

        [Test]
        public void TestCommitRenameFolderContainingRenamedFileAndNotRenamedFile()
        {
            CreateFolder(testPath + "/A", false);
            WriteFile(testPath + "/A/TestA1.txt", "filedata", false);
            WriteFile(testPath + "/A/TestB1.txt", "filedata", true);

            _provider.DeleteItem(_activityId, testPath + "/A");
            _provider.CopyItem(_activityId, testPath + "/A", testPath + "/B");
            _provider.DeleteItem(_activityId, testPath + "/B/TestA1.txt");
            _provider.CopyItem(_activityId, testPath + "/A/TestA1.txt", testPath + "/B/TestA2.txt");
            MergeActivityResponse response = Commit();

            // Assert state of TFS database
            Assert.IsFalse(_provider.ItemExists(testPath + "/A"));
            Assert.IsTrue(_provider.ItemExists(testPath + "/B"));
            Assert.IsTrue(_provider.ItemExists(testPath + "/B/TestA2.txt"));
            Assert.IsTrue(_provider.ItemExists(testPath + "/B/TestB1.txt"));
            // Assert TFS history
            LogItem log1 = _provider.GetLog(testPath + "/B", 1, _provider.GetLatestVersion(), Recursion.Full, 1);
            Assert.AreEqual(ChangeType.Rename, log1.History[0].Changes[0].ChangeType);
            Assert.AreEqual(ChangeType.Rename, log1.History[0].Changes[1].ChangeType);
            Assert.AreEqual(ChangeType.Rename, log1.History[0].Changes[2].ChangeType);
            // Assert commit output
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(3, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath + "/B", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath + "/B/TestA2.txt", ItemType.File));
        }

        [Test]
        public void TestCommitRenameFolderContainingUpdatedFile()
        {
            CreateFolder(testPath + "/A", false);
            WriteFile(testPath + "/A/Test.txt", "filedata", true);

            _provider.DeleteItem(_activityId, testPath + "/A");
            _provider.CopyItem(_activityId, testPath + "/A", testPath + "/B");
            bool created = _provider.WriteFile(_activityId, testPath + "/B/Test.txt", GetBytes("filedata2"));
            MergeActivityResponse response = Commit();

            // Assert state of TFS database
            Assert.IsFalse(_provider.ItemExists(testPath + "/A"));
            Assert.IsTrue(_provider.ItemExists(testPath + "/B"));
            Assert.IsTrue(_provider.ItemExists(testPath + "/B/Test.txt"));
            Assert.AreEqual("filedata2", ReadFile(testPath + "/B/Test.txt"));
            // Assert TFS history
            LogItem log1 = _provider.GetLog(testPath + "/B", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log1.History[0].Changes[0].ChangeType);
            LogItem log2 =
                _provider.GetLog(testPath + "/B/Test.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Edit | ChangeType.Rename, log2.History[0].Changes[0].ChangeType);
            // Assert commit output
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(3, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/B", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath + "/B/Test.txt", ItemType.File));
        }

        [Test]
        public void TestCommitRenameFolderWithPropertiesAddedInCommitAfterFolderCreated()
        {
            CreateFolder(testPath + "/A", true);
            SetProperty(testPath + "/A", "prop1", "val1", true);

            _provider.DeleteItem(_activityId, testPath + "/A");
            _provider.CopyItem(_activityId, testPath + "/A", testPath + "/B");
            MergeActivityResponse response = Commit();

            // Assert state of TFS database
            Assert.IsFalse(_provider.ItemExists(testPath + "/A"));
            Assert.IsTrue(_provider.ItemExists(testPath + "/B"));
            Assert.AreEqual("val1", _provider.GetItems(-1, testPath + "/B", Recursion.None).Properties["prop1"]);
            // Assert TFS history
            LogItem log1 = _provider.GetLog(testPath + "/B", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log1.History[0].Changes[0].ChangeType);
            // Assert commit output
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/B", ItemType.Folder));
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitReplacedFile()
        {
            WriteFile(testPath + "/TestFile.txt", "Test file contents", true);

            _provider.DeleteItem(_activityId, testPath + "/TestFile.txt");
            _provider.WriteFile(_activityId, testPath + "/TestFile.txt", GetBytes("new"));
            MergeActivityResponse response = Commit();

            // Assert state of TFS database
            Assert.IsTrue(_provider.ItemExists(testPath + "/TestFile.txt"));
            Assert.AreEqual("new", ReadFile(testPath + "/TestFile.txt"));
            // Assert TFS history
            LogItem log1 =
                _provider.GetLog(testPath + "/TestFile.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Edit, log1.History[0].Changes[0].ChangeType);
            // Assert commit output
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(2, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/TestFile.txt", ItemType.File));
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
        }

        [Test]
        public void TestCommitUpdatedFile()
        {
            WriteFile(testPath + "/TestFile.txt", "Test file contents", true);
            byte[] testFile = GetBytes("Test file contents\r\nUpdated");

            bool created = _provider.WriteFile(_activityId, testPath + "/TestFile.txt", testFile);
            MergeActivityResponse response = Commit();

            Assert.AreEqual(GetString(testFile), ReadFile(testPath + "/TestFile.txt"));
            Assert.AreEqual(false, created);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(1, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/TestFile.txt", ItemType.File));
        }

        [Test, Ignore("Not implemented yet")]
        [ExpectedException(typeof (ConflictException))]
        public void TestCommitUpdatedFileAtSameTimeAsAnotherUserThrowsConflictException()
        {
            string activity2 = Guid.NewGuid().ToString();
            WriteFile(testPath + "/Test.txt", "data", true);

            //_provider.Checkout(_activityId);
            _provider.MakeActivity(activity2);
            //_provider.Checkout(activity2);
            _provider.WriteFile(activity2, testPath + "/Test.txt", GetBytes("new1"));
            _provider.MergeActivity(activity2);
            _provider.DeleteActivity(activity2);
            _provider.WriteFile(_activityId, testPath + "/Test.txt", GetBytes("new2"));
            _provider.MergeActivity(_activityId);
        }

        [Test]
        public void TestCommitUpdatePropertyOnFile()
        {
            string mimeType1 = "application/octet-stream1";
            string mimeType2 = "application/octet-stream2";
            string path = testPath + "/TestFile.txt";
            WriteFile(path, "Fun text", false);
            SetProperty(path, "mime-type", mimeType1, true);

            _provider.SetProperty(_activityId, path, "mime-type", mimeType2);
            MergeActivityResponse response = Commit();

            FolderMetaData item = (FolderMetaData) _provider.GetItems(-1, testPath, Recursion.Full);
            Assert.AreEqual(mimeType2, item.Items[0].Properties["mime-type"]);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(1, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath + "/TestFile.txt", ItemType.File));
        }

        [Test]
        public void TestCommitUpdatePropertyOnFolder()
        {
            string ignore1 = "*.bad\n";
            string ignore2 = "*.good\n";
            SetProperty(testPath, "ignore", ignore1, true);

            _provider.SetProperty(_activityId, testPath, "ignore", ignore2);
            MergeActivityResponse response = Commit();

            FolderMetaData item = (FolderMetaData) _provider.GetItems(-1, testPath, Recursion.Full);
            Assert.AreEqual(ignore2, item.Properties["ignore"]);
            Assert.AreEqual(_provider.GetLatestVersion(), response.Version);
            Assert.AreEqual(1, response.Items.Count);
            Assert.IsTrue(ResponseContains(response, testPath, ItemType.Folder));
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
    }
}