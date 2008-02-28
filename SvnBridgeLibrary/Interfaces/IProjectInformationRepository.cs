using CodePlex.TfsLibrary.ObjectModel;
using CodePlex.TfsLibrary.RepositoryWebSvc;
using SvnBridge.SourceControl;
using System.Net;

namespace SvnBridge.Interfaces
{
    public interface IProjectInformationRepository
    {
        ProjectLocationInformation GetProjectLocation(ICredentials credentials,string projectName);
    }
}
