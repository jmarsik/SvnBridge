using System.Net;
using SvnBridge.Infrastructure;
using SvnBridge.SourceControl;
using SvnBridge.Cache;

namespace SvnBridge.Interfaces
{
    public interface ISourceControlServicesHub
    {
        ICredentials Credentials { get; }
        ITFSSourceControlService SourceControlService { get; }
        IProjectInformationRepository ProjectInformationRepository { get; }
        IAssociateWorkItemWithChangeSet AssociateWorkItemWithChangeSet { get; }
        ILogger Logger { get; }
        ICache Cache { get; }
        FileCache FileCache { get; }
		IMetaDataRepositoryFactory MetaDataRepositoryFactory { get; }
        FileRepository FileRepository { get; }
    }
}