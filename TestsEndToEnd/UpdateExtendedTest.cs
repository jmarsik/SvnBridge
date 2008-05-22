using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Xunit;
using System.Net;
using SvnBridge.SourceControl;
using CodePlex.TfsLibrary.ObjectModel;
using SvnBridge.Interfaces;
using CodePlex.TfsLibrary.RegistrationWebSvc;
using CodePlex.TfsLibrary.Utility;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.Infrastructure;
using SvnBridge.NullImpl;
using Rhino.Mocks;
using System.Diagnostics;

namespace TestsEndToEnd
{
    public class UpdateExtendedTest : EndToEndTestBase
    {
        //[SvnBridgeFact]
        public void TestUpdateAllRevisions()
        {
            string path = Path.Combine(Environment.CurrentDirectory, testPath.Substring(1));
            string path1 = Path.Combine(path, "path1");
            string path2 = Path.Combine(path, "path2");

            ServerUrl = "https://tfs03.codeplex.com";
            TFSSourceControlProvider provider = new TFSSourceControlProvider("https://tfs03.codeplex.com",
                                                     "SvnBridge",
                                                     null,
                                                     CreateSourceControlServicesHub2());

            LogItem log = provider.GetLog("", 1, provider.GetLatestVersion(), Recursion.Full, 200);
            Directory.CreateDirectory(path1);
            string testUrl = "http://" + IPAddress.Loopback + ":" + this.Port + "/tfs03.codeplex.com/SvnBridge";
            Environment.CurrentDirectory = path;
            Svn("co " + testUrl + " path1");

            foreach (SourceItemHistory changeset in log.History)
            {
                Environment.CurrentDirectory = path;
                Directory.CreateDirectory(path2);

                System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("") + ":svn co " + testUrl + " path2 -r " + changeset.ChangeSetID);
                Svn("co " + testUrl + " path2 -r " + changeset.ChangeSetID);
                System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString() + ":svn up path2");
                Svn("up path2");

                System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString() + ":Compare directories");
                bool result = CompareDirectory(path1, path2);
                Environment.CurrentDirectory = @"c:\";

                System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString() + ":Remove directory");
                string command = "/c rd \"" + path2 + "\" /s /q";
                ProcessStartInfo psi = new ProcessStartInfo("cmd", command);
                psi.CreateNoWindow = true;
                psi.UseShellExecute = false;
                Process svn = Process.Start(psi);
                svn.WaitForExit();
                Assert.True(result);
            }
        }

        public ISourceControlServicesHub CreateSourceControlServicesHub2()
        {
            RegistrationWebSvcFactory factory = new RegistrationWebSvcFactory();
            FileSystem system = new FileSystem();

            RegistrationService service = new RegistrationService(factory);
            RepositoryWebSvcFactory factory1 = new RepositoryWebSvcFactory(factory);
            WebTransferService webTransferService = new WebTransferService(system);

            TFSSourceControlService tfsSourceControlService = new TFSSourceControlService(service,
                                                                                          factory1,
                                                                                          webTransferService,
                                                                                          system,
                                                                                          new NullLogger());
            MetaDataRepositoryFactory metaDataRepositoryFactory = new MetaDataRepositoryFactory(tfsSourceControlService, IoC.Resolve<IPersistentCache>(), false);
            ProjectInformationRepository repository = new ProjectInformationRepository(
                                                                                       metaDataRepositoryFactory,
                                                                                       ServerUrl);
            ICredentials credentials = null;
            IFileCache fileCache = MockRepository.GenerateStub<IFileCache>();
            ILogger logger = new NullLogger();
            FileRepository fileRepository = new FileRepository(ServerUrl, credentials, fileCache, webTransferService, logger, false);
            return new SourceControlServicesHub(
                credentials,
                webTransferService,
                tfsSourceControlService,
                repository,
                associateWorkItemWithChangeSet,
                logger,
                new NullCache(),
                fileCache,
                metaDataRepositoryFactory,
                fileRepository);
        }

        public bool CompareDirectory(string path1, string path2)
        {
            if (path1.Contains(@"\.svn\") || path1.EndsWith(@"\.svn"))
                return true;

            if (Directory.GetDirectories(path1).Length != Directory.GetDirectories(path2).Length)
            {
                System.Diagnostics.Debug.WriteLine("Directory count doesn't match: " + path1);
                return false;
            }

            if (Directory.GetFiles(path1).Length != Directory.GetFiles(path2).Length)
            {
                System.Diagnostics.Debug.WriteLine("File count doesn't match: " + path1);
                return false;
            }

            foreach (string directory1 in Directory.GetDirectories(path1))
            {
                string directory2 = path2 + directory1.Substring(path1.Length);
                bool match = CompareDirectory(directory1, directory2);
                if (!match)
                    return false;
            }

            foreach (string file1 in Directory.GetFiles(path1))
            {
                string file2 = path2 + file1.Substring(path1.Length);
                using (FileStream stream1 = File.OpenRead(file1))
                using (FileStream stream2 = File.OpenRead(file2))
                {
                    if (stream1.Length != stream2.Length)
                    {
                        System.Diagnostics.Debug.WriteLine("File length doesn't match: " + file1);
                        return false;
                    }

                    int data1;
                    int data2;
                    do
                    {
                        data1 = stream1.ReadByte();
                        data2 = stream2.ReadByte();
                        if (data1 != data2)
                        {
                            System.Diagnostics.Debug.WriteLine("File data doesn't match: " + file1);
                            return false;
                        }

                    } while (data1 != -1);
                }
            }
            return true;
        }
    }
}
