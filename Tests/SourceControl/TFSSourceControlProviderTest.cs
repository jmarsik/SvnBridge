using CodePlex.TfsLibrary.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using SvnBridge.Cache;
using SvnBridge.Infrastructure;
using SvnBridge.Interfaces;

namespace SvnBridge.SourceControl
{
    [TestFixture]
    public class TFSSourceControlProviderTest
    {
        private MockRepository mocks;
        private IAssociateWorkItemWithChangeSet associateWorkItemWithChangeSet;
        private TFSSourceControlProvider provider;

        [SetUp]
        public void TestInitialize()
        {
            mocks = new MockRepository();
            associateWorkItemWithChangeSet = mocks.CreateMock<IAssociateWorkItemWithChangeSet>();
            provider = new TFSSourceControlProvider(
                "blah",
                null,
                CreateSourceControlServicesHub());
        }

        public ISourceControlServicesHub CreateSourceControlServicesHub()
        {
            return new SourceControlServicesHub(
                System.Net.CredentialCache.DefaultCredentials,
                MockRepository.GenerateStub<IWebTransferService>(),
                MockRepository.GenerateStub<ITFSSourceControlService>(),
                MockRepository.GenerateStub<IProjectInformationRepository>(),
                associateWorkItemWithChangeSet,
                new FileLogger(),
                new NullCache(),
                MockRepository.GenerateStub<IFileCache>());
        }


        [TearDown]
        public void TestCleanup()
        {
            mocks.VerifyAll();
        }

        [Test]
        public void WillNotAssociateIfCommentHasNoWorkItems()
        {
            mocks.ReplayAll();
            provider.AssociateWorkItemsWithChangeSet("blah blah", 15);
        }

        [Test]
        public void WillExtractWorkItemsFromCheckInCommentsAndAssociateWithChangeSet()
        {
            associateWorkItemWithChangeSet.Associate(15,15);
            associateWorkItemWithChangeSet.SetWorkItemFixed(15);
            mocks.ReplayAll();
            string comment = @"blah blah
Work Item: 15";
            provider.AssociateWorkItemsWithChangeSet(comment, 15);
        }

        [Test]
        public void CanAssociateMoreThanOneId()
        {
            associateWorkItemWithChangeSet.Associate(15, 15);
            associateWorkItemWithChangeSet.SetWorkItemFixed(15);
            associateWorkItemWithChangeSet.Associate(16, 15);
            associateWorkItemWithChangeSet.SetWorkItemFixed(16);
            associateWorkItemWithChangeSet.Associate(17, 15);
            associateWorkItemWithChangeSet.SetWorkItemFixed(17);
            mocks.ReplayAll();
            string comment = @"blah blah
Work Items: 15, 16, 17";
            provider.AssociateWorkItemsWithChangeSet(comment, 15);
        }

        [Test]
        public void CanAssociateOnMultiplyLines()
        {
            associateWorkItemWithChangeSet.Associate(15, 15);
            associateWorkItemWithChangeSet.SetWorkItemFixed(15);
            associateWorkItemWithChangeSet.Associate(16, 15);
            associateWorkItemWithChangeSet.SetWorkItemFixed(16);
            associateWorkItemWithChangeSet.Associate(17, 15);
            associateWorkItemWithChangeSet.SetWorkItemFixed(17);
            mocks.ReplayAll();
            string comment = @"blah blah
Work Items: 15, 16
Work Item: 17";
            provider.AssociateWorkItemsWithChangeSet(comment, 15); 
        }

        [Test]
        public void WillRecognizeWorkItemsIfWorkItemAppearsPreviouslyInText()
        {

            associateWorkItemWithChangeSet.Associate(15, 15);
            associateWorkItemWithChangeSet.SetWorkItemFixed(15);
            associateWorkItemWithChangeSet.Associate(16, 15);
            associateWorkItemWithChangeSet.SetWorkItemFixed(16);
            associateWorkItemWithChangeSet.Associate(17, 15);
            associateWorkItemWithChangeSet.SetWorkItemFixed(17);
            associateWorkItemWithChangeSet.Associate(81, 15);
            associateWorkItemWithChangeSet.SetWorkItemFixed(81);
            mocks.ReplayAll();
            string comment = @"Adding work items support and fixing
other issues with workitems
Solved Work Items: 15, 16
Fixed WorkItem: 17
Assoicate with workitem: 81";
            provider.AssociateWorkItemsWithChangeSet(comment, 15); 
        }
    }
}
