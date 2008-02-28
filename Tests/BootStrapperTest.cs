using System.Collections;
using System.Net;
using NUnit.Framework;
using Rhino.Mocks;
using SvnBridge.Infrastructure;
using SvnBridge.Interfaces;
using SvnBridge.SourceControl;

namespace SvnBridge
{
    [TestFixture]
    public class BootStrapperTest
    {
        #region Setup/Teardown

        [SetUp]
        public void TestInitialize()
        {
            mocks = new MockRepository();
        }

        [TearDown]
        public void TestCleanup()
        {
            mocks.VerifyAll();
        }

        #endregion

        private MockRepository mocks;

        [Test]
        public void AfterStartingBootStrapper_CanResolveCache_FromContainer()
        {
            new BootStrapper().Start();
            Assert.IsNotNull(IoC.Resolve<ICache>());
        }

        [Test]
        public void AfterStartingBootStrapper_CanResoveCachingItemMetaDataRepository()
        {
            new BootStrapper().Start();
            Hashtable dependencies = new Hashtable();
            dependencies["serverUrl"] = "http://codeplex-tfs3:8080/";
            dependencies["projectName"] = "as";
            dependencies["credentials"] = CredentialCache.DefaultCredentials;

            IProjectInformationRepository stub = mocks.Stub<IProjectInformationRepository>();
            SetupResult.For(stub.GetProjectLocation(null, null)).IgnoreArguments().Return(
                new ProjectLocationInformation("test", "http://codeplex-tfs3:8080/"));

            mocks.ReplayAll();

            dependencies["projectInformationRepository"] = stub;
            Assert.IsNotNull(IoC.Resolve<IItemMetaDataRepository>(dependencies));
        }
    }
}