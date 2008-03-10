using System.IO;
using NUnit.Framework;

namespace TestsEndToEnd
{
    /// <summary>
    /// This set of tests are here for scenarios that were found by
    /// users
    /// </summary>
    [TestFixture]
    public class IntegrationsTests : EndToEndTestBase
    {
        /// <summary>
        /// http://www.codeplex.com/SvnBridge/WorkItem/View.aspx?WorkItemId=9315
        /// </summary>
        [Test]
        public void FailToAddFileOnUpdateAfterAdd()
        {
            CheckoutAndChangeDirectory();
            File.WriteAllText("test.txt", "blah");
            Svn("add test.txt");
            Svn("commit -m test");
            Svn("update");
        }

        [Test]
        public void CommitRenamesOfFiles()
        {
            CheckoutAndChangeDirectory();
            for (int i = 0; i < 25; i++)
            {
                File.WriteAllText("test." + i, i.ToString());
            }
            Svn("add test.*");
            Svn("commit -m blah");

            for (int i = 0; i < 25; i+=2)
            {
                Svn("rename test." + i + " ren." + i);
            }

            Svn("commit -m ren");
        }
    }
}
