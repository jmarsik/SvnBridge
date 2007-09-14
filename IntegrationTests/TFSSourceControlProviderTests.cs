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
        public void TestItemExistsReturnsTrueIfFileExists()
        {
            WriteFile(_testPath + "/TestFile.txt", "Fun text", true);

            bool result = _provider.ItemExists(_testPath + "/TestFile.txt");

            Assert.IsTrue(result);
        }

        [Test]
        public void TestItemExistsReturnsFalseIfFileDoesNotExist()
        {
            bool result = _provider.ItemExists(_testPath + "/TestFile.txt");

            Assert.IsFalse(result);
        }

        [Test]
        public void TestItemExistsReturnsFalseIfFileDoesNotExistInSpecifiedVersion()
        {
            int version = _lastCommitRevision;
            WriteFile(_testPath + "/TestFile.txt", "Fun text", true);
            
            bool result = _provider.ItemExists(_testPath + "/TestFile.txt", version);

            Assert.IsFalse(result);
        }

        [Test, ExpectedException(typeof(FolderAlreadyExistsException))]
        public void TestAddFolderThatAlreadyExistsThrowsException()
        {
            CreateFolder(_testPath + "/New Folder", true);

            _provider.MakeCollection(_activityId, _testPath + "/New Folder");
        }

        [Test]
        public void TestDeleteItemReturnsTrueWhenFileExists()
        {
            WriteFile(_testPath + "/File.txt", "filedata", true);

            bool result = _provider.DeleteItem(_activityId, _testPath + "/File.txt");

            Assert.IsTrue(result);
        }

        [Test]
        public void TestDeleteItemReturnsFalseIfFileDoesNotExist()
        {
            bool result = _provider.DeleteItem(_activityId, _testPath + "/NotHere.txt");

            Assert.IsFalse(result);
        }
    }
}