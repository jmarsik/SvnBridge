using System.Net;

namespace SvnBridge.SourceControl
{
    public delegate ISourceControlProvider CreateSourceControlProvider(string serverUrl, NetworkCredential credential);
    
    public static class SourceControlProviderFactory
    {
        private static CreateSourceControlProvider createDelegate;
        
        public static CreateSourceControlProvider CreateDelegate
        {
            set { createDelegate = value; }
        }

        public static ISourceControlProvider Create(string serverUrl, string projectName, NetworkCredential credential)
        {
            if (createDelegate == null)
                return new TFSSourceControlProvider(serverUrl, projectName, credential);
            else
                return createDelegate(serverUrl, credential);
        }
    }
}
