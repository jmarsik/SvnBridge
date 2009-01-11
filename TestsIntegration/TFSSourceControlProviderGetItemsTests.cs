using System;
using SvnBridge.SourceControl;
using Xunit;

namespace IntegrationTests
{
    public class TFSSourceControlProviderGetItemsTests : TFSSourceControlProviderTestsBase
    {
        [IntegrationTestFact]
        public void GetItemInActivity_ReturnsCorrectItemIfIsInRenamedFolder()
        {
            CreateFolder(MergePaths(testPath, "/A"), false);
            WriteFile(MergePaths(testPath, "/A/Test.txt"), "filedata", true);
            DeleteItem(MergePaths(testPath, "/A"), false);
            CopyItem(MergePaths(testPath, "/A"), MergePaths(testPath, "/B"), false);

            ItemMetaData item = _provider.GetItemInActivity(_activityId, MergePaths(testPath, "/B/Test.txt"));

            Assert.Equal(MergePaths(testPath, "/A/Test.txt").Substring(1), item.Name);
        }

        [IntegrationTestFact]
        public void GetItems_ForRootSucceedsWithAllRecursionLevels()
        {
            _provider.GetItems(-1, "", Recursion.None);
            _provider.GetItems(-1, "", Recursion.OneLevel);
            _provider.GetItems(-1, "", Recursion.Full);
        }

        [IntegrationTestFact]
        public void GetItems_ForRootAndRootFolderHasProperties_ReturnsCorrectRevision()
        {
            SetProperty(testPath, "prop1", "val1", true);

            ItemMetaData folder = _provider.GetItems(-1, testPath, Recursion.Full);

            Assert.Equal(_lastCommitRevision, folder.Revision);
        }

        [IntegrationTestFact]
        public void GetItems_OnFile()
        {
            WriteFile(MergePaths(testPath, "/File1.txt"), "filedata", true);

            ItemMetaData item = _provider.GetItems(-1, MergePaths(testPath, "/File1.txt"), Recursion.None);

            Assert.NotNull(item);
            Assert.Equal(MergePaths(testPath, "/File1.txt").Substring(1), item.Name);
        }

        [IntegrationTestFact]
        public void GetItems_OnFileReturnsPropertiesForFile()
        {
            WriteFile(MergePaths(testPath, "/File1.txt"), "filedata", false);
            string propvalue = "prop1value";
            SetProperty(MergePaths(testPath, "/File1.txt"), "prop1", propvalue, true);

            ItemMetaData item = _provider.GetItems(-1, MergePaths(testPath, "/File1.txt"), Recursion.None);

            Assert.Equal(propvalue, item.Properties["prop1"]);
        }

        [IntegrationTestFact]
        public void GetItems_OnFolderReturnsPropertiesForFileWithinFolder()
        {
            string mimeType = "application/octet-stream";
            string path = MergePaths(testPath, "/TestFile.txt");
            WriteFile(path, "Fun text", false);
            SetProperty(path, "mime-type", mimeType, true);

            FolderMetaData folder = (FolderMetaData) _provider.GetItems(-1, testPath, Recursion.Full);

            Assert.Equal(1, folder.Items.Count);
            Assert.Equal(mimeType, folder.Items[0].Properties["mime-type"]);
        }

        [IntegrationTestFact]
        public void GetItems_OnFolderReturnsPropertiesForFolder()
        {
            string ignore = "*.bad\n";
            SetProperty(testPath, "ignore", ignore, true);

            FolderMetaData item = (FolderMetaData) _provider.GetItems(-1, testPath, Recursion.Full);

            Assert.Equal(0, item.Items.Count);
            Assert.Equal(ignore, item.Properties["ignore"]);
        }

        [IntegrationTestFact]
        public void GetItems_OnFolderContainingUpdatesWithRecursionFull_ReturnsLatestChangesetOfContainedItems()
        {
            WriteFile(MergePaths(testPath, "/Test.txt"), "whee", true);

            FolderMetaData folder = (FolderMetaData)_provider.GetItems(-1, testPath, Recursion.Full);

            ItemMetaData item = _provider.GetItems(-1, MergePaths(testPath, "/Test.txt"), Recursion.None);
            Assert.Equal(item.Revision, folder.Revision);
            Assert.Equal(item.LastModifiedDate, folder.LastModifiedDate);
        }

        [IntegrationTestFact]
        public void GetItems_OnFolderContainingUpdatesWithRecursionNone_ReturnsLatestChangesetOfContainedItems()
        {
            WriteFile(MergePaths(testPath, "/Test.txt"), "whee", true);

            FolderMetaData folder = (FolderMetaData)_provider.GetItems(-1, testPath, Recursion.None);

            ItemMetaData item = _provider.GetItems(-1, MergePaths(testPath, "/Test.txt"), Recursion.None);
            Assert.Equal(item.Revision, folder.Revision);
            Assert.Equal(item.LastModifiedDate, folder.LastModifiedDate);
        }

