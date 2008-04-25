using Xunit;

namespace TestsEndToEnd
{
    public class BlameTest : EndToEndTestBase
    {
        [SvnBridgeFact]
        public void CannotBlameOnFolder()
        {
            CreateFolder(testPath + "/TestFolder1", true);
            CheckoutAndChangeDirectory();
            string error = SvnExpectError("blame TestFolder1");
            Assert.Contains("TestFolder1' is not a file", error);
        }

        [SvnBridgeFact]
        public void CannotBlameOnNonExistingFile()
        {
            CreateFolder(testPath + "/TestFolder1", true);
            CheckoutAndChangeDirectory();
            string error = SvnExpectError("blame " + testUrl + "/not_here");
            Assert.Contains("not_here' path not found", error);
        }
    }
}