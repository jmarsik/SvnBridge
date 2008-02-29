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
            ExecuteCommand("commit -m blah");
            WriteFile(testPath + "/test2.txt", "blah", true);
            string output = ExecuteCommand("update");
            Assert.IsTrue(
                output.Contains("A    test2.txt")
                );
        }

        [Test]
        public void CanUpdateWorkingCopyToPreviousVersion()
        {
            CheckoutAndChangeDirectory();
            File.WriteAllText("test.txt", "hab");
            ExecuteCommand("add test.txt");
            ExecuteCommand("commit -m blah");
            ExecuteCommand("update");
            ExecuteCommand("update test.txt --revision PREV");
        }

        [Test]
        public void CanUpdateWorkingCopyToPreviousVersion_AndRemoveFolder()
        {
            CheckoutAndChangeDirectory();
            Directory.CreateDirectory("foo");
            File.WriteAllText("foo/test.txt", "hab");
            ExecuteCommand("add foo");
            ExecuteCommand("commit -m blah");
            ExecuteCommand("update");
            ExecuteCommand("update foo --revision PREV");
        }

        [Test]
        public void CanGetLatestChangesWhenMovingBackward()
        {
            CheckoutAndChangeDirectory();

            // v 1.0
            File.WriteAllText("test.txt", "hab");
            ExecuteCommand("add test.txt");
            ExecuteCommand("commit -m blah");

            // v 2.0
            File.WriteAllText("test2.txt", "hab");
            ExecuteCommand("add test2.txt");
            ExecuteCommand("commit -m blah");

            // v 3.0
            File.WriteAllText("test.txt", "hab123");
            ExecuteCommand("commit -m blah");

            int previousVersion = _provider.GetLatestVersion() -1 ;

            ExecuteCommand("update");

            ExecuteCommand("update test.txt --revision " + previousVersion);

            Assert.AreEqual("hab", File.ReadAllText("test.txt"));
        }
    }
}