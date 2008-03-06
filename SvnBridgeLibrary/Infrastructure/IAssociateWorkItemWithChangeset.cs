namespace SvnBridge.Infrastructure
{
    public interface IAssociateWorkItemWithChangeSet
    {
        void Associate(int workItemId, int changeSetId);
        void SetWorkItemFixed(int workItemId);
    }
}