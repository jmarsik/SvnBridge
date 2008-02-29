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
    }
}
