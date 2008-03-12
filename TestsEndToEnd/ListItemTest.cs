using System;
using Xunit;

namespace TestsEndToEnd
{
    public class ListItemTest : EndToEndTestBase
    {
        [Fact]
        public void CanListFolderAndFile()
        {
            CreateFolder(testPath + "/TestFolder1", true);
            WriteFile(testPath + "/test.txt", "blah", true);

            string actual = Svn("list " + testUrl);
            string expected = @"TestFolder1/
test.txt
";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanListFolders()
        {
            CreateFolder(testPath + "/TestFolder1", true);
            CreateFolder(testPath + "/TestFolder2", true);

            string actual = Svn("list " + testUrl);
            string expected = @"TestFolder1/
TestFolder2/
";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanListFoldersAndFilesRecursively()
        {
            CreateFolder(testPath + "/TestFolder1", true);
            CreateFolder(testPath + "/TestFolder2", true);
            WriteFile(testPath + "/TestFolder2/text.txt", "blah", true);

            string actual = Svn("list " + testUrl + " --recursive");
            string expected = @"TestFolder1/
TestFolder2/
TestFolder2/text.txt
";
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void CanListPreviousVersion()
        {
            int version = CreateFolder(testPath + "/TestFolder1", true);
            WriteFile(testPath + "/test.txt", "blah", true); // here we create a new version

            string actual = Svn("list " + testUrl + " --revision " + version);
            string expected = @"TestFolder1/
";
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void CanListPreviousVersion_UsingPrev()
        {
            CreateFolder(testPath + "/TestFolder1", true);
            WriteFile(testPath + "/test.txt", "blah", true); // here we create a new version
            CheckoutAndChangeDirectory();
            WriteFile(testPath + "/test.txt", "foo", true);
            Svn("update");
            string actual = Svn("list test.txt --revision PREV");
            string expected = @"test.txt
";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanListPreviousVersion_WhenDirectoryDoesNotExists()
        {
            CheckoutAndChangeDirectory();
            string actual = ExecuteCommandAndGetError("list --revision PREV");
            string expected = @"svn: Unable to find repository location for '' in revision";
            Assert.True(
                actual.StartsWith(expected)
                );
        }

        [Fact(Skip="SvnBridge doesn't support dated-rev-report")]
        public void CanListPreviousVersionUsingDate()
        {
            CreateFolder(testPath + "/TestFolder1", true);
            DateTime commitDate = DateTime.Now;

            WriteFile(testPath + "/test.txt", "blah", true); // here we create a new version

            string actual =
                Svn("list " + testUrl + " --revision {" + commitDate.ToString("yyyyMMddTHHmmss") + "}");
            string expected = @"TestFolder1/
";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanListSingleFolder()
        {
            CreateFolder(testPath + "/TestFolder", true);
            string actual = Svn("list " + testUrl);
            string expected = @"TestFolder/
";
            Assert.Equal(expected, actual);
        }
    }
}
