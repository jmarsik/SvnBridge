using System;
using System.Net;
using System.Net.Sockets;
using Xunit;
using Rhino.Mocks;
using SvnBridge.Interfaces;
using SvnBridge.Proxies;
using Tests;
using SvnBridge.Infrastructure;

namespace SvnBridge.Proxies
{
    public class RetryOnSocketExceptionTest : IDisposable
    {
        private readonly MockRepository mocks;
        private readonly MyMocks attach;

        public RetryOnSocketExceptionTest()
        {
            mocks = new MockRepository();
            attach = new MyMocks();
        }

        public void Dispose()
        {
            mocks.VerifyAll();
        }

        [Fact]
        public void WillNotFailOnFirstSocketException()
        {
			RetryOnExceptionsInterceptor<SocketException> interceptor = new RetryOnExceptionsInterceptor<SocketException>(attach.CreateObject<DefaultLogger>());
            IInvocation mock = mocks.CreateMock<IInvocation>();
            // first call throws
            Expect.Call(mock.Proceed).Throw(new SocketException());

            // second succeed
            Expect.Call(mock.Proceed);

            mocks.ReplayAll();

            interceptor.Invoke(mock);
        }

        [Fact]
        public void WillNotFailOnFirstWebException()
        {
            RetryOnExceptionsInterceptor<WebException> interceptor = new RetryOnExceptionsInterceptor<WebException>(attach.CreateObject<DefaultLogger>());
            IInvocation mock = mocks.CreateMock<IInvocation>();
            // first call throws
            Expect.Call(mock.Proceed).Throw(new WebException());

            // second succeed
            Expect.Call(mock.Proceed);

            mocks.ReplayAll();

            interceptor.Invoke(mock);
        }

        [Fact]
        public void WillFailOnNonSocketOrWebException()
        {
            RetryOnExceptionsInterceptor<WebException> interceptor = new RetryOnExceptionsInterceptor<WebException>(attach.CreateObject<DefaultLogger>());
            IInvocation mock = mocks.CreateMock<IInvocation>();
          
            Expect.Call(mock.Proceed).Throw(new InvalidOperationException());

            mocks.ReplayAll();

            Exception result = Record.Exception(delegate { interceptor.Invoke(mock); });

            Assert.IsType(typeof(InvalidOperationException), result);
        }

        [Fact]
        public void WillThrowAfterThreeAttempts()
        {
            RetryOnExceptionsInterceptor<SocketException> interceptor = new RetryOnExceptionsInterceptor<SocketException>(attach.CreateObject<DefaultLogger>());
            IInvocation mock = mocks.CreateMock<IInvocation>();
            Expect.Call(mock.Proceed).Throw(new SocketException()).Repeat.Times(3);

            mocks.ReplayAll();

            Exception result = Record.Exception(delegate { interceptor.Invoke(mock); });

            Assert.IsType(typeof(SocketException), result);
        }
    }
}
