using System;
using CodePlex.TfsLibrary.ObjectModel;
using Rhino.Mocks.Impl;
using SvnBridge.NullImpl;
using Xunit;
using Rhino.Mocks;
using SvnBridge.Infrastructure;
using SvnBridge.Interfaces;
using NullLogger=SvnBridge.NullImpl.NullLogger;
using Tests;
using SvnBridge.Cache;

namespace SvnBridge.SourceControl
{
    public class TFSSourceControlProviderTest : IDisposable
    {
        private readonly MockFramework attach;
        private readonly MockRepository mocks;
        private readonly IAssociateWorkItemWithChangeSet associateWorkItemWithChangeSet;
        private readonly TFSSourceControlProvider provider;

        public TFSSourceControlProviderTest()
        {
            attach = new MockFramework();
            mocks = new MockRepository();
            associateWorkItemWithChangeSet = mocks.CreateMock<IAssociateWorkItemWithChangeSet>();
            provider = new TFSSourceControlProvider(
                "blah",
                null,
				null,
                CreateSourceControlServicesHub(),
				MockRepository.GenerateStub<IIgnoredFilesSpecification>());
        }

        public SourceControlServicesHub CreateSourceControlServicesHub()
        {
            return new SourceControlServicesHub(
                System.Net.CredentialCache.DefaultCredentials,
                MockRepository.GenerateStub<ITFSSourceControlService>(),
                MockRepository.GenerateStub<IProjectInformationRepository>(),
                associateWorkItemWithChangeSet,
                new NullLogger(),
                new NullCache(),
                attach.CreateObject<FileCache>(null),
				MockRepository.GenerateStub<IMetaDataRepositoryFactory>(),
                attach.CreateObject<FileRepository>("http://www.codeplex.com", null, null, null, null, false));
        }

        public void Dispose()
        {
            mocks.VerifyAll();
        }

        [Fact]
        public void WillNotAssociateIfCommentHasNoWorkItems()
        {
            mocks.ReplayAll();
            provider.AssociateWorkItemsWithChangeSet("blah blah", 15);
        }

        [Fact]
        public void WillExtractWorkItemsFromCheckInCommentsAndAssociateWithChangeSet()
        {
            associateWorkItemWithChangeSet.Associate(15,15);
            associateWorkItemWithChangeSet.SetWorkItemFixed(15);
            mocks.ReplayAll();
            string comment = @"blah blah
Work Item: 15";
            provider.AssociateWorkItemsWithChangeSet(comment, 15);
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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
