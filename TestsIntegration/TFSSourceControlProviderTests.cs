using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RegistrationWebSvc;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using CodePlex.TfsLibrary.Utility;
using SvnBridge.Cache;
using SvnBridge.Exceptions;
using SvnBridge.SourceControl;
using SvnBridge.Infrastructure;

namespace Tests
{
	using IntegrationTests;
	using Xunit;

	public class TFSSourceControlProviderTests : TFSSourceControlProviderTestsBase
    {
        [Fact]
        public void TestAddFolderThatAlreadyExistsThrowsException()
        {
            CreateFolder(testPath + "/New Folder", true);

        	Assert.Throws(typeof (FolderAlreadyExistsException), delegate
        	{
        		_provider.MakeCollection(_activityId, testPath + "/New Folder");
        	});
        }

        [Fact]
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
                                                                                          system, new NullCache());
            TFSSourceControlProvider provider = new TFSSourceControlProvider(ServerUrl + "," + ServerUrl,
                                                                             PROJECT_NAME,
                                                                             CreateSourceControlServicesHub());
        }

        [Fact]
        public void TestDeleteItemReturnsFalseIfFileDoesNotExist()
        {
            bool result = _provider.DeleteItem(_activityId, testPath + "/NotHere.txt");

            Assert.False(result);
        }

        [Fact]
        public void TestDeleteItemReturnsTrueWhenFileExists()
        {
            WriteFile(testPath + "/File.txt", "filedata", true);

            bool result = _provider.DeleteItem(_activityId, testPath + "/File.txt");

            Assert.True(result);
        }

        [Fact]
        public void TestItemExistsReturnsFalseIfFileDoesNotExist()
        {
            bool result = _provider.ItemExists(testPath + "/TestFile.txt");

            Assert.False(result);
        }

        [Fact]
        public void TestItemExistsReturnsFalseIfFileDoesNotExistInSpecifiedVersion()
        {
            int version = _lastCommitRevision;
            WriteFile(testPath + "/TestFile.txt", "Fun text", true);

            bool result = _provider.ItemExists(testPath + "/TestFile.txt", version);

            Assert.False(result);
        }

        [Fact]
        public void TestItemExistsReturnsTrueIfFileExists()
        {
            WriteFile(testPath + "/TestFile.txt", "Fun text", true);

            bool result = _provider.ItemExists(testPath + "/TestFile.txt");

            Assert.True(result);
        }
    }
}
