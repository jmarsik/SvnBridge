using System;
using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using SvnBridge.Interfaces;

namespace SvnBridge.SourceControl
{
    [TestFixture]
    public class ProjectInformationRepositoryTest
    {
        #region Setup/Teardown

        [SetUp]
        public void TestInitialize()
        {
            mocks = new MockRepository();
            sourceControlService = mocks.DynamicMock<ITFSSourceControlService>();
            cache = mocks.DynamicMock<ICache>();
        }

        [TearDown]
        public void TestCleanup()
        {
            mocks.VerifyAll();
        }

        #endregion

        private MockRepository mocks;
        private ITFSSourceControlService sourceControlService;
        private ICache cache;

        [Test]
        public void GetProjectInforation_WillQueryServerForProject()
        {
            string serverUrl = "http://codeplex-tfs3:8080";
            sourceControlService.QueryItems(null,
                                            null,
                                            null,
                                            RecursionType.None,
                                            null,
                                            DeletedState.Any,
                                            ItemType.Any);
            SourceItem sourceItem = new SourceItem();
            sourceItem.RemoteName = "$/test";
            LastCall.Constraints(Is.Equal(serverUrl),
                                 Is.Anything(),
                                 Is.Anything(),
                                 Is.Anything(),
                                 Is.Anything(),
                                 Is.Anything(),
                                 Is.Anything())
                .Return(new SourceItem[] {sourceItem,});


            IProjectInformationRepository repository =
                new ProjectInformationRepository(cache, sourceControlService, serverUrl);
            mocks.ReplayAll();

            ProjectLocationInformation location =
                repository.GetProjectLocation(CredentialCache.DefaultCredentials, "blah");
        }

        [Test]
        public void GetProjectInforation_WillReturnRemoteProjectName()
        {
            string serverUrl = "http://codeplex-tfs3:8080";
            sourceControlService.QueryItems(null,
                                            null,
                                            null,
                                            RecursionType.None,
                                            null,
                                            DeletedState.Any,
                                            ItemType.Any);
            SourceItem sourceItem = new SourceItem();
            sourceItem.RemoteName = "$/test";
            LastCall.Constraints(Is.Equal(serverUrl),
                                 Is.Anything(),
                                 Is.Anything(),
                                 Is.Anything(),
                                 Is.Anything(),
                                 Is.Anything(),
                                 Is.Anything())
                .Return(new SourceItem[] {sourceItem,});


            IProjectInformationRepository repository =
                new ProjectInformationRepository(cache, sourceControlService, serverUrl);
            mocks.ReplayAll();

            ProjectLocationInformation location =
                repository.GetProjectLocation(CredentialCache.DefaultCredentials, "blah");

            Assert.AreEqual("test", location.RemoteProjectName);
        }

        [Test]
        public void GetProjectInformation_WillQueryAllServers()
        {
            string multiServers = "http://codeplex-tfs3:8080,http://codeplex-tfs2:8080,http://codeplex-tfs1:8080";

            SourceItem sourceItem = new SourceItem();
            sourceItem.RemoteName = "$/test";

            sourceControlService.QueryItems(null,
                                            null,
                                            null,
                                            RecursionType.None,
                                            null,
                                            DeletedState.Any,
                                            ItemType.Any);
            LastCall.IgnoreArguments().Repeat.Twice().Return(new SourceItem[0]);

            sourceControlService.QueryItems(null,
                                            null,
                                            null,
                                            RecursionType.None,
                                            null,
                                            DeletedState.Any,
                                            ItemType.Any);

            LastCall.IgnoreArguments().Return(new SourceItem[] {sourceItem});

            mocks.ReplayAll();

            IProjectInformationRepository repository =
                new ProjectInformationRepository(cache, sourceControlService, multiServers);

            repository.GetProjectLocation(null, "as");
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException),
            ExpectedMessage = "Could not find project 'blah' in: http://not.used")]
        public void IfProjectNotFound_WillThrow()
        {
            mocks.ReplayAll();

            IProjectInformationRepository repository =
                new ProjectInformationRepository(cache, sourceControlService, "http://not.used");

            repository.GetProjectLocation(null, "blah");
        }

        [Test]
        public void WillGetFromCacheIfFound()
        {
            Expect.Call(cache.Get("GetProjectLocation-blah")).Return(new CachedResult(new ProjectLocationInformation("blah", "http")));

            mocks.ReplayAll();

            IProjectInformationRepository repository =
                new ProjectInformationRepository(cache, sourceControlService, "http://not.used");

            ProjectLocationInformation location = repository.GetProjectLocation(null, "blah");
            Assert.IsNotNull(location);
        }

        [Test]
        public void WillSetInCacheAfterFindingFromServer()
        {
            string serverUrl = "http://codeplex-tfs3:8080";
            sourceControlService.QueryItems(null,
                                            null,
                                            null,
                                            RecursionType.None,
                                            null,
                                            DeletedState.Any,
                                            ItemType.Any);
            SourceItem sourceItem = new SourceItem();
            sourceItem.RemoteName = "$/test";
            LastCall.Constraints(Is.Equal(serverUrl),
                                 Is.Anything(),
                                 Is.Anything(),
                                 Is.Anything(),
                                 Is.Anything(),
                                 Is.Anything(),
                                 Is.Anything())
                .Return(new SourceItem[] {sourceItem,});

            cache.Set(null, null);
            LastCall.IgnoreArguments();


            IProjectInformationRepository repository =
                new ProjectInformationRepository(cache, sourceControlService, serverUrl);
            mocks.ReplayAll();

            ProjectLocationInformation location =
                repository.GetProjectLocation(CredentialCache.DefaultCredentials, "blah");

            Assert.AreEqual("test", location.RemoteProjectName);
        }
    }
}