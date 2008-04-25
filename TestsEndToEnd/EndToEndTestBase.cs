using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using IntegrationTests;
using SvnBridge;
using SvnBridge.Infrastructure;
using SvnBridge.Interfaces;
using SvnBridge.Net;

namespace TestsEndToEnd
{
	public abstract class EndToEndTestBase : TFSSourceControlProviderTestsBase
	{
		#region Setup/Teardown
        private string originalCurrentDirectory;

		protected EndToEndTestBase()
		{
			authenticateAsLowPrivilegeUser = new AuthenticateAsLowPrivilegeUser();
			port = new Random().Next(1024, short.MaxValue);
            originalCurrentDirectory = Environment.CurrentDirectory;
        }

		public string TestUrl
		{
			get { return testUrl; }
			set { testUrl = value; }
		}

		public virtual void Initialize(string url, IPathParser parser)
		{
			initialized = true;
			testUrl = url;

			new BootStrapper().Start();

			CreateTempFolder();

			Environment.CurrentDirectory = Path.Combine(Path.GetTempPath(), checkoutFolder);
			Debug.WriteLine("cd " + checkoutFolder);
			listener = IoC.Resolve<IListener>();
			listener.ListenError += delegate(object sender, ListenErrorEventArgs e) { Debug.WriteLine(e.Exception); };
			listener.Port = this.port;

			listener.Start(parser);
		}

		private void CreateTempFolder()
		{
			checkoutFolder = Path.GetTempFileName();
			File.Delete(checkoutFolder);
			Directory.CreateDirectory(checkoutFolder);
			Debug.WriteLine("md " + checkoutFolder);
		}

		public override void Dispose()
		{
            Environment.CurrentDirectory = originalCurrentDirectory;

			if (initialized==false)
				return;
			
			listener.Stop();

			base.Dispose();
			ForAllFilesInCurrentDirectory(
				delegate(FileInfo file)
				{
					try
					{
						file.Attributes = file.Attributes & ~FileAttributes.ReadOnly;
					}
					catch (Exception)
					{
						// nothing much to do here
					}
				});

			authenticateAsLowPrivilegeUser.Dispose();
		}

		#endregion

		protected IListener listener;
		private string checkoutFolder;
		protected string testUrl;
		protected int port;
		private AuthenticateAsLowPrivilegeUser authenticateAsLowPrivilegeUser;
		private bool initialized;

		protected static void ForAllFilesInCurrentDirectory(Action<FileInfo> action)
		{
			ForAllFilesIn(Environment.CurrentDirectory, action);
		}

		protected static void ForAllFilesIn(string directory,
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
			Debug.WriteLine("cd " + checkoutFolder);
			Svn("co " + testUrl);
			Environment.CurrentDirectory =
				Path.Combine(Environment.CurrentDirectory, testPath.Substring(1) /* remove '/' */);
			Debug.WriteLine("cd " + Environment.CurrentDirectory);
		}

		protected static string SvnExpectError(string command)
		{
			string err = null;
			ExecuteInternal(command, delegate(Process svn)
			{
				err = svn.StandardError.ReadToEnd();
			});
			Debug.WriteLine(err);
			return err;
		}

		protected string Svn(string command)
		{
			StringBuilder output = new StringBuilder();
			string err = null;
			ExecuteInternal(command, delegate(Process svn)
			{
				ThreadPool.QueueUserWorkItem(delegate
				{
					err = svn.StandardError.ReadToEnd();
				});
				ThreadPool.QueueUserWorkItem(delegate
				{
					string line;
					while ((line = svn.StandardOutput.ReadLine()) != null)
					{
						Debug.WriteLine(line);
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

		protected XmlDocument SvnXml(string command)
		{
			StringBuilder output = new StringBuilder();
			string err = null;
			ExecuteInternal(command, delegate(Process svn)
			{
				ThreadPool.QueueUserWorkItem(delegate
				{
					err = svn.StandardError.ReadToEnd();
				});
				ThreadPool.QueueUserWorkItem(delegate
				{
					string line;
					while ((line = svn.StandardOutput.ReadLine()) != null)
					{
                        Debug.WriteLine(line);
						output.AppendLine(line);
					}
				});
			});
			if (string.IsNullOrEmpty(err) == false)
			{
				throw new InvalidOperationException("Failed to execute command: " + err);
			}
			if (command.StartsWith("commit"))
			{
				// we need to recreate the work space, because
				// a commit will kill all existing workspaces
				_provider.MakeActivity(_activityId);
			}
			XmlDocument document = new XmlDocument();
			document.LoadXml(output.ToString());
			return document;
		}

		private static void ExecuteInternal(string command, Action<Process> process)
		{
            Debug.WriteLine("svn " + command);
			ProcessStartInfo psi = new ProcessStartInfo("svn", command);
			psi.RedirectStandardOutput = true;
			psi.RedirectStandardError = true;
			psi.CreateNoWindow = true;
			psi.UseShellExecute = false;
			Process svn = Process.Start(psi);
			process(svn);
			svn.WaitForExit();
		}

		public int Port
		{
			get { return port; }
		}

		public string TestPath
		{
			get { return testPath; }
		}
	}
}
