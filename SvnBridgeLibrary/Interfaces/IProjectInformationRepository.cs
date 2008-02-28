using System.Net;
using SvnBridge.SourceControl;

namespace SvnBridge.Interfaces
{
    public interface IProjectInformationRepository
    {
        ProjectLocationInformation GetProjectLocation(ICredentials credentials,
                                                      string projectName);
    }
}