using System.Security.Principal;
using SvnBridge.Net;

namespace SvnBridge.Stubs
{
    public class StubHttpContext : IHttpContext
    {
        internal IHttpRequest RequestProperty;
        internal IHttpResponse ResponseProperty;
        internal IPrincipal UserProperty;

        #region IConnection Members

        public IHttpRequest Request
        {
            get { return RequestProperty; }
            internal set { RequestProperty = value; }
        }

        public IHttpResponse Response
        {
            get { return ResponseProperty; }
            internal set { ResponseProperty = value; }
        }

        public IPrincipal User
        {
            get { return UserProperty; }
            internal set { UserProperty = value; }
        }

        #endregion
    }
}