using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Text;
using NUnit.Framework;
using Assert = CodePlex.NUnitExtensions.Assert;

namespace SvnBridge.Net
{
    [TestFixture]
    public class HttpContextTests
    {
        [Test]
        public void UserNullWithoutAuthHeader()
        {
            string request =
                "GET /foo/bar HTTP/1.1\r\n" +
                "Content-Length: 0\r\n" +
                "Host: localhost:8081\r\n" +
                "\r\n";
            MemoryStream stream = new MemoryStream(Constants.BufferSize);
            byte[] buffer = Encoding.Default.GetBytes(request);
            stream.Write(buffer, 0, buffer.Length);
            stream.Position = 0;

            HttpContext context = new HttpContext(stream);

            Assert.Null(context.User);
        }
        
        [Test]
        public void UserNameInAuthHeaderRead()
        {
            string request =
                "GET /foo/bar HTTP/1.1\r\n" +
                "Authorization: Basic {0}\r\n" +
                "Content-Length: 0\r\n" +
                "Host: localhost:8081\r\n" +
                "\r\n";
            request = String.Format(request, Convert.ToBase64String(Encoding.Default.GetBytes("foo:bar")));
            MemoryStream stream = new MemoryStream(Constants.BufferSize);
            byte[] buffer = Encoding.Default.GetBytes(request);
            stream.Write(buffer, 0, buffer.Length);
            stream.Position = 0;
            
            HttpContext context = new HttpContext(stream);

            Assert.NotNull(context.User);
            Assert.NotNull(context.User.Identity);
            Assert.IsType<GenericPrincipal>(context.User);
            Assert.IsType<HttpListenerBasicIdentity>(context.User.Identity);
            Assert.Equal("foo", ((HttpListenerBasicIdentity)context.User.Identity).Name);
        }

        [Test]
        public void UserPasswordInAuthHeaderRead()
        {
            string request =
                "GET /foo/bar HTTP/1.1\r\n" +
                "Authorization: Basic {0}\r\n" +
                "Content-Length: 0\r\n" +
                "Host: localhost:8081\r\n" +
                "\r\n";
            request = String.Format(request, Convert.ToBase64String(Encoding.Default.GetBytes("foo:bar")));
            MemoryStream stream = new MemoryStream(Constants.BufferSize);
            byte[] buffer = Encoding.Default.GetBytes(request);
            stream.Write(buffer, 0, buffer.Length);
            stream.Position = 0;

            HttpContext context = new HttpContext(stream);

            Assert.NotNull(context.User);
            Assert.NotNull(context.User.Identity);
            Assert.IsType<GenericPrincipal>(context.User);
            Assert.IsType<HttpListenerBasicIdentity>(context.User.Identity);
            Assert.Equal("bar", ((HttpListenerBasicIdentity)context.User.Identity).Password);
        }

        [Test]
        public void UserDomainInAuthHeaderIgnored()
        {
            string request =
                "GET /foo/bar HTTP/1.1\r\n" +
                "Authorization: Basic {0}\r\n" +
                "Content-Length: 0\r\n" +
                "Host: localhost:8081\r\n" +
                "\r\n";
            request = String.Format(request, Convert.ToBase64String(Encoding.Default.GetBytes(@"whee\foo:bar")));
            MemoryStream stream = new MemoryStream(Constants.BufferSize);
            byte[] buffer = Encoding.Default.GetBytes(request);
            stream.Write(buffer, 0, buffer.Length);
            stream.Position = 0;

            HttpContext context = new HttpContext(stream);

            Assert.NotNull(context.User);
            Assert.NotNull(context.User.Identity);
            Assert.IsType<GenericPrincipal>(context.User);
            Assert.IsType<HttpListenerBasicIdentity>(context.User.Identity);
            Assert.Equal("foo", ((HttpListenerBasicIdentity)context.User.Identity).Name);
        }
    }
}
