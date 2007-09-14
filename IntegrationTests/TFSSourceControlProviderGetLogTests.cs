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
    public class TFSSourceControlProviderGetLogTests : TFSSourceControlProviderTestsBase
    {
        [Test]
        public void TestGetLog()
        {
            int versionFrom = _lastCommitRevision;
            WriteFile(_testPath + "/TestFile.txt", "Fun text", true);
            int versionTo = _lastCommitRevision;

            LogItem logItem = _provider.GetLog(_testPath, versionFrom, versionTo, Recursion.Full, Int32.MaxValue);

            Assert.AreEqual(2, logItem.History.Length);
        }

        [Test]
        public void TestGetLogWithBranchedFileContainsOriginalNameAndRevision()
        {
            WriteFile(_testPath + "/TestFile.txt", "Fun text", true);
            int versionFrom = _lastCommitRevision;
            CopyItem(_testPath + "/TestFile.txt", _testPath + "/TestFileBranch.txt", true);
            int versionTo = _lastCommitRevision;

            LogItem logItem = _provider.GetLog(_testPath, versionTo, versionTo, Recursion.Full, Int32.MaxValue);

            Assert.AreEqual(ChangeType.Branch, logItem.History[0].Changes[0].ChangeType & ChangeType.Branch);
            Assert.AreEqual(_testPath + "/TestFile.txt", ((RenamedSourceItem)logItem.History[0].Changes[0].Item).OriginalRemoteName.Substring(1));
            Assert.AreEqual(versionFrom, ((RenamedSourceItem)logItem.History[0].Changes[0].Item).OriginalRevision);
        }

        [Test]
        public void TestGetLogWithBranchedFileContainsOriginalVersionAsRevisionImmediatelyBeforeBranch()
        {
            WriteFile(_testPath + "/TestFile.txt", "Fun text", true);
            WriteFile(_testPath + "/TestFile2.txt", "Fun text", true);
            int versionFrom = _lastCommitRevision;
            CopyItem(_testPath + "/TestFile.txt", _testPath + "/TestFileBranch.txt", true);
            int versionTo = _lastCommitRevision;

            LogItem logItem = _provider.GetLog(_testPath, versionTo, versionTo, Recursion.Full, Int32.MaxValue);

            Assert.AreEqual(versionFrom, ((RenamedSourceItem)logItem.History[0].Changes[0].Item).OriginalRevision);
        }

        [Test]
        public void TestGetLogReturnsOriginalNameAndRevisionForRenamedItems()
        {
            WriteFile(_testPath + "/Fun.txt", "Fun text", true);
            int versionFrom = _lastCommitRevision;
            MoveItem(_testPath + "/Fun.txt", _testPath + "/FunRename.txt", true);
            int versionTo = _lastCommitRevision;

            LogItem logItem = _provider.GetLog(_testPath + "/FunRename.txt", versionFrom, versionTo, Recursion.None, 1);

            Assert.AreEqual(_testPath + "/Fun.txt", ((RenamedSourceItem)logItem.History[0].Changes[0].Item).OriginalRemoteName.Substring(1));
            Assert.AreEqual(versionFrom, ((RenamedSourceItem)logItem.History[0].Changes[0].Item).OriginalRevision);
            Assert.AreEqual(_testPath + "/FunRename.txt", logItem.History[0].Changes[0].Item.RemoteName.Substring(1));
        }
    }
}