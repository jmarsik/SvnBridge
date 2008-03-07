using System.Net;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using NUnit.Framework;
using SvnBridge.Infrastructure;
using SvnBridge.SourceControl;
using TestsRequiringTfsClient.Properties;

namespace TestsRequiringTfsClient
{
    [TestFixture]
    public class AssociateWorkItemWithChangeSetTest
    {
        private int workItemId;
        private int changesetId;
        private WorkItemStore store;
        private AuthenticateAsLowPrivilegeUser authenticateAsLowPrivilegeUser;

        [SetUp]
        public void SetUp()
        {
            authenticateAsLowPrivilegeUser = new AuthenticateAsLowPrivilegeUser();

            TeamFoundationServer server = TeamFoundationServerFactory.GetServer(Settings.Default.ServerUrl);
            store = (WorkItemStore)server.GetService(typeof(WorkItemStore));
            CreateWorkItemAndGetLatestChangeSet(out changesetId, out workItemId);
        }

        [TearDown]
        public void TestCleanup()
        {
            authenticateAsLowPrivilegeUser.Dispose();
        }

        public static void CreateWorkItemAndGetLatestChangeSet(out int latestChangeSetId, out int workItemId)
        {
            TeamFoundationServer server = TeamFoundationServerFactory.GetServer(Settings.Default.ServerUrl);
            WorkItemStore store = (WorkItemStore)server.GetService(typeof(WorkItemStore));
            Project project = store.Projects["SvnBridgeTesting"];

            WorkItemType wit = project.WorkItemTypes["Work Item"];
            WorkItem wi = new WorkItem(wit);

            wi.Title = "blah";
            wi.Description = "no";

            wi.Save();

            workItemId = wi.Id;
            VersionControlServer vcs = (VersionControlServer)server.GetService(typeof(VersionControlServer));
            latestChangeSetId = vcs.GetLatestChangesetId();
        }


        [Test]
        public void CanAssociateWorkItemWithChangeSet()
        {
            IAssociateWorkItemWithChangeSet associateWorkItemWithChangeSet =
                new AssociateWorkItemWithChangeSet(Settings.Default.ServerUrl, CredentialsHelper.DefaultCredentials);
            associateWorkItemWithChangeSet.Associate(workItemId, changesetId);
            associateWorkItemWithChangeSet.SetWorkItemFixed(workItemId);

            WorkItem item = store.GetWorkItem(workItemId);
            Assert.AreEqual(1, item.Links.Count);
            Assert.AreEqual("Fixed", item.State);
            Assert.AreEqual("Fixed", item.Reason);
        }

        [Test]
        public void CanAssociateWithWorkItemAfterWorkItemHasBeenModified()
        {
            IAssociateWorkItemWithChangeSet associateWorkItemWithChangeSet =
               new AssociateWorkItemWithChangeSet(Settings.Default.ServerUrl, CredentialsHelper.DefaultCredentials);

            WorkItem item = store.GetWorkItem(workItemId);
            item.History = "test foo";
            item.Save();

            Assert.AreEqual(2, item.Revision);

            associateWorkItemWithChangeSet.Associate(workItemId, changesetId);
            associateWorkItemWithChangeSet.SetWorkItemFixed(workItemId);

            item = store.GetWorkItem(workItemId);
            Assert.AreEqual(1, item.Links.Count);
            Assert.AreEqual("Fixed", item.State);
            Assert.AreEqual("Fixed", item.Reason);
        }
    }
}
