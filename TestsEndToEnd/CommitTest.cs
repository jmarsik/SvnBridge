using System.IO;
using Xunit;

namespace TestsEndToEnd
{
    public class CommitTest : EndToEndTestBase
    {
        [Fact]
        public void CanCommitNewFile()
        {
            CheckoutAndChangeDirectory();
            File.WriteAllText("test.txt", "hab");
            Svn("add test.txt");
            string command = Svn("commit -m blah");
            Assert.True(
                command.Contains("Committed")
                );
        }
    }
}
