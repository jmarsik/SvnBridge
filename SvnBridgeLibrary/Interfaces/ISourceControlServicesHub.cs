using System.Net;
using SvnBridge.Infrastructure;
using SvnBridge.SourceControl;

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
        IFileCache FileCache { get; }
		IMetaDataRepositoryFactory MetaDataRepositoryFactory { get; }
        FileRepository FileRepository { get; }
    }
}