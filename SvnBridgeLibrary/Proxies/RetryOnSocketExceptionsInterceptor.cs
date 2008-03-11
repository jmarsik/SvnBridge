using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SvnBridge.Infrastructure;
using SvnBridge.Interfaces;

namespace SvnBridge.Proxies
{
    public class RetryOnSocketExceptionsInterceptor : IInterceptor
    {
        private readonly ILogger logger;

        public RetryOnSocketExceptionsInterceptor(ILogger logger)
        {
            this.logger = logger;
        }

        public void Invoke(IInvocation invocation)
        {
            Exception exception = null;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    invocation.Proceed();
                    return;
                }
                catch (WebException we)
                {
                    exception = we;
                    // we will retry here, since we assume that the failure is trasient
                    logger.Info("Web Exception occured, attempt #" + (i + 1) + ", retrying...", we);
                }
                catch (SocketException se)
                {
                    exception = se;
                    // we will retry here, since we assume that the failure is trasient
                    logger.Info("Socket Error occured, attempt #" + (i + 1) + ", retrying...", se);
                }

                // if we are here we got an network exception, we will assume this is a
                // trasient situation and wait a bit, hopefully it will clear up
                Thread.Sleep(500);
            }
            if (exception == null)
                return;
            logger.Error("All retries failed", exception);
            ExceptionHelper.PreserveStackTrace(exception);
            throw exception;
        }
    }
}
