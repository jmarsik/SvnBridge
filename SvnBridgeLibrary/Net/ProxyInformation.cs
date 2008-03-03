namespace SvnBridge.Net
{
    public class ProxyInformation
    {
        private bool useProxy;
        private int port;
        private string username;
        private string password;
        private bool useDefaultCredentails;
        private string url;

        public bool UseProxy
        {
            get { return useProxy; }
            set { useProxy = value; }
        }

        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public bool UseDefaultCredentails
        {
            get { return useDefaultCredentails; }
            set { useDefaultCredentails = value; }
        }

        public string Url
        {
            get { return url; }
            set { url = value; }
        }
    }
}