        [IntegrationTestFact]
        public void GetItems_ReturnsCorrectRevisionWhenPropertyHasBeenAddedToFileAndRecursionIsFull()
        {
            WriteFile(MergePaths(testPath, "/Test.txt"), "whee", true);
            SetProperty(MergePaths(testPath, "/Test.txt"), "prop1", "val1", true);
            int revision = _lastCommitRevision;

            FolderMetaData item = (FolderMetaData) _provider.GetItems(-1, testPath, Recursion.Full);

            Assert.Equal(revision, item.Items[0].Revision);
        }

        [IntegrationTestFact]
        public void GetItems_ReturnsCorrectRevisionWhenPropertyHasBeenAddedToFolderAndRecursionIsFull()
        {
            CreateFolder(MergePaths(testPath, "/Folder1"), true);
            SetProperty(MergePaths(testPath, "/Folder1"), "prop1", "val1", true);
            int revision = _lastCommitRevision;

            FolderMetaData item = (FolderMetaData) _provider.GetItems(-1, testPath, Recursion.Full);

            Assert.Equal(revision, item.Items[0].Revision);
        }

        [IntegrationTestFact]
        public void GetItems_ReturnsCorrectRevisionWhenPropertyIsAdded()
        {
            WriteFile(MergePaths(testPath, "/File1.txt"), "filedata", true);
            SetProperty(MergePaths(testPath, "/File1.txt"), "prop1", "val1", true);
            int revision = _lastCommitRevision;

            ItemMetaData item = _provider.GetItems(-1, MergePaths(testPath, "/File1.txt"), Recursion.None);

            Assert.Equal(revision, item.Revision);
        }

        [IntegrationTestFact]
        public void GetItems_ReturnsCorrectRevisionWhenPropertyIsAddedThenFileIsUpdated()
        {
            WriteFile(MergePaths(testPath, "/File1.txt"), "filedata", true);
            SetProperty(MergePaths(testPath, "/File1.txt"), "prop1", "val1", true);
            WriteFile(MergePaths(testPath, "/File1.txt"), "filedata2", true);
            int revision = _lastCommitRevision;

            ItemMetaData item = _provider.GetItems(-1, MergePaths(testPath, "/File1.txt"), Recursion.None);

            Assert.Equal(revision, item.Revision);
        }

        [IntegrationTestFact]
        public void GetItems_WithAllRecursionLevelsReturnsPropertyForFolder()
        {
            CreateFolder(MergePaths(testPath, "/Folder1"), false);
            string ignore = "*.bad\n";
            SetProperty(MergePaths(testPath, "/Folder1"), "ignore", ignore, true);

            FolderMetaData item1 = (FolderMetaData) _provider.GetItems(-1, MergePaths(testPath, "/Folder1"), Recursion.Full);
            FolderMetaData item2 = (FolderMetaData) _provider.GetItems(-1, MergePaths(testPath, "/Folder1"), Recursion.OneLevel);
            FolderMetaData item3 = (FolderMetaData) _provider.GetItems(-1, MergePaths(testPath, "/Folder1"), Recursion.None);

            Assert.Equal(ignore, item1.Properties["ignore"]);
            Assert.Equal(ignore, item2.Properties["ignore"]);
            Assert.Equal(ignore, item3.Properties["ignore"]);
        }

        [IntegrationTestFact]
        public void GetItems_WithOneLevelRecursionReturnsPropertiesForSubFolders()
        {
            CreateFolder(MergePaths(testPath, "/Folder1"), false);
            CreateFolder(MergePaths(testPath, "/Folder1/SubFolder"), false);
            string ignore1 = "*.bad1\n";
            string ignore2 = "*.bad2\n";
            SetProperty(MergePaths(testPath, "/Folder1"), "ignore", ignore1, false);
            SetProperty(MergePaths(testPath, "/Folder1/SubFolder"), "ignore", ignore2, true);

            FolderMetaData item = (FolderMetaData)_provider.GetItems(-1, MergePaths(testPath, "/Folder1"), Recursion.OneLevel);

            Assert.Equal(ignore1, item.Properties["ignore"]);
            Assert.Equal(ignore2, item.Items[0].Properties["ignore"]);
        }

        [IntegrationTestFact]
        public void GetItems_IgnoresStalePropertyFiles()
        {
            string propertyFile = "<?xml version=\"1.0\" encoding=\"utf-8\"?><ItemProperties xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><Properties><Property><Name>mime-type</Name><Value>application/octet-stream</Value></Property></Properties></ItemProperties>";
            CreateFolder(MergePaths(testPath, "/..svnbridge"), true);
            WriteFile(MergePaths(testPath, "/..svnbridge/WheelMUD Database Creation.sql"), GetBytes(propertyFile), true);

            Assert.DoesNotThrow(delegate
            {
                _provider.GetItems(-1, testPath, Recursion.Full);     
            });
        }

        [IntegrationTestFact]
        public void GetItems_RequestedRevision0ForRoot_ReturnsFirstRevision()
        {
            ItemMetaData item = _provider.GetItems(0, "", Recursion.None);

            Assert.NotNull(item);
        }
    }
}