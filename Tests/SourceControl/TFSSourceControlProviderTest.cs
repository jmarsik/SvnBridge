using System;
using CodePlex.TfsLibrary.ObjectModel;
using Rhino.Mocks.Impl;
using SvnBridge.NullImpl;
using Xunit;
using Rhino.Mocks;
using SvnBridge.Infrastructure;
using SvnBridge.Interfaces;
using Tests;
using SvnBridge.Cache;
using Attach;

namespace SvnBridge.SourceControl
{
    public class TFSSourceControlProviderTest
    {
        private readonly MyMocks attach;
        private readonly MockRepository mocks;
        private readonly AssociateWorkItemWithChangeSet associateWorkItemWithChangeSet;
        private readonly TFSSourceControlProvider provider;

        public TFSSourceControlProviderTest()
        {
            attach = new MyMocks();
            mocks = new MockRepository();
            associateWorkItemWithChangeSet = attach.CreateObject<AssociateWorkItemWithChangeSet>("http://www.codeplex.com", null);
            provider = new TFSSourceControlProvider(
                "blah",
                null,
				null,
                CreateSourceControlServicesHub(),
                attach.CreateObject<OldSvnBridgeFilesSpecification>());
        }

        public SourceControlServicesHub CreateSourceControlServicesHub()
        {
            return new SourceControlServicesHub(
                System.Net.CredentialCache.DefaultCredentials,
                MockRepository.GenerateStub<ITFSSourceControlService>(),
                MockRepository.GenerateStub<IProjectInformationRepository>(),
                associateWorkItemWithChangeSet,
                attach.CreateObject<DefaultLogger>(),
                attach.CreateObject<WebCache>(),
                attach.CreateObject<FileCache>(null),
                attach.CreateObject<MetaDataRepositoryFactory>(null, null, false),
                attach.CreateObject<FileRepository>("http://www.codeplex.com", null, null, null, null, false));
        }

        [Fact]
        public void WillNotAssociateIfCommentHasNoWorkItems()
        {
            Results r1 = attach.Attach(associateWorkItemWithChangeSet.Associate);
            Results r2 = attach.Attach(associateWorkItemWithChangeSet.SetWorkItemFixed);

            provider.AssociateWorkItemsWithChangeSet("blah blah", 15);

            Assert.False(r1.WasCalled);
            Assert.False(r2.WasCalled);
        }

        [Fact]
        public void WillExtractWorkItemsFromCheckInCommentsAndAssociateWithChangeSet()
        {
            Results r1 = attach.Attach(associateWorkItemWithChangeSet.Associate);
            Results r2 = attach.Attach(associateWorkItemWithChangeSet.SetWorkItemFixed);
            string comment = @"blah blah
Work Item: 15";

            provider.AssociateWorkItemsWithChangeSet(comment, 15);

            Assert.Equal(15, r1.Parameters[0]);
            Assert.Equal(15, r1.Parameters[1]);
            Assert.Equal(15, r2.Parameters[0]);
        }

        [Fact]
        public void CanAssociateMoreThanOneId()
        {
            Results r1 = attach.Attach(associateWorkItemWithChangeSet.Associate);
            Results r2 = attach.Attach(associateWorkItemWithChangeSet.SetWorkItemFixed);
            string comment = @"blah blah
Work Items: 15, 16, 17";

            provider.AssociateWorkItemsWithChangeSet(comment, 15);

            Assert.Equal(15, r1.History[0].Parameters[0]);
            Assert.Equal(15, r1.History[0].Parameters[1]);
            Assert.Equal(16, r1.History[1].Parameters[0]);
            Assert.Equal(15, r1.History[1].Parameters[1]);
            Assert.Equal(17, r1.History[2].Parameters[0]);
            Assert.Equal(15, r1.History[2].Parameters[1]);
            Assert.Equal(15, r2.History[0].Parameters[0]);
            Assert.Equal(16, r2.History[1].Parameters[0]);
            Assert.Equal(17, r2.History[2].Parameters[0]);
        }

        [Fact]
        public void CanAssociateOnMultiplyLines()
        {
            Results r1 = attach.Attach(associateWorkItemWithChangeSet.Associate);
            Results r2 = attach.Attach(associateWorkItemWithChangeSet.SetWorkItemFixed);
            string comment = @"blah blah
Work Items: 15, 16
Work Item: 17";

            provider.AssociateWorkItemsWithChangeSet(comment, 15);

            Assert.Equal(15, r1.History[0].Parameters[0]);
            Assert.Equal(15, r1.History[0].Parameters[1]);
            Assert.Equal(16, r1.History[1].Parameters[0]);
            Assert.Equal(15, r1.History[1].Parameters[1]);
            Assert.Equal(17, r1.History[2].Parameters[0]);
            Assert.Equal(15, r1.History[2].Parameters[1]);
            Assert.Equal(15, r2.History[0].Parameters[0]);
            Assert.Equal(16, r2.History[1].Parameters[0]);
            Assert.Equal(17, r2.History[2].Parameters[0]);
        }

        [Fact]
        public void WillRecognizeWorkItemsIfWorkItemAppearsPreviouslyInText()
        {
            Results r1 = attach.Attach(associateWorkItemWithChangeSet.Associate);
            Results r2 = attach.Attach(associateWorkItemWithChangeSet.SetWorkItemFixed);
            string comment = @"Adding work items support and fixing
other issues with workitems
Solved Work Items: 15, 16
Fixed WorkItem: 17
Assoicate with workitem: 81";

            provider.AssociateWorkItemsWithChangeSet(comment, 15);

            Assert.Equal(15, r1.History[0].Parameters[0]);
            Assert.Equal(15, r1.History[0].Parameters[1]);
            Assert.Equal(16, r1.History[1].Parameters[0]);
            Assert.Equal(15, r1.History[1].Parameters[1]);
            Assert.Equal(17, r1.History[2].Parameters[0]);
            Assert.Equal(15, r1.History[2].Parameters[1]);
            Assert.Equal(81, r1.History[3].Parameters[0]);
            Assert.Equal(15, r1.History[3].Parameters[1]);
            Assert.Equal(15, r2.History[0].Parameters[0]);
            Assert.Equal(16, r2.History[1].Parameters[0]);
            Assert.Equal(17, r2.History[2].Parameters[0]);
            Assert.Equal(81, r2.History[3].Parameters[0]);
        }
    }
}
