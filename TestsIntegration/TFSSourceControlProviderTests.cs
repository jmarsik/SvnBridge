using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RegistrationWebSvc;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using CodePlex.TfsLibrary.Utility;
using NUnit.Framework;
using SvnBridge.Cache;
using SvnBridge.Exceptions;
using SvnBridge.SourceControl;
using SvnBridge.Infrastructure;

namespace Tests
{
    [TestFixture]
    public class TFSSourceControlProviderTests : TFSSourceControlProviderTestsBase
    {
        [Test, ExpectedException(typeof (FolderAlreadyExistsException))]
        public void TestAddFolderThatAlreadyExistsThrowsException()
        {
            CreateFolder(testPath + "/New Folder", true);

            _provider.MakeCollection(_activityId, testPath + "/New Folder");
        }

        [Test]
        public void TestCreateProviderWithMultipleTFSUrlsSucceeds()
        {
            RegistrationWebSvcFactory factory = new RegistrationWebSvcFactory();
            FileSystem system = new FileSystem();
            RegistrationService service = new RegistrationService(factory);
            RepositoryWebSvcFactory factory1 = new RepositoryWebSvcFactory(factory);
            WebTransferService webTransferService = new WebTransferService(system);
            TFSSourceControlService tfsSourceControlService = new TFSSourceControlService(service,
                                                                                          factory1,
                                                                                          webTransferService,
                                                                                          system);
            TFSSourceControlProvider provider = new TFSSourceControlProvider(ServerUrl + "," + ServerUrl,
                                                                             PROJECT_NAME,
                                                                             null,
                                                                             webTransferService,
                                                                             tfsSourceControlService,
                                                                             new ProjectInformationRepository(
                                                                                 new NullCache(),
                                                                                 tfsSourceControlService,
                                                                                 ServerUrl),
                                                                                 new AssociateWorkItemWithChangeSet(ServerUrl, null));
        }

        [Test]
        public void TestDeleteItemReturnsFalseIfFileDoesNotExist()
        {
            bool result = _provider.DeleteItem(_activityId, testPath + "/NotHere.txt");

            Assert.IsFalse(result);
        }

        [Test]
        public void TestDeleteItemReturnsTrueWhenFileExists()
        {
            WriteFile(testPath + "/File.txt", "filedata", true);

            bool result = _provider.DeleteItem(_activityId, testPath + "/File.txt");

            Assert.IsTrue(result);
        }

        [Test]
        public void TestItemExistsReturnsFalseIfFileDoesNotExist()
        {
            bool result = _provider.ItemExists(testPath + "/TestFile.txt");

            Assert.IsFalse(result);
        }

        [Test]
        public void TestItemExistsReturnsFalseIfFileDoesNotExistInSpecifiedVersion()
        {
            int version = _lastCommitRevision;
            WriteFile(testPath + "/TestFile.txt", "Fun text", true);

            bool result = _provider.ItemExists(testPath + "/TestFile.txt", version);

            Assert.IsFalse(result);
        }

        [Test]
        public void TestItemExistsReturnsTrueIfFileExists()
        {
            WriteFile(testPath + "/TestFile.txt", "Fun text", true);

            bool result = _provider.ItemExists(testPath + "/TestFile.txt");

            Assert.IsTrue(result);
        }
    }
}