using System.IO;
using NUnit.Framework;

namespace TestsEndToEnd
{
    [TestFixture]
    public class CommitTest : EndToEndTestBase
    {
        [Test]
        public void CanCommitNewFile()
        {
            CheckoutAndChangeDirectory();
            File.WriteAllText("test.txt", "hab");
            Svn("add test.txt");
            string command = Svn("commit -m blah");
            Assert.IsTrue(
                command.Contains("Committed")
                );
        }
    }
}
