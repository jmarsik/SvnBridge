namespace SvnBridge.Infrastructure
{
    public interface IAssociateWorkItemWithChangeSet
    {
        void Associate(int workItemId, int changeSetId);
    }
}