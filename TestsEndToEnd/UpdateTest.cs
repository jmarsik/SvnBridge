using System;
using System.IO;
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

            int previousVersion = _provider.GetLatestVersion() -1 ;

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
