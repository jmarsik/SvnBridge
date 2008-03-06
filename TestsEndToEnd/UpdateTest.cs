using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace TestsEndToEnd
{
    [TestFixture]
    public class UpdateTest : EndToEndTestBase
    {
        [Test]
        public void CanUpdateWorkingCopy()
        {
            CheckoutAndChangeDirectory();
            File.WriteAllText("test.txt", "hab");
            Svn("commit -m blah");
            WriteFile(testPath + "/test2.txt", "blah", true);
            string output = Svn("update");
            Assert.IsTrue(
                output.Contains("A    test2.txt")
                );
        }

        [Test]
        public void CanUpdateWorkingCopyToPreviousVersion()
        {
            CheckoutAndChangeDirectory();
            File.WriteAllText("test.txt", "hab");
            Svn("add test.txt");
            Svn("commit -m blah");
            Svn("update");
            Svn("update test.txt --revision PREV");
        }

        [Test]
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

        [Test]
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
            Assert.IsTrue(
                Regex.IsMatch(svn,@"^At revision \d+\.\r\n$")
                );
        }


        [Test]
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

            Assert.AreEqual("12312", File.ReadAllText("foo.bar"));
        }


        [Test]
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

            Assert.AreEqual("12312", File.ReadAllText("foo.bar"));
        }

        [Test]
        public void UpdatingFolderWhenItIsMissingInWorkingCopy()
        {
            CheckoutAndChangeDirectory();

            Directory.CreateDirectory("foo");

            Svn("add foo");
            Svn("commit -m blah");

            Svn("propset blah b .");
            Svn("commit -m blah");

            Svn("update foo --revision PREV");

            Assert.IsFalse(Directory.Exists("foo"));

            Svn("update");

            Assert.IsTrue(Directory.Exists("foo"));
        }


        [Test]
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

            Assert.AreEqual("hab", File.ReadAllText("test.txt"));
        }

        [Test]
        public void WhenFileInFolderIsInPreviousVersionAndUpdatingToLatestShouldUpdateFile()
        {
            CheckoutAndChangeDirectory();

            CreateFolder(testPath + "/TestFolder1", true);
            WriteFile(testPath + "/TestFolder1/blah.txt", "abc", true);
            UpdateFile(testPath + "/TestFolder1/blah.txt", "def", true);

            Svn("update");
            Svn("update TestFolder1/blah.txt --revision PREV");
            Assert.AreEqual("abc", File.ReadAllText("TestFolder1/blah.txt"));
            Svn("update");
            Assert.AreEqual("def", File.ReadAllText("TestFolder1/blah.txt"));
        }
    }
}
