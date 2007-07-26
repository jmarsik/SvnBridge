using System;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Text;

namespace SvnBridge.Net
{
    public class HttpContext : IHttpContext
    {
        private readonly HttpRequest request;
        private readonly HttpResponse response;
        private IPrincipal user;

        public HttpContext(Stream stream)
        {
            request = new HttpRequest(stream);
            response = new HttpResponse(request, stream);

            Authenticate();
        }

        #region IHttpContext Members

        public IHttpRequest Request
        {
            get { return request; }
        }

        public IHttpResponse Response
        {
            get { return response; }
        }

        public IPrincipal User
        {
            get { return user; }
        }

        #endregion

        private void Authenticate()
        {
            string authorizationHeader = request.Headers["Authorization"];

            if (authorizationHeader != null)
            {
                string encodedCredential = authorizationHeader.Substring(authorizationHeader.IndexOf(' ') + 1);
                string credential = UTF8Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredential));
                string[] credentialParts = credential.Split(':');

                string username = credentialParts[0];
                string password = credentialParts[1];

                if (username.IndexOf('\\') >= 0)
                {
                    username = username.Substring(username.IndexOf('\\') + 1);
                }

                user = new GenericPrincipal(new HttpListenerBasicIdentity(username, password), new string[0] {});
            }
        }
    }
}