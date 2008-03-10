using System;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using Rhino.Mocks;
using SvnBridge.Interfaces;
using SvnBridge.Proxies;

namespace SvnBridge.Proxies
{
    [TestFixture]
    public class RetryOnSocketExceptionTest
    {
        private MockRepository mocks;

        [SetUp]
        public void TestInitialize()
        {
            mocks = new MockRepository();
        }

        [TearDown]
        public void TestCleanup()
        {
            mocks.VerifyAll();
        }

        [Test]
        public void WillNotFailOnFirstSocketException()
        {
            RetryOnSocketExceptionsInterceptor interceptor = new RetryOnSocketExceptionsInterceptor(mocks.Stub<ILogger>());
            IInvocation mock = mocks.CreateMock<IInvocation>();
            // first call throws
            Expect.Call(mock.Proceed).Throw(new SocketException());

            // second succeed
            Expect.Call(mock.Proceed);

            mocks.ReplayAll();

            interceptor.Invoke(mock);
        }

        [Test]
        public void WillFailOnNonSocketException()
        {
            RetryOnSocketExceptionsInterceptor interceptor = new RetryOnSocketExceptionsInterceptor(mocks.Stub<ILogger>());
            IInvocation mock = mocks.CreateMock<IInvocation>();
          
            Expect.Call(mock.Proceed).Throw(new WebException());

            mocks.ReplayAll();

            try
            {
                interceptor.Invoke(mock);
                Assert.Fail("Should have thrown");
            }
            catch(WebException)
            {

            }
        }

        [Test]
        public void WillThrowAfterThreeAttempts()
        {
            RetryOnSocketExceptionsInterceptor interceptor = new RetryOnSocketExceptionsInterceptor(mocks.Stub<ILogger>());
            IInvocation mock = mocks.CreateMock<IInvocation>();
            Expect.Call(mock.Proceed).Throw(new SocketException()).Repeat.Times(3);

            mocks.ReplayAll();

            try
            {
                interceptor.Invoke(mock);
                Assert.Fail("Should have thrown");
            }
            catch (SocketException)
            {

            }
        }
    }
}