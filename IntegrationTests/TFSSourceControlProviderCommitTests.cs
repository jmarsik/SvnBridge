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
            Commit();

            byte[] actual = ReadFile(_testPath + "/TestFile.txt");
            Assert.AreEqual(GetString(testFile), GetString(actual));
            Assert.AreEqual(true, created);
        }

        [Test]
        public void TestCommitUpdatedFile()
        {
            WriteFile(_testPath + "/TestFile.txt", "Test file contents", true);
            byte[] testFile = GetBytes("Test file contents\r\nUpdated");

            bool created = _provider.WriteFile(_activityId, _testPath + "/TestFile.txt", testFile);
            Commit();

            byte[] actual = ReadFile(_testPath + "/TestFile.txt");
            Assert.AreEqual(GetString(testFile), GetString(actual));
            Assert.AreEqual(false, created);
        }

        [Test]
        public void TestCommitNewFolder()
        {
            _provider.MakeCollection(_activityId, _testPath + "/TestFolder");
            Commit();

            Assert.IsTrue(_provider.ItemExists(_testPath + "/TestFolder"));
            Assert.AreEqual(ItemType.Folder, _provider.GetItems(-1, _testPath + "/TestFolder", Recursion.None).ItemType);
        }

        [Test]
        public void TestCommitMultipleNewFiles()
        {
            byte[] testFile = GetBytes("Test file contents");

            _provider.WriteFile(_activityId, _testPath + "/TestFile1.txt", testFile);
            _provider.WriteFile(_activityId, _testPath + "/TestFile2.txt", testFile);
            _provider.WriteFile(_activityId, _testPath + "/TestFile3.txt", testFile);
            Commit();

            byte[] actual = ReadFile(_testPath + "/TestFile1.txt");
            Assert.AreEqual(GetString(testFile), GetString(actual));
            actual = ReadFile(_testPath + "/TestFile2.txt");
            Assert.AreEqual(GetString(testFile), GetString(actual));
            actual = ReadFile(_testPath + "/TestFile3.txt");
            Assert.AreEqual(GetString(testFile), GetString(actual));
        }

        [Test]
        public void TestCommitNewSubFolderInNewFolder()
        {
            _provider.MakeCollection(_activityId, _testPath + "/TestFolder");
            _provider.MakeCollection(_activityId, _testPath + "/TestFolder/SubFolder");
            Commit();

            Assert.IsTrue(_provider.ItemExists(_testPath + "/TestFolder/SubFolder"));
            Assert.AreEqual(ItemType.Folder, _provider.GetItems(-1, _testPath + "/TestFolder/SubFolder", Recursion.None).ItemType);
        }

        [Test]
        public void TestCommitNewFileInNewFolder()
        {
            byte[] testFile = GetBytes("Test file contents");

            _provider.MakeCollection(_activityId, _testPath + "/TestFolder");
            _provider.WriteFile(_activityId, _testPath + "/TestFolder/TestFile.txt", testFile);
            Commit();

            byte[] actual = ReadFile(_testPath + "/TestFolder/TestFile.txt");
            Assert.AreEqual(GetString(testFile), GetString(actual));
        }

        [Test]
        public void TestCommitDeleteFile()
        {
            string path = _testPath + "/TestFile.txt";
            WriteFile(path, "Test file contents", true);

            _provider.DeleteItem(_activityId, path);
            Commit();

            Assert.IsFalse(_provider.ItemExists(path));
        }

        [Test]
        public void TestCommitDeleteFolder()
        {
            string path = _testPath + "/TestFolder";
            CreateFolder(path, true);

            _provider.DeleteItem(_activityId, path);
            Commit();

            Assert.IsFalse(_provider.ItemExists(path));
        }

        [Test]
        public void TestCommitNewMimeTypePropertyOnFile()
        {
            string mimeType = "application/octet-stream";
            string path = _testPath + "/TestFile.txt";
            WriteFile(path, "Fun text", true);

            _provider.SetProperty(_activityId, path, "mime-type", mimeType);
            Commit();

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.OneLevel);
            Assert.AreEqual(mimeType, item.Items[0].Properties["mime-type"]);
        }

        [Test]
        public void TestCommitNewIgnoreListPropertyOnFolder()
        {
            string ignore = "*.bad\n";

            _provider.SetProperty(_activityId, _testPath, "ignore", ignore);
            Commit();

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.OneLevel);
            Assert.AreEqual(ignore, item.Properties["ignore"]);
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
            Commit();

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.OneLevel);
            Assert.AreEqual(mimeType2, item.Items[0].Properties["mime-type"]);
        }

        [Test]
        public void TestCommitUpdateIgnoreListPropertyOnFolder()
        {
            string ignore1 = "*.bad\n";
            string ignore2 = "*.good\n";
            SetProperty(_testPath, "ignore", ignore1, true);

            _provider.SetProperty(_activityId, _testPath, "ignore", ignore2);
            Commit();

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.OneLevel);
            Assert.AreEqual(ignore2, item.Properties["ignore"]);
        }

        [Test]
        public void TestCommitWithNoItemsReturnsLatestChangeset()
        {
            int startVersion = _provider.GetLatestVersion();

            MergeActivityResponse response = _provider.MergeActivity(_activityId);

            int endVersion = _provider.GetLatestVersion();
            Assert.AreEqual(startVersion, response.Version);
            Assert.AreEqual(startVersion, endVersion);
        }

        [Test]
        public void TestCommitRenameFile()
        {
            WriteFile(_testPath + "/Fun.txt", "Fun text", true);

            _provider.DeleteItem(_activityId, _testPath + "/Fun.txt");
            _provider.CopyItem(_activityId, _testPath + "/Fun.txt", _testPath + "/FunRename.txt");
            Commit();

            LogItem log = _provider.GetLog(_testPath + "/FunRename.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
        }

        [Test]
        public void TestCommitRenameFileWithCopyBeforeDelete()
        {
            WriteFile(_testPath + "/Fun.txt", "Fun text", true);

            _provider.CopyItem(_activityId, _testPath + "/Fun.txt", _testPath + "/FunRename.txt");
            _provider.DeleteItem(_activityId, _testPath + "/Fun.txt");
            Commit();

            LogItem log = _provider.GetLog(_testPath + "/FunRename.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
        }

        [Test]
        public void TestCommitRenameFolder()
        {
            CreateFolder(_testPath + "/Fun", true);

            _provider.DeleteItem(_activityId, _testPath + "/Fun");
            _provider.CopyItem(_activityId, _testPath + "/Fun", _testPath + "/FunRename");
            Commit();

            LogItem log = _provider.GetLog(_testPath + "/FunRename", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
        }

        [Test]
        public void TestCommitBranchFile()
        {
            WriteFile(_testPath + "/Fun.txt", "Fun text", true);

            _provider.CopyItem(_activityId, _testPath + "/Fun.txt", _testPath + "/FunBranch.txt");
            Commit();

            LogItem log = _provider.GetLog(_testPath + "/FunBranch.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Branch, log.History[0].Changes[0].ChangeType & ChangeType.Branch);
        }

        [Test]
        public void TestCommitBranchFolder()
        {
            CreateFolder(_testPath + "/Fun", true);

            _provider.CopyItem(_activityId, _testPath + "/Fun", _testPath + "/FunBranch");
            Commit();

            LogItem log = _provider.GetLog(_testPath + "/FunBranch", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Branch, log.History[0].Changes[0].ChangeType & ChangeType.Branch);
        }

        [Test]
        public void TestCommitRenameAndEditFile()
        {
            WriteFile(_testPath + "/Fun.txt", "Fun text", true);
            byte[] updatedText = GetBytes("Test file contents");

            _provider.CopyItem(_activityId, _testPath + "/Fun.txt", _testPath + "/FunRename.txt");
            _provider.DeleteItem(_activityId, _testPath + "/Fun.txt");
            bool created = _provider.WriteFile(_activityId, _testPath + "/FunRename.txt", updatedText);
            Commit();

            LogItem log = _provider.GetLog(_testPath + "/FunRename.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename | ChangeType.Edit, log.History[0].Changes[0].ChangeType);
            byte[] actualUpdatedText = ReadFile(_testPath + "/FunRename.txt");
            Assert.AreEqual(GetString(updatedText), GetString(actualUpdatedText));
            Assert.AreEqual(false, created);
        }

        [Test]
        public void TestCommitOfMoveFileOutOfFolderAndDeleteFolder()
        {
            CreateFolder(_testPath + "/TestFolder", false);
            bool created = WriteFile(_testPath + "/TestFolder/TestFile.txt", "Test file contents", true);

            _provider.CopyItem(_activityId, _testPath + "/TestFolder/TestFile.txt", _testPath + "/FunFile.txt");
            _provider.DeleteItem(_activityId, _testPath + "/TestFolder/TestFile.txt");
            _provider.DeleteItem(_activityId, _testPath + "/TestFolder");
            Commit();

            LogItem log = _provider.GetLog(_testPath + "/FunFile.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
            Assert.IsNull(_provider.GetItems(-1, _testPath + "/TestFolder", Recursion.None));
        }

        [Test]
        public void TestCommitDeleteFileAlsoDeletesPropertiesOnFile()
        {
            string mimeType = "application/octet-stream";
            string path = _testPath + "/TestFile.txt";
            WriteFile(path, "Fun text", false);
            SetProperty(path, "mime-type", mimeType, true);

            _provider.DeleteItem(_activityId, _testPath + "/TestFile.txt");
            Commit();

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.OneLevel);
        }

        [Test]
        public void TestCommitOfMovedAndEditedFile()
        {
            CreateFolder(_testPath + "/Nodes", false);
            WriteFile(_testPath + "/Nodes/Fun.txt", "filedata", false);
            CreateFolder(_testPath + "/Protocol", true);
            byte[] fileData = GetBytes("filedata2");

            bool created = _provider.WriteFile(_activityId, _testPath + "/Nodes/Fun.txt", fileData);
            _provider.DeleteItem(_activityId, _testPath + "/Nodes/Fun.txt");
            _provider.CopyItem(_activityId, _testPath + "/Nodes/Fun.txt", _testPath + "/Protocol/Fun.txt");
            Commit();

            LogItem log = _provider.GetLog(_testPath + "/Protocol/Fun.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename | ChangeType.Edit, log.History[0].Changes[0].ChangeType);
        }
    }
}
