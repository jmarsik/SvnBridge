using NUnit.Framework;

namespace TestsEndToEnd
{
    [TestFixture]
    public class LogTest : EndToEndTestBase
    {
        [Test]
        public void CanAskForLogOfItemThatDoesNotExists()
        {
            string command = ExecuteCommandAndGetError("log " + testUrl + " --revision 1");
            Assert.AreEqual("svn: Unable to find repository location for 'http://localhost:9090/SvnBridgeTesting" +
                            testPath
                            + "' in revision 1\r\n",
                            command);
        }

        [Test]
        public void CanGetLogByUrl()
        {
            int revision = CreateFolder(testPath + "/Test4", true);
            string command = ExecuteCommand("log " + testUrl + " --revision " + revision);
            Assert.IsTrue(
                command.Contains("r" + revision), "does not contains revision"
                );
            Assert.IsTrue(
                command.Contains("A /SvnBridgeTesting" + testPath + "/Test4"),
                "does not contains created folder"
                );
        }

        [Test]
        public void CanGetLogOfAllChanges_WithoutLimit()
        {
            CheckoutAndChangeDirectory();

            string actual = ExecuteCommand("log");
            // we want to verify that we can execute it, not verify the contet
            Assert.IsFalse(string.IsNullOrEmpty(actual));
        }
    }
}