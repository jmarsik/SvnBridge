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
        
        public static ISourceControlProvider Create(string serverUrl, NetworkCredential credential)
        {
            if (createDelegate == null)
                return new TFSSourceControlProvider(serverUrl, null, credential);
            else
                return createDelegate(serverUrl, credential);
        }
    }
}
