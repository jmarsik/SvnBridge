using System;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace TestsEndToEnd
{
    public class UpdateTest : EndToEndTestBase
    {
        [Fact]
        public void CanUpdateWorkingCopy()
        {
            CheckoutAndChangeDirectory();
            File.WriteAllText("test.txt", "hab");
            Svn("commit -m blah");
            WriteFile(testPath + "/test2.txt", "blah", true);
            string output = Svn("update");
            Assert.True(
                output.Contains("A    test2.txt")
                );
        }

        [Fact]
        public void CanUpdateWorkingCopyToPreviousVersion()
        {
            CheckoutAndChangeDirectory();
            File.WriteAllText("test.txt", "hab");
            Svn("add test.txt");
            Svn("commit -m blah");
            Svn("update");
            Svn("update test.txt --revision PREV");
        }

        [Fact]
        public void CanUpdateWorkingCopyToPreviousVersion_AndRemoveFolder()
        {
            CheckoutAndChangeDirectory();
            Directory.CreateDirectory("foo");
            File.WriteAllText("foo/test.txt", "hab");
            Svn("add foo");
            Svn("commit -m blah");
            Svn("update");
            Svn("update foo --revision PREV");
        }

        [Fact]
        public void AfterAnErrorWhenGettingFile_WillBeAbleToUpdateAgain()
        {
            CheckoutAndChangeDirectory();

            File.WriteAllText("foo.bar", "blah");

            Svn("add foo.bar");
            Svn("commit -m blah");

            WriteFile(testPath + "/test.txt", "as", true);
            File.WriteAllText("test.txt", "hab");
            ExecuteCommandAndGetError("update");
            File.Delete("test.txt");

            string svn = Svn("update");
            Assert.True(
                Regex.IsMatch(svn,@"^At revision \d+\.\r\n$")
                );
        }


        [Fact]
        public void AfterAnErrorWhenGettingFile_WillBeAbleToUpdateAgain_AndGetModifiedFile()
        {
            CheckoutAndChangeDirectory();

            File.WriteAllText("foo.bar", "blah");

            Svn("add foo.bar");
            Svn("commit -m blah");

            WriteFile(testPath + "/test.txt", "as", true);
            File.WriteAllText("test.txt", "hab");
            ExecuteCommandAndGetError("update");
            File.Delete("test.txt");

            WriteFile(testPath + "/foo.bar", "12312", true);

            Svn("update");

            Assert.Equal("12312", File.ReadAllText("foo.bar"));
        }


        [Fact]
        public void UpdatingFileWhenItIsMissingInWorkingCopy()
        {
            CheckoutAndChangeDirectory();

            File.WriteAllText("foo.bar", "12312");

            Svn("add foo.bar");
            Svn("commit -m blah");

            Svn("propset blah b .");
            Svn("commit -m blah");

            Svn("update foo.bar --revision PREV");

            Svn("update");

            Assert.Equal("12312", File.ReadAllText("foo.bar"));
        }

        [Fact]
        public void UpdatingFolderWhenItIsMissingInWorkingCopy()
        {
            CheckoutAndChangeDirectory();

            Directory.CreateDirectory("foo");

            Svn("add foo");
            Svn("commit -m blah");

            Svn("propset blah b .");
            Svn("commit -m blah");

            Svn("update foo --revision PREV");

            Assert.False(Directory.Exists("foo"));

            Svn("update");

            Assert.True(Directory.Exists("foo"));
        }


        [Fact]
        public void CanGetLatestChangesWhenMovingBackward()
        {
            CheckoutAndChangeDirectory();

            // v 1.0
            File.WriteAllText("test.txt", "hab");
            Svn("add test.txt");
            Svn("commit -m blah");

            // v 2.0
            File.WriteAllText("test2.txt", "hab");
            Svn("add test2.txt");
            Svn("commit -m blah");

            // v 3.0
            File.WriteAllText("test.txt", "hab123");
            Svn("commit -m blah2");

            int previousVersion = _provider.GetLatestVersion() - 1;

            Svn("update");

            Svn("update test.txt --revision " + previousVersion);

            Assert.Equal("hab", File.ReadAllText("test.txt"));
        }

        [Fact]
        public void WhenFileInFolderIsInPreviousVersionAndUpdatingToLatestShouldUpdateFile()
        {
            CheckoutAndChangeDirectory();

            CreateFolder(testPath + "/TestFolder1", true);
            WriteFile(testPath + "/TestFolder1/blah.txt", "abc", true);
            UpdateFile(testPath + "/TestFolder1/blah.txt", "def", true);

            Svn("update");
            Svn("update TestFolder1/blah.txt --revision PREV");
            Assert.Equal("abc", File.ReadAllText("TestFolder1/blah.txt"));
            Svn("update");
            Assert.Equal("def", File.ReadAllText("TestFolder1/blah.txt"));
        }
    }
}
