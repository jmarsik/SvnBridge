using System.IO;
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
        int latestChangeSetId;
        int workItemId;
        private WorkItemStore store;
        public override void SetUp()
        {
            base.SetUp();
            
            TeamFoundationServer server = TeamFoundationServerFactory.GetServer(Settings.Default.ServerUrl);
            store = (WorkItemStore)server.GetService(typeof(WorkItemStore));

            AssociateWorkItemWithChangeSetTest.CreateWorkItemAndGetLatestChangeSet(out latestChangeSetId, out workItemId);
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
    }
}