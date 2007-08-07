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
        public void TestGetItemsReturnsIgnoreInfo()
        {
            string ignore = "*.bad\n";
            SetProperty(_testPath, "ignore", ignore, true);

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.Full);

            Assert.AreEqual(ignore, item.Properties["ignore"]);
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

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.Full);

            Assert.AreEqual(mimeType, item.Items[0].Properties["mime-type"]);
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
    }
}