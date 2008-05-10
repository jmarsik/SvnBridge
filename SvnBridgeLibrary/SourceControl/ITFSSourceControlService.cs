using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;

namespace SvnBridge.SourceControl
{
    public interface ITFSSourceControlService : ISourceControlService
    {
        ExtendedItem[][] QueryItemsExtended(string tfsUrl,
                                            ICredentials credentials,
                                            string workspaceName,
                                            ItemSpec[] items,
                                            DeletedState deletedState,
                                            ItemType itemType);

        BranchRelative[][] QueryBranches(string tfsUrl,
                                         ICredentials credentials,
                                         string workspaceName,
                                         ItemSpec[] items,
                                         VersionSpec version);

    }
}
