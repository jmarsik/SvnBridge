namespace SvnBridge.SourceControl
{
    public class ProjectLocationInformation
    {
        public string ServerUrl;
        public string RemoteProjectName;

        public ProjectLocationInformation(string canonizedProjectName, string serverUrl)
        {
            RemoteProjectName = canonizedProjectName;
            ServerUrl = serverUrl;
        }
    }
}