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

            bool created = WriteFile(testPath + "/TestFile.txt", testFile, true);

            byte[] actual = ReadFile(testPath + "/TestFile.txt");
            Assert.AreEqual(Encoding.Default.GetString(testFile), Encoding.Default.GetString(actual));
            Assert.AreEqual(true, created);
        }

        [Test]
        public void TestCommitMultipleNewFiles()
        {
            byte[] testFile = Encoding.Default.GetBytes("Test file contents");

            WriteFile(testPath + "/TestFile1.txt", testFile, false);
            WriteFile(testPath + "/TestFile2.txt", testFile, false);
            WriteFile(testPath + "/TestFile3.txt", testFile, true);

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
            WriteFile(testPath + "/TestFolder/TestFile.txt", testFile, true);

            byte[] actual = ReadFile(testPath + "/TestFolder/TestFile.txt");
            Assert.AreEqual(Encoding.Default.GetString(testFile), Encoding.Default.GetString(actual));
        }

        [Test]
        public void TestCommitUpdatedFile()
        {
            WriteFile(testPath + "/TestFile.txt", "Test file contents", true);
            string testUpdatedFile = "Test file contents\r\nUpdated";

            bool created = WriteFile(testPath + "/TestFile.txt", testUpdatedFile, true);

            byte[] actual = ReadFile(testPath + "/TestFile.txt");
            Assert.AreEqual(testUpdatedFile, Encoding.Default.GetString(actual));
            Assert.AreEqual(false, created);
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
            WriteFile(path, "Test file contents", true);

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
            WriteFile(testPath + "/TestFile.txt", "Fun text", true);
            int versionTo = provider.GetLatestVersion();

            LogItem logItem = provider.GetLog(testPath, versionFrom, versionTo, Recursion.Full, Int32.MaxValue);

            Assert.AreEqual(2, logItem.History.Length);
        }

        [Test]
        public void TestGetItemsReturnsMimeTypeInfo()
        {
            string mimeType = "application/octet-stream";
            string path = testPath + "/TestFile.txt";
            WriteFile(path, "Fun text", false);
            SetProperty(path, "mime-type", mimeType, true);

            FolderMetaData item = (FolderMetaData)provider.GetItems(-1, testPath, Recursion.OneLevel);

            Assert.AreEqual(mimeType, item.Items[0].Properties["mime-type"]);
        }

        [Test]
        public void TestSetMimeTypeOnFile()
        {
            string mimeType = "application/octet-stream";
            string path = testPath + "/TestFile.txt";
            WriteFile(path, "Fun text", true);

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
            WriteFile(path, "Fun text", false);
            SetProperty(path, "mime-type", mimeType1, true);

            SetProperty(path, "mime-type", mimeType2, true);

            FolderMetaData item = (FolderMetaData)provider.GetItems(-1, testPath, Recursion.OneLevel);
            Assert.AreEqual(mimeType2, item.Items[0].Properties["mime-type"]);
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

        [Test, ExpectedException(typeof(FolderAlreadyExistsException))]
        public void TestAddFolderThatAlreadyExistsThrowsException()
        {
            CreateFolder(testPath + "/New Folder", true);

            provider.MakeCollection(activityId, testPath + "/New Folder");
        }

        [Test]
        public void TestRenameFile()
        {
            WriteFile(testPath + "/Fun.txt", "Fun text", true);

            provider.DeleteItem(activityId, testPath + "/Fun.txt");
            provider.CopyItem(activityId, testPath + "/Fun.txt", testPath + "/FunRename.txt");
            Commit();

            LogItem log = provider.GetLog(testPath + "/FunRename.txt", 1, provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
        }

        [Test]
        public void TestRenameFileWithCopyBeforeDelete()
        {
            WriteFile(testPath + "/Fun.txt", "Fun text", true);

            provider.CopyItem(activityId, testPath + "/Fun.txt", testPath + "/FunRename.txt");
            provider.DeleteItem(activityId, testPath + "/Fun.txt");
            Commit();

            LogItem log = provider.GetLog(testPath + "/FunRename.txt", 1, provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
        }

        [Test]
        public void TestRenameFolder()
        {
            CreateFolder(testPath + "/Fun", true);

            provider.DeleteItem(activityId, testPath + "/Fun");
            provider.CopyItem(activityId, testPath + "/Fun", testPath + "/FunRename");
            Commit();

            LogItem log = provider.GetLog(testPath + "/FunRename", 1, provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
        }

        [Test]
        public void TestBranchFile()
        {
            WriteFile(testPath + "/Fun.txt", "Fun text", true);

            provider.CopyItem(activityId, testPath + "/Fun.txt", testPath + "/FunBranch.txt");
            Commit();

            LogItem log = provider.GetLog(testPath + "/FunBranch.txt", 1, provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Branch, log.History[0].Changes[0].ChangeType & ChangeType.Branch);
        }

        [Test]
        public void TestBranchFolder()
        {
            CreateFolder(testPath + "/Fun", true);

            provider.CopyItem(activityId, testPath + "/Fun", testPath + "/FunBranch");
            Commit();

            LogItem log = provider.GetLog(testPath + "/FunBranch", 1, provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Branch, log.History[0].Changes[0].ChangeType & ChangeType.Branch);
        }

        [Test]
        public void TestRenameAndEditFile()
        {
            WriteFile(testPath + "/Fun.txt", "Fun text", true);

            provider.CopyItem(activityId, testPath + "/Fun.txt", testPath + "/FunRename.txt");
            provider.DeleteItem(activityId, testPath + "/Fun.txt");
            byte[] updatedText = Encoding.Default.GetBytes("Test file contents");
            bool created = provider.WriteFile(activityId, testPath + "/FunRename.txt", updatedText);
            Commit();

            LogItem log = provider.GetLog(testPath + "/FunRename.txt", 1, provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename | ChangeType.Edit, log.History[0].Changes[0].ChangeType);
            byte[] actualUpdatedText = ReadFile(testPath + "/FunRename.txt");
            Assert.AreEqual(Encoding.Default.GetString(updatedText), Encoding.Default.GetString(actualUpdatedText));
            Assert.AreEqual(false, created);
        }

        [Test]
        public void TestCommitOfMoveFileOutOfFolderAndDeleteFolder()
        {
            CreateFolder(testPath + "/TestFolder", false);
            bool created = WriteFile(testPath + "/TestFolder/TestFile.txt", "Test file contents", true);

            provider.CopyItem(activityId, testPath + "/TestFolder/TestFile.txt", testPath + "/FunFile.txt");
            provider.DeleteItem(activityId, testPath + "/TestFolder/TestFile.txt");
            provider.DeleteItem(activityId, testPath + "/TestFolder");
            Commit();

            LogItem log = provider.GetLog(testPath + "/FunFile.txt", 1, provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename, log.History[0].Changes[0].ChangeType);
            Assert.IsNull(provider.GetItems(-1, testPath + "/TestFolder", Recursion.None));
        }

        [Test]
        public void TestDeleteFileAlsoDeletesPropertiesOnFile()
        {
            string mimeType = "application/octet-stream";
            string path = testPath + "/TestFile.txt";
            WriteFile(path, "Fun text", false);
            SetProperty(path, "mime-type", mimeType, true);

            DeleteItem(testPath + "/TestFile.txt", true);

            FolderMetaData item = (FolderMetaData)provider.GetItems(-1, testPath, Recursion.OneLevel);
        }

        [Test]
        public void TestGetLogReturnsOriginalNameAndRevisionForRenamedItems()
        {
            WriteFile(testPath + "/Fun.txt", "Fun text", true);
            int versionFrom = provider.GetLatestVersion();
            MoveItem(testPath + "/Fun.txt", testPath + "/FunRename.txt", true);
            int versionTo = provider.GetLatestVersion();

            LogItem logItem = provider.GetLog(testPath + "/FunRename.txt", versionFrom, versionTo, Recursion.None, 1);

            Assert.AreEqual(testPath + "/Fun.txt", ((RenamedSourceItem)logItem.History[0].Changes[0].Item).OriginalRemoteName.Substring(1));
            Assert.AreEqual(versionFrom, ((RenamedSourceItem)logItem.History[0].Changes[0].Item).OriginalRevision);
            Assert.AreEqual(testPath + "/FunRename.txt", logItem.History[0].Changes[0].Item.RemoteName.Substring(1));
        }

        [Test]
        public void TestCommitOfMovedAndEditedFile()
        {
            CreateFolder(testPath + "/Nodes", false);
            WriteFile(testPath + "/Nodes/Fun.txt", "filedata", false);
            CreateFolder(testPath + "/Protocol", true);

            WriteFile(testPath + "/Nodes/Fun.txt", "filedata2", false);
            provider.DeleteItem(activityId, testPath + "/Nodes/Fun.txt");
            provider.CopyItem(activityId, testPath + "/Nodes/Fun.txt", testPath + "/Protocol/Fun.txt");
            Commit();

            LogItem log = provider.GetLog(testPath + "/Protocol/Fun.txt", 1, provider.GetLatestVersion(), Recursion.None, 1);
            Assert.AreEqual(ChangeType.Rename | ChangeType.Edit, log.History[0].Changes[0].ChangeType);
        }
    }
}
