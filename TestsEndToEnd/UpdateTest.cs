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
            TemporaryIgnore("We do not support versioning backward at the moment");
            CheckoutAndChangeDirectory();
            File.WriteAllText("test.txt", "hab");
            ExecuteCommand("commit -m blah");
            ExecuteCommand("update --revision PREV");
        }
    }
}