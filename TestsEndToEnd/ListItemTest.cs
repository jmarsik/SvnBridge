using System.Diagnostics;
using NUnit.Framework;
using SvnBridge;
using SvnBridge.Net;
using Tests;

namespace TestsEndToEnd
{
    [TestFixture]
    public class ListItemTest : TFSSourceControlProviderTestsBase
    {
        private IListener listener;

        public override void SetUp()
        {
            base.SetUp();

            new BootStrapper().Start();

            listener = ListenerFactory.Create();
            this.listener.TfsUrl = "http://codeplex-tfs3:8080";
            this.listener.Port = 9090;
            this.listener.Start();
        }

        public override void TearDown()
        {
            base.TearDown();
            listener.Stop();
        }

        [Test]
        public void CanListFiles()
        {
            CreateFolder(_testPath + "/TestFolder", true);
            string actual = ExecuteCommand("list http://localhost:9090/SvnBridgeTesting" + _testPath);
            Assert.AreEqual("TestFolder/\r\n", actual);

        }

        private static string ExecuteCommand(string command)
        {
            ProcessStartInfo psi = new ProcessStartInfo("svn", command);
            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process svn = Process.Start(psi);
            svn.WaitForExit(1000);
            return svn.StandardOutput.ReadToEnd();
        }
    }
}
