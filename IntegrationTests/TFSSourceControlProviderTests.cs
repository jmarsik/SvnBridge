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
    public class TFSSourceControlProviderTests : TFSSourceControlProviderTestsBase
    {
        [Test]
        public void TestCommitFolder()
        {
            CreateFolder(_testPath + "/TestFolder", true);

            Assert.IsTrue(_provider.ItemExists(_testPath + "/TestFolder"));
            Assert.AreEqual(ItemType.Folder, _provider.GetItems(-1, _testPath + "/TestFolder", Recursion.None).ItemType);
        }

        [Test]
        public void TestCommitFolderAndSubFolder()
        {
            CreateFolder(_testPath + "/TestFolder", false);
            CreateFolder(_testPath + "/TestFolder/SubFolder", true);

            Assert.IsTrue(_provider.ItemExists(_testPath + "/TestFolder/SubFolder"));
            Assert.AreEqual(ItemType.Folder, _provider.GetItems(-1, _testPath + "/TestFolder/SubFolder", Recursion.None).ItemType);
        }

        [Test]
        public void TestCommitNewFile()
        {
            byte[] testFile = Encoding.Default.GetBytes("Test file contents");

            bool created = WriteFile(_testPath + "/TestFile.txt", testFile, true);

            byte[] actual = ReadFile(_testPath + "/TestFile.txt");
            Assert.AreEqual(Encoding.Default.GetString(testFile), Encoding.Default.GetString(actual));
            Assert.AreEqual(true, created);
        }

        [Test]
        public void TestCommitMultipleNewFiles()
        {
            byte[] testFile = Encoding.Default.GetBytes("Test file contents");

            WriteFile(_testPath + "/TestFile1.txt", testFile, false);
            WriteFile(_testPath + "/TestFile2.txt", testFile, false);
            WriteFile(_testPath + "/TestFile3.txt", testFile, true);

            byte[] actual = ReadFile(_testPath + "/TestFile1.txt");
            Assert.AreEqual(Encoding.Default.GetString(testFile), Encoding.Default.GetString(actual));
            actual = ReadFile(_testPath + "/TestFile2.txt");
            Assert.AreEqual(Encoding.Default.GetString(testFile), Encoding.Default.GetString(actual));
            actual = ReadFile(_testPath + "/TestFile3.txt");
            Assert.AreEqual(Encoding.Default.GetString(testFile), Encoding.Default.GetString(actual));
        }

        [Test]
        public void TestCommitNewFileInNewSubFolder()
        {
            byte[] testFile = Encoding.Default.GetBytes("Test file contents");

            CreateFolder(_testPath + "/TestFolder", false);
            WriteFile(_testPath + "/TestFolder/TestFile.txt", testFile, true);

            byte[] actual = ReadFile(_testPath + "/TestFolder/TestFile.txt");
            Assert.AreEqual(Encoding.Default.GetString(testFile), Encoding.Default.GetString(actual));
        }

        [Test]
        public void TestCommitUpdatedFile()
        {
            WriteFile(_testPath + "/TestFile.txt", "Test file contents", true);
            string testUpdatedFile = "Test file contents\r\nUpdated";

            bool created = WriteFile(_testPath + "/TestFile.txt", testUpdatedFile, true);

            byte[] actual = ReadFile(_testPath + "/TestFile.txt");
            Assert.AreEqual(testUpdatedFile, Encoding.Default.GetString(actual));
            Assert.AreEqual(false, created);
        }

        [Test]
        public void TestGetItemsReturnsIgnoreInfo()
        {
            string ignore = "*.bad\n";
            SetProperty(_testPath, "ignore", ignore, true);

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.OneLevel);

            Assert.AreEqual(ignore, item.Properties["ignore"]);
        }

        [Test]
        public void TestSetIgnoreListOnFolder()
        {
            string ignore = "*.bad\n";

            SetProperty(_testPath, "ignore", ignore, true);

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.OneLevel);
            Assert.AreEqual(ignore, item.Properties["ignore"]);
        }

        [Test]
        public void TestUpdateIgnoreListOnFolder()
        {
            string ignore1 = "*.bad\n";
            string ignore2 = "*.good\n";
            SetProperty(_testPath, "ignore", ignore1, true);

            SetProperty(_testPath, "ignore", ignore2, true);

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.OneLevel);
            Assert.AreEqual(ignore2, item.Properties["ignore"]);
        }

        [Test]
        public void TestDeleteFile()
        {
            string path = _testPath + "/TestFile.txt";
            WriteFile(path, "Test file contents", true);

            DeleteItem(path, true);

            Assert.IsFalse(_provider.ItemExists(path));
        }

        [Test]
        public void TestDeleteFolder()
        {
            string path = _testPath + "/TestFolder";
            CreateFolder(path, true);

            DeleteItem(path, true);

            Assert.IsFalse(_provider.ItemExists(path));
        }

        [Test]
        public void TestGetLog()
        {
            int versionFrom = _provider.GetLatestVersion();
            WriteFile(_testPath + "/TestFile.txt", "Fun text", true);
            int versionTo = _provider.GetLatestVersion();

            LogItem logItem = _provider.GetLog(_testPath, versionFrom, versionTo, Recursion.Full, Int32.MaxValue);

            Assert.AreEqual(2, logItem.History.Length);
        }

        [Test]
        public void TestGetItemsReturnsMimeTypeInfo()
        {
            string mimeType = "application/octet-stream";
            string path = _testPath + "/TestFile.txt";
            WriteFile(path, "Fun text", false);
            SetProperty(path, "mime-type", mimeType, true);

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.OneLevel);

            Assert.AreEqual(mimeType, item.Items[0].Properties["mime-type"]);
        }

        [Test]
        public void TestSetMimeTypeOnFile()
        {
            string mimeType = "application/octet-stream";
            string path = _testPath + "/TestFile.txt";
            WriteFile(path, "Fun text", true);

            SetProperty(path, "mime-type", mimeType, true);

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.OneLevel);
            Assert.AreEqual(mimeType, item.Items[0].Properties["mime-type"]);
        }

        [Test]
        public void TestUpdateMimeTypeOnFile()
        {
            string mimeType1 = "application/octet-stream1";
            string mimeType2 = "application/octet-stream2";
            string path = _testPath + "/TestFile.txt";
            WriteFile(path, "Fun text", false);
            SetProperty(path, "mime-type", mimeType1, true);

            SetProperty(path, "mime-type", mimeType2, true);

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.OneLevel);
            Assert.AreEqual(mimeType2, item.Items[0].Properties["mime-type"]);
        }

        [Test]
        public void CommitWithNoItemsReturnsLatestChangeset()
        {
            int startVersion = _provider.GetLatestVersion();

            MergeActivityResponse response = _provider.MergeActivity(_activityId);

            int endVersion = _provider.GetLatestVersion();
            Assert.AreEqual(startVersion, response.Version);
            Assert.AreEqual(startVersion, endVersion);
        }

        [Test]
        public void TestGetItemsForRootSucceeds()
        {
            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, "", Recursion.OneLevel);
        }

        [Test, ExpectedException(typeof(FolderAlreadyExistsException))]
        public void TestAddFolderThatAlreadyExistsThrowsException()
        {
            CreateFolder(_testPath + "/New Folder", true);

            _provider.MakeCollection(_activityId, _testPath + "/New Folder");
        }

        [Test]
        public void TestRenameFile()
        {
            WriteFile(_testPath + "/Fun.txt", "Fun text", true);

            _provider.DeleteItem(_activityId, _testPath + "/Fun.txt");
            _provider.CopyItem(_activityId, _testPath + "/Fun.txt", _testPath + "/FunRename.txt");
            Commit();

            LogItem log = _provider.GetLog(_testPath + "/FunRename.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
        }

        [Test]
        public void TestRenameFileWithCopyBeforeDelete()
        {
            WriteFile(_testPath + "/Fun.txt", "Fun text", true);

            _provider.CopyItem(_activityId, _testPath + "/Fun.txt", _testPath + "/FunRename.txt");
            _provider.DeleteItem(_activityId, _testPath + "/Fun.txt");
            Commit();

            LogItem log = _provider.GetLog(_testPath + "/FunRename.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
        }

        [Test]
        public void TestRenameFolder()
        {
            CreateFolder(_testPath + "/Fun", true);

            _provider.DeleteItem(_activityId, _testPath + "/Fun");
            _provider.CopyItem(_activityId, _testPath + "/Fun", _testPath + "/FunRename");
            Commit();

            LogItem log = _provider.GetLog(_testPath + "/FunRename", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
        }

        [Test]
        public void TestBranchFile()
        {
            WriteFile(_testPath + "/Fun.txt", "Fun text", true);

            _provider.CopyItem(_activityId, _testPath + "/Fun.txt", _testPath + "/FunBranch.txt");
            Commit();

            LogItem log = _provider.GetLog(_testPath + "/FunBranch.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Branch, log.History[0].Changes[0].ChangeType & ChangeType.Branch);
        }

        [Test]
        public void TestBranchFolder()
        {
            CreateFolder(_testPath + "/Fun", true);

            _provider.CopyItem(_activityId, _testPath + "/Fun", _testPath + "/FunBranch");
            Commit();

            LogItem log = _provider.GetLog(_testPath + "/FunBranch", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Branch, log.History[0].Changes[0].ChangeType & ChangeType.Branch);
        }

        [Test]
        public void TestRenameAndEditFile()
        {
            WriteFile(_testPath + "/Fun.txt", "Fun text", true);

            _provider.CopyItem(_activityId, _testPath + "/Fun.txt", _testPath + "/FunRename.txt");
            _provider.DeleteItem(_activityId, _testPath + "/Fun.txt");
            byte[] updatedText = Encoding.Default.GetBytes("Test file contents");
            bool created = _provider.WriteFile(_activityId, _testPath + "/FunRename.txt", updatedText);
            Commit();

            LogItem log = _provider.GetLog(_testPath + "/FunRename.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename | ChangeType.Edit, log.History[0].Changes[0].ChangeType);
            byte[] actualUpdatedText = ReadFile(_testPath + "/FunRename.txt");
            Assert.AreEqual(Encoding.Default.GetString(updatedText), Encoding.Default.GetString(actualUpdatedText));
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
        public void TestDeleteFileAlsoDeletesPropertiesOnFile()
        {
            string mimeType = "application/octet-stream";
            string path = _testPath + "/TestFile.txt";
            WriteFile(path, "Fun text", false);
            SetProperty(path, "mime-type", mimeType, true);

            DeleteItem(_testPath + "/TestFile.txt", true);

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.OneLevel);
        }

        [Test]
        public void TestGetLogReturnsOriginalNameAndRevisionForRenamedItems()
        {
            WriteFile(_testPath + "/Fun.txt", "Fun text", true);
            int versionFrom = _provider.GetLatestVersion();
            MoveItem(_testPath + "/Fun.txt", _testPath + "/FunRename.txt", true);
            int versionTo = _provider.GetLatestVersion();

            LogItem logItem = _provider.GetLog(_testPath + "/FunRename.txt", versionFrom, versionTo, Recursion.None, 1);

            Assert.AreEqual(_testPath + "/Fun.txt", ((RenamedSourceItem)logItem.History[0].Changes[0].Item).OriginalRemoteName.Substring(1));
            Assert.AreEqual(versionFrom, ((RenamedSourceItem)logItem.History[0].Changes[0].Item).OriginalRevision);
            Assert.AreEqual(_testPath + "/FunRename.txt", logItem.History[0].Changes[0].Item.RemoteName.Substring(1));
        }

        [Test]
        public void TestCommitOfMovedAndEditedFile()
        {
            CreateFolder(_testPath + "/Nodes", false);
            WriteFile(_testPath + "/Nodes/Fun.txt", "filedata", false);
            CreateFolder(_testPath + "/Protocol", true);

            WriteFile(_testPath + "/Nodes/Fun.txt", "filedata2", false);
            _provider.DeleteItem(_activityId, _testPath + "/Nodes/Fun.txt");
            _provider.CopyItem(_activityId, _testPath + "/Nodes/Fun.txt", _testPath + "/Protocol/Fun.txt");
            Commit();

            LogItem log = _provider.GetLog(_testPath + "/Protocol/Fun.txt", 1, _provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename | ChangeType.Edit, log.History[0].Changes[0].ChangeType);
        }
    }
}
