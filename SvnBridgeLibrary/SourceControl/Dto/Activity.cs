using System.Collections.Generic;

namespace SvnBridge.SourceControl.Dto
{
    public class Activity
    {
        public readonly Dictionary<string, Dictionary<string, string>> AddedProperties =
            new Dictionary<string, Dictionary<string, string>>();

        public readonly List<string> Collections = new List<string>();
        public readonly List<CopyAction> CopiedItems = new List<CopyAction>();
        public readonly List<string> DeletedItems = new List<string>();
        public readonly List<ActivityItem> MergeList = new List<ActivityItem>();
        public readonly List<string> PostCommitDeletedItems = new List<string>();
        public readonly Dictionary<string, List<string>> RemovedProperties = new Dictionary<string, List<string>>();
        public string Comment;
    }
}