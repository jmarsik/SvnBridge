using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using IntegrationTests;
using SvnBridge;
using SvnBridge.Infrastructure;
using SvnBridge.Net;
using Tests;
using Xunit;

namespace TestsEndToEnd
{
    public class EndToEndTestBase : TFSSourceControlProviderTestsBase, IDisposable
    {
        #region Setup/Teardown

        public EndToEndTestBase()
        {
            base.SetUp();
            authenticateAsLowPrivilegeUser = new AuthenticateAsLowPrivilegeUser();
            port = new Random().Next(1024, short.MaxValue);
            testUrl = "http://localhost:" + this.port + "/SvnBridgeTesting" + testPath;

            new BootStrapper().Start();

            CreateTempFolder();

            Environment.CurrentDirectory = Path.Combine(Path.GetTempPath(), checkoutFolder);
            Console.WriteLine("cd " + checkoutFolder);
            listener = IoC.Resolve<IListener>();
            listener.ListenError += delegate(object sender, ListenErrorEventArgs e) { Console.WriteLine(e.Exception); };
            listener.TfsUrl = ServerUrl;
            listener.Port = this.port;
            listener.Start();
        }

        private void CreateTempFolder()
        {
            checkoutFolder = Path.GetTempFileName();
            File.Delete(checkoutFolder);
            Directory.CreateDirectory(checkoutFolder);
            Console.WriteLine("md " + checkoutFolder);
        }

        public virtual void Dispose()
        {
            listener.Stop();

            base.TearDown();
            ForAllFilesInCurrentDirectory(
                delegate(FileInfo file) { file.Attributes = file.Attributes & ~FileAttributes.ReadOnly; });

            Environment.CurrentDirectory = Path.GetPathRoot(Environment.CurrentDirectory);

            //Directory.Delete(checkoutFolder, true);

            authenticateAsLowPrivilegeUser.Dispose();
        }

        #endregion

        protected IListener listener;
        private string checkoutFolder;
        protected string testUrl;
        protected int port;
        private AuthenticateAsLowPrivilegeUser authenticateAsLowPrivilegeUser;

        private static void ForAllFilesInCurrentDirectory(Action<FileInfo> action)
        {
            ForAllFilesIn(Environment.CurrentDirectory, action);
        }

        private static void ForAllFilesIn(string directory,
                                          Action<FileInfo> action)
        {
            foreach (string file in Directory.GetFiles(directory))
            {
                action(new FileInfo(file));
            }
            foreach (string dir in Directory.GetDirectories(directory))
            {
                ForAllFilesIn(dir, action);
            }
        }

        protected void CheckoutAndChangeDirectory()
        {
            Svn("co " + testUrl);
            Environment.CurrentDirectory =
                Path.Combine(Environment.CurrentDirectory, testPath.Substring(1) /* remove '/' */);
        }


        protected void CheckoutAgainAndChangeDirectory()
        {
            CreateTempFolder();
            Environment.CurrentDirectory = checkoutFolder;
            Console.WriteLine("cd " +checkoutFolder);
            Svn("co " + testUrl);
            Environment.CurrentDirectory = 
                Path.Combine(Environment.CurrentDirectory, testPath.Substring(1) /* remove '/' */);
            Console.WriteLine("cd " + Environment.CurrentDirectory);
        }

        protected static string ExecuteCommandAndGetError(string command)
        {
            string err = null;
            ExecuteInternal(command,delegate(Process svn)
            {
                err = svn.StandardError.ReadToEnd();
            });
            return err;
        }

        protected static string Svn(string command)
        {
            StringBuilder output = new StringBuilder();
            string err = null;
            ExecuteInternal(command,delegate(Process svn)
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    err = svn.StandardError.ReadToEnd();
                });
                ThreadPool.QueueUserWorkItem(delegate
                {
                    string line;
                    while((line = svn.StandardOutput.ReadLine())!=null)
                    {
                        Console.WriteLine(line);
                        output.AppendLine(line);
                    }
                });
            });
            if (string.IsNullOrEmpty(err) == false)
            {
                throw new InvalidOperationException("Failed to execute command: " + err);
            }
            return output.ToString();
        }

        private static void ExecuteInternal(string command, Action<Process> process)
        {
            Console.WriteLine("svn " + command);
            ProcessStartInfo psi = new ProcessStartInfo("svn", command);
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process svn = Process.Start(psi);
            process(svn);
            svn.WaitForExit();
        }
    }
}