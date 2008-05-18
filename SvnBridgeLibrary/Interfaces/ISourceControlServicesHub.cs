using System.Net;
using CodePlex.TfsLibrary.ObjectModel;
using SvnBridge.Infrastructure;
using SvnBridge.SourceControl;

namespace SvnBridge.Interfaces
{
    public interface ISourceControlServicesHub
    {
        ICredentials Credentials { get; }
        IWebTransferService WebTransferService { get; }
        ITFSSourceControlService SourceControlService { get; }
        IProjectInformationRepository ProjectInformationRepository { get; }
        IAssociateWorkItemWithChangeSet AssociateWorkItemWithChangeSet { get; }
        ILogger Logger { get; }
        ICache Cache { get; }
        IFileCache FileCache { get; }
		IMetaDataRepositoryFactory MetaDataRepositoryFactory { get; }
        IFileRepository FileRepository { get; }
    }
}