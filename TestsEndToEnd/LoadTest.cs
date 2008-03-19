using System.IO;
using Xunit;
using SvnBridge.Net;

namespace TestsEndToEnd
{
    public class LoadTest : EndToEndTestBase
    {
		public LoadTest()
		{
			listener.FinishedHandling += delegate(object sender, FinishedHandlingEventArgs e)
		   {
			   System.Console.WriteLine(e.Method + " " + e.Url + " in " + e.Duration);
		   };
		}

        [Fact(Skip="Takes too long to run")]
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