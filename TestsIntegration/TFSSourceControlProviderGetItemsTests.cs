using NUnit.Framework;
using SvnBridge.SourceControl;
using Tests;

namespace IntegrationTests
{
    [TestFixture]
    public class TFSSourceControlProviderGetItemsTests : TFSSourceControlProviderTestsBase
    {
        [Test]
        public void TestGetItemInActivityReturnsCorrectItemIfIsInRenamedFolder()
        {
            CreateFolder(testPath + "/A", false);
            WriteFile(testPath + "/A/Test.txt", "filedata", true);
            DeleteItem(testPath + "/A", false);
            CopyItem(testPath + "/A", testPath + "/B", false);

            ItemMetaData item = _provider.GetItemInActivity(_activityId, testPath + "/B/Test.txt");

            Assert.AreEqual(testPath.Substring(1) + "/A/Test.txt", item.Name);
        }

        [Test]
        public void TestGetItemsForRootSucceeds()
        {
            FolderMetaData item = (FolderMetaData) _provider.GetItems(-1, "", Recursion.OneLevel);
        }

        [Test]
        public void TestGetItemsOnFile()
        {
            WriteFile(testPath + "/File1.txt", "filedata", true);

            ItemMetaData item = _provider.GetItems(-1, testPath + "/File1.txt", Recursion.None);

            Assert.IsNotNull(item);
            Assert.AreEqual(testPath.Substring(1) + "/File1.txt", item.Name);
        }

        [Test]
        public void TestGetItemsOnFileReturnsPropertiesForFile()
        {
            WriteFile(testPath + "/File1.txt", "filedata", false);
            string propvalue = "prop1value";
            SetProperty(testPath + "/File1.txt", "prop1", propvalue, true);

            ItemMetaData item = _provider.GetItems(-1, testPath + "/File1.txt", Recursion.None);

            Assert.AreEqual(propvalue, item.Properties["prop1"]);
        }

        [Test]
        public void TestGetItemsOnFolderReturnsPropertiesForFileWithinFolder()
        {
            string mimeType = "application/octet-stream";
            string path = testPath + "/TestFile.txt";
            WriteFile(path, "Fun text", false);
            SetProperty(path, "mime-type", mimeType, true);

            FolderMetaData item = (FolderMetaData) _provider.GetItems(-1, testPath, Recursion.Full);

            Assert.AreEqual(mimeType, item.Items[0].Properties["mime-type"]);
        }

        [Test]
        public void TestGetItemsOnFolderReturnsPropertiesForFolder()
        {
            string ignore = "*.bad\n";
            SetProperty(testPath, "ignore", ignore, true);

            FolderMetaData item = (FolderMetaData) _provider.GetItems(-1, testPath, Recursion.Full);

            Assert.AreEqual(ignore, item.Properties["ignore"]);
        }

        [Test]
        public void TestGetItemsReturnsCorrectRevisionWhenPropertyHasBeenAddedToFileAndRecursionIsFull()
        {
            WriteFile(testPath + "/Test.txt", "whee", true);
            SetProperty(testPath + "/Test.txt", "prop1", "val1", true);
            int revision = _lastCommitRevision;

            FolderMetaData item = (FolderMetaData) _provider.GetItems(-1, testPath, Recursion.Full);

            Assert.AreEqual(revision, item.Items[0].Revision);
        }

        [Test]
        public void TestGetItemsReturnsCorrectRevisionWhenPropertyHasBeenAddedToFolderAndRecursionIsFull()
        {
            CreateFolder(testPath + "/Folder1", true);
            SetProperty(testPath + "/Folder1", "prop1", "val1", true);
            int revision = _lastCommitRevision;

            FolderMetaData item = (FolderMetaData) _provider.GetItems(-1, testPath, Recursion.Full);

            Assert.AreEqual(revision, item.Items[0].Revision);
        }

        [Test]
        public void TestGetItemsReturnsCorrectRevisionWhenPropertyIsAdded()
        {
            WriteFile(testPath + "/File1.txt", "filedata", true);
            SetProperty(testPath + "/File1.txt", "prop1", "val1", true);
            int revision = _lastCommitRevision;

            ItemMetaData item = _provider.GetItems(-1, testPath + "/File1.txt", Recursion.None);

            Assert.AreEqual(revision, item.Revision);
        }

        [Test]
        public void TestGetItemsReturnsCorrectRevisionWhenPropertyIsAddedThenFileIsUpdated()
        {
            WriteFile(testPath + "/File1.txt", "filedata", true);
            SetProperty(testPath + "/File1.txt", "prop1", "val1", true);
            WriteFile(testPath + "/File1.txt", "filedata2", true);
            int revision = _lastCommitRevision;

            ItemMetaData item = _provider.GetItems(-1, testPath + "/File1.txt", Recursion.None);

            Assert.AreEqual(revision, item.Revision);
        }

        [Test]
        public void TestGetItemsWithAllRecursionLevelsReturnsPropertyForFolder()
        {
            CreateFolder(testPath + "/Folder1", false);
            string ignore = "*.bad\n";
            SetProperty(testPath + "/Folder1", "ignore", ignore, true);

            FolderMetaData item1 = (FolderMetaData) _provider.GetItems(-1, testPath + "/Folder1", Recursion.Full);
            FolderMetaData item2 = (FolderMetaData) _provider.GetItems(-1, testPath + "/Folder1", Recursion.OneLevel);
            FolderMetaData item3 = (FolderMetaData) _provider.GetItems(-1, testPath + "/Folder1", Recursion.None);

            Assert.AreEqual(ignore, item1.Properties["ignore"]);
            Assert.AreEqual(ignore, item2.Properties["ignore"]);
            Assert.AreEqual(ignore, item3.Properties["ignore"]);
        }
    }
}