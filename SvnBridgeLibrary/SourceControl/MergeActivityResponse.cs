using System;
using System.Collections.Generic;

namespace SvnBridge.SourceControl
{
    public class MergeActivityResponse
    {
        public int Version;
        public string Creator;
        public DateTime CreationDate;
        public List<MergeActivityResponseItem> Items = new List<MergeActivityResponseItem>();

        public MergeActivityResponse(int version,
                                     DateTime creationDate,
                                     string creator)
        {
            Version = version;
            CreationDate = creationDate;
            Creator = creator;
        }
    }
}