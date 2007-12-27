using System;
using System.Collections.Generic;
using System.Text;
using Tests;
using SvnBridge.SourceControl;
using NUnit.Framework;

namespace IntegrationTests
{
    [TestFixture]
    public class TFSSourceControlProviderGetItemsTests : TFSSourceControlProviderTestsBase
    {
        [Test]
        public void TestGetItemsOnFolderReturnsPropertiesForFolder()
        {
            string ignore = "*.bad\n";
            SetProperty(_testPath, "ignore", ignore, true);

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.Full);

            Assert.AreEqual(ignore, item.Properties["ignore"]);
        }

        [Test]
        public void TestGetItemsOnFolderReturnsPropertiesForFileWithinFolder()
        {
            string mimeType = "application/octet-stream";
            string path = _testPath + "/TestFile.txt";
            WriteFile(path, "Fun text", false);
            SetProperty(path, "mime-type", mimeType, true);

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.Full);

            Assert.AreEqual(mimeType, item.Items[0].Properties["mime-type"]);
        }

        [Test]
        public void TestGetItemsWithAllRecursionLevelsReturnsPropertyForFolder()
        {
            CreateFolder(_testPath + "/Folder1", false);
            string ignore = "*.bad\n";
            SetProperty(_testPath + "/Folder1", "ignore", ignore, true);

            FolderMetaData item1 = (FolderMetaData)_provider.GetItems(-1, _testPath + "/Folder1", Recursion.Full);
            FolderMetaData item2 = (FolderMetaData)_provider.GetItems(-1, _testPath + "/Folder1", Recursion.OneLevel);
            FolderMetaData item3 = (FolderMetaData)_provider.GetItems(-1, _testPath + "/Folder1", Recursion.None);

            Assert.AreEqual(ignore, item1.Properties["ignore"]);
            Assert.AreEqual(ignore, item2.Properties["ignore"]);
            Assert.AreEqual(ignore, item3.Properties["ignore"]);
        }

        [Test]
        public void TestGetItemsOnFile()
        {
            WriteFile(_testPath + "/File1.txt", "filedata", true);

            ItemMetaData item = _provider.GetItems(-1, _testPath + "/File1.txt", Recursion.None);

            Assert.IsNotNull(item);
            Assert.AreEqual(_testPath.Substring(1) + "/File1.txt", item.Name);
        }

        [Test]
        public void TestGetItemsOnFileReturnsPropertiesForFile()
        {
            WriteFile(_testPath + "/File1.txt", "filedata", false);
            string propvalue = "prop1value";
            SetProperty(_testPath + "/File1.txt", "prop1", propvalue, true);

            ItemMetaData item = _provider.GetItems(-1, _testPath + "/File1.txt", Recursion.None);

            Assert.AreEqual(propvalue, item.Properties["prop1"]);
        }

        [Test]
        public void TestGetItemsForRootSucceeds()
        {
            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, "", Recursion.OneLevel);
        }
        [Test]
        public void TestGetItemsReturnsCorrectRevisionWhenPropertyIsAdded()
        {
            WriteFile(_testPath + "/File1.txt", "filedata", true);
            SetProperty(_testPath + "/File1.txt", "prop1", "val1", true);
            int revision = _lastCommitRevision;

            ItemMetaData item = _provider.GetItems(-1, _testPath + "/File1.txt", Recursion.None);

            Assert.AreEqual(revision, item.Revision);
        }

        [Test]
        public void TestGetItemsReturnsCorrectRevisionWhenPropertyIsAddedThenFileIsUpdated()
        {
            WriteFile(_testPath + "/File1.txt", "filedata", true);
            SetProperty(_testPath + "/File1.txt", "prop1", "val1", true);
            WriteFile(_testPath + "/File1.txt", "filedata2", true);
            int revision = _lastCommitRevision;

            ItemMetaData item = _provider.GetItems(-1, _testPath + "/File1.txt", Recursion.None);

            Assert.AreEqual(revision, item.Revision);
        }

        [Test]
        public void TestGetItemsReturnsCorrectRevisionWhenPropertyHasBeenAddedToFolderAndRecursionIsFull()
        {
            CreateFolder(_testPath + "/Folder1", true);
            SetProperty(_testPath + "/Folder1", "prop1", "val1", true);
            int revision = _lastCommitRevision;

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.Full);

            Assert.AreEqual(revision, item.Items[0].Revision);
        }

        [Test]
        public void TestGetItemsReturnsCorrectRevisionWhenPropertyHasBeenAddedToFileAndRecursionIsFull()
        {
            WriteFile(_testPath + "/Test.txt", "whee", true);
            SetProperty(_testPath + "/Test.txt", "prop1", "val1", true);
            int revision = _lastCommitRevision;

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, _testPath, Recursion.Full);

            Assert.AreEqual(revision, item.Items[0].Revision);
        }

        [Test]
        public void TestGetItemInActivityReturnsCorrectItemIfIsInRenamedFolder()
        {
            CreateFolder(_testPath + "/A", false);
            WriteFile(_testPath + "/A/Test.txt", "filedata", true);
            DeleteItem(_testPath + "/A", false);
            CopyItem(_testPath + "/A", _testPath + "/B", false);

            ItemMetaData item = _provider.GetItemInActivity(_activityId, _testPath + "/B/Test.txt");

            Assert.AreEqual(_testPath.Substring(1) + "/A/Test.txt", item.Name);
        }
    }
}
