using System;
using System.Text;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RegistrationWebSvc;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using CodePlex.TfsLibrary.Utility;
using IntegrationTests.Properties;
using NUnit.Framework;
using SvnBridge.Cache;
using SvnBridge.SourceControl;

namespace Tests
{
    [TestFixture]
    public abstract class TFSSourceControlProviderTestsBase
    {
        #region Setup/Teardown

        [SetUp]
        public virtual void SetUp()
        {
            _activityId = Guid.NewGuid().ToString();
            RegistrationWebSvcFactory factory = new RegistrationWebSvcFactory();
            FileSystem system = new FileSystem();
            RegistrationService service = new RegistrationService(factory);
            RepositoryWebSvcFactory factory1 = new RepositoryWebSvcFactory(factory);
            WebTransferService webTransferService = new WebTransferService(system);
            TFSSourceControlService tfsSourceControlService = new TFSSourceControlService(service,
                                                                                          factory1,
                                                                                          webTransferService,
                                                                                          system);
            _provider = new TFSSourceControlProvider(ServerUrl,
                                                     PROJECT_NAME,
                                                     null,
                                                     webTransferService,
                                                     tfsSourceControlService,
                                                     new ProjectInformationRepository(new NullCache(),
                                                                                      tfsSourceControlService,
                                                                                      ServerUrl));
            testPath = "/Test" + DateTime.Now.ToString("yyyyMMddHHmmss");
            _provider.MakeActivity(_activityId);
            _provider.MakeCollection(_activityId, testPath);
            Commit();
        }

        [TearDown]
        public virtual void TearDown()
        {
            Commit();
            DeleteItem(testPath, false);
            _provider.MergeActivity(_activityId);
            _provider.DeleteActivity(_activityId);
        }

        #endregion

        protected static string ServerUrl = Settings.Default.ServerUrl;
        protected const string PROJECT_NAME = "SvnBridgeTesting";
        protected string _activityId;
        protected string testPath;
        protected TFSSourceControlProvider _provider;
        protected int _lastCommitRevision;

        protected void UpdateFile(string path,
                                  string fileData,
                                  bool commit)
        {
            byte[] data = Encoding.Default.GetBytes(fileData);
            _provider.WriteFile(_activityId, path, data);
            if (commit)
            {
                Commit();
            }
        }

        protected bool WriteFile(string path,
                                 string fileData,
                                 bool commit)
        {
            byte[] data = Encoding.Default.GetBytes(fileData);
            return WriteFile(path, data, commit);
        }

        protected bool WriteFile(string path,
                                 byte[] fileData,
                                 bool commit)
        {
            bool created = _provider.WriteFile(_activityId, path, fileData);
            if (commit)
            {
                Commit();
            }
            return created;
        }

        protected MergeActivityResponse Commit()
        {
            MergeActivityResponse response = _provider.MergeActivity(_activityId);
            _lastCommitRevision = response.Version;
            _provider.DeleteActivity(_activityId);
            _provider.MakeActivity(_activityId);
            return response;
        }

        protected void DeleteItem(string path,
                                  bool commit)
        {
            _provider.DeleteItem(_activityId, path);
            if (commit)
            {
                Commit();
            }
        }

        protected void CopyItem(string path,
                                string newPath,
                                bool commit)
        {
            _provider.CopyItem(_activityId, path, newPath);
            if (commit)
            {
                Commit();
            }
        }

        protected void RenameItem(string path,
                                  string newPath,
                                  bool commit)
        {
            MoveItem(path, newPath, commit);
        }

        protected void MoveItem(string path,
                                string newPath,
                                bool commit)
        {
            DeleteItem(path, false);
            CopyItem(path, newPath, false);
            if (commit)
            {
                Commit();
            }
        }

        protected int CreateFolder(string path,
                                   bool commit)
        {
            _provider.MakeCollection(_activityId, path);
            if (commit)
            {
                return Commit().Version;
            }
            return -1;
        }

        protected string ReadFile(string path)
        {
            ItemMetaData item = _provider.GetItems(-1, path, Recursion.None);
            return GetString(_provider.ReadFile(item));
        }

        protected void SetProperty(string path,
                                   string name,
                                   string value,
                                   bool commit)
        {
            _provider.SetProperty(_activityId, path, name, value);
            if (commit)
            {
                Commit();
            }
        }

        protected string GetString(byte[] data)
        {
            return Encoding.Default.GetString(data);
        }

        protected byte[] GetBytes(string data)
        {
            return Encoding.Default.GetBytes(data);
        }

        protected bool ResponseContains(MergeActivityResponse response,
                                        string path,
                                        ItemType itemType)
        {
            bool found = false;
            foreach (MergeActivityResponseItem item in response.Items)
            {
                if ((item.Path == path) && (item.Type == itemType))
                {
                    found = true;
                }
            }

            return found;
        }
    }
}