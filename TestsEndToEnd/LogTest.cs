using System;
using System.IO;
using NUnit.Framework;

namespace TestsEndToEnd
{
    [TestFixture]
    public class LogTest : EndToEndTestBase
    {
        [Test]
        public void CanGetLogOfAllChanges_WithoutLimit()
        {
            CheckoutAndChangeDirectory();

            string actual = ExecuteCommand("log");
            // we want to verify that we can execute it, not verify the contet
            Assert.IsFalse(string.IsNullOrEmpty(actual));
        }

        [Test]
        public void CanGetLogByUrl()
        {
            int revision = CreateFolder(testPath + "/Test4", true);
            string command = ExecuteCommand("log "+ testUrl + " --revision " + revision);
            Assert.IsTrue(
                command.Contains("r"+revision), "does not contains revision"
                );
            Assert.IsTrue(
                command.Contains("A /SvnBridgeTesting" + testPath + "/Test4"),
                "does not contains created folder"
                );
        }
    }
}