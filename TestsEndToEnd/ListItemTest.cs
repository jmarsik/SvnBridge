using System;
using NUnit.Framework;

namespace TestsEndToEnd
{
    [TestFixture]
    public class ListItemTest : EndToEndTestBase
    {
        [Test]
        public void CanListFolderAndFile()
        {
            CreateFolder(testPath + "/TestFolder1", true);
            WriteFile(testPath + "/test.txt", "blah", true);

            string actual = ExecuteCommand("list " + testUrl);
            string expected = @"TestFolder1/
test.txt
";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanListFolders()
        {
            CreateFolder(testPath + "/TestFolder1", true);
            CreateFolder(testPath + "/TestFolder2", true);

            string actual = ExecuteCommand("list " + testUrl);
            string expected = @"TestFolder1/
TestFolder2/
";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanListFoldersAndFilesRecursively()
        {
            CreateFolder(testPath + "/TestFolder1", true);
            CreateFolder(testPath + "/TestFolder2", true);
            WriteFile(testPath + "/TestFolder2/text.txt", "blah", true);

            string actual = ExecuteCommand("list " + testUrl + " --recursive");
            string expected = @"TestFolder1/
TestFolder2/
TestFolder2/text.txt
";
            Assert.AreEqual(expected, actual);
        }


        [Test]
        public void CanListPreviousVersion()
        {
            int version = CreateFolder(testPath + "/TestFolder1", true);
            WriteFile(testPath + "/test.txt", "blah", true); // here we create a new version

            string actual = ExecuteCommand("list " + testUrl + " --revision " + version);
            string expected = @"TestFolder1/
";
            Assert.AreEqual(expected, actual);
        }


        [Test]
        public void CanListPreviousVersion_UsingPrev()
        {
            CreateFolder(testPath + "/TestFolder1", true);
            WriteFile(testPath + "/test.txt", "blah", true); // here we create a new version
            CheckoutAndChangeDirectory();
            WriteFile(testPath + "/test.txt", "foo", true);
            ExecuteCommand("update");
            string actual = ExecuteCommand("list test.txt --revision PREV");
            string expected = @"test.txt
";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanListPreviousVersion_WhenDirectoryDoesNotExists()
        {
            CheckoutAndChangeDirectory();
            string actual = ExecuteCommandAndGetError("list --revision PREV");
            string expected = @"svn: Unable to find repository location for '' in revision";
            Assert.IsTrue(
                actual.StartsWith(expected)
                );
        }

        [Test]
        public void CanListPreviousVersionUsingDate()
        {
            TemporaryIgnore("SvnBridge doesn't support dated-rev-report");

            CreateFolder(testPath + "/TestFolder1", true);
            DateTime commitDate = DateTime.Now;

            WriteFile(testPath + "/test.txt", "blah", true); // here we create a new version

            string actual =
                ExecuteCommand("list " + testUrl + " --revision {" + commitDate.ToString("yyyyMMddTHHmmss") + "}");
            string expected = @"TestFolder1/
";
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CanListSingleFolder()
        {
            CreateFolder(testPath + "/TestFolder", true);
            string actual = ExecuteCommand("list " + testUrl);
            string expected = @"TestFolder/
";
            Assert.AreEqual(expected, actual);
        }
    }
}