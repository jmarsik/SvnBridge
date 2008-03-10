using System.IO;
using NUnit.Framework;
using SvnBridge.Net;

namespace TestsEndToEnd
{
    [TestFixture]
    [Ignore("Takes too long to run")]
    public class LoadTest : EndToEndTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            listener.FinishedHandling+=delegate(object sender, FinishedHandlingEventArgs e)
            {
                System.Console.WriteLine(e.Method + " " + e.Url + " in " + e.Duration);
            };
        }

        [Test]
        public void Commit1000Files()
        {
            CheckoutAndChangeDirectory();
            for (int i = 0; i < 1000; i++)
            {
                File.WriteAllText("file."+i.ToString("0000"), i.ToString());
            }
            Svn("add file.*");
            Svn("commit -m \"Big Commit\" ");
        }
    }
}