using System.IO;
using IntegrationTests;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using NUnit.Framework;
using TestsEndToEnd;
using TestsRequiringTfsClient.Properties;

namespace TestsRequiringTfsClient
{
    [TestFixture]
    public class CommitAndSetWorkItemTest : EndToEndTestBase
    {
        private int workItemId;
        private WorkItemStore store;
        private AuthenticateAsLowPrivilegeUser authenticateAsLowPrivilegeUser;

        public override void SetUp()
        {
            base.SetUp();
            authenticateAsLowPrivilegeUser = new AuthenticateAsLowPrivilegeUser();

            TeamFoundationServer server = TeamFoundationServerFactory.GetServer(Settings.Default.ServerUrl);
            store = (WorkItemStore) server.GetService(typeof (WorkItemStore));
            int latestChangeSetId;
            AssociateWorkItemWithChangeSetTest.CreateWorkItemAndGetLatestChangeSet(out latestChangeSetId, out workItemId);
        }

        [TearDown]
        public void TestCleanup()
        {
            authenticateAsLowPrivilegeUser.Dispose();
        }

        [Test]
        public void CanFixWorkItemByCommitMessage()
        {
            CheckoutAndChangeDirectory();

            File.WriteAllText("test.txt", "blah");

            Svn("add test.txt");

            Svn("commit -m \"Done. Work Item: " + workItemId + "\"");

            WorkItem item = store.GetWorkItem(workItemId);
            Assert.AreEqual("Fixed", item.State);
            Assert.AreEqual("Fixed", item.Reason);
        }

        [Test]
        public void AssociateTwoChangesetsWithWorkItem()
        {
            CheckoutAndChangeDirectory();

            File.WriteAllText("test.txt", "blah");

            Svn("add test.txt");

            Svn("commit -m \"Done. Work Item: " + workItemId + "\"");

            File.WriteAllText("test.txt", "hlab");

            Svn("commit -m \"Done. Work Item: " + workItemId + "\"");

            WorkItem item = store.GetWorkItem(workItemId);
            Assert.AreEqual(2, item.Links.Count);
            Assert.AreEqual("Fixed", item.State);
            Assert.AreEqual("Fixed", item.Reason);
        }

        [Test]
        public void AssociatingSingleCheckInWithMultiplyWorkItems()
        {
            int oldWorkItemId = workItemId;
            int latestChangeSetId;
            AssociateWorkItemWithChangeSetTest.CreateWorkItemAndGetLatestChangeSet(out latestChangeSetId, out workItemId);
            string workItems = oldWorkItemId + ", " + workItemId;

            CheckoutAndChangeDirectory();

            File.WriteAllText("test.txt", "blah");

            Svn("add test.txt");

            Svn("commit -m \"Done. Work Item: " + workItems + "\"");

            foreach (int workItem in new int[] {oldWorkItemId, workItemId})
            {
                WorkItem item = store.GetWorkItem(workItem);
                Assert.AreEqual(1, item.Links.Count);
                Assert.AreEqual("Fixed", item.State);
                Assert.AreEqual("Fixed", item.Reason);
            }
        }
    }
}