using System;
using System.Net.Sockets;
using SvnBridge.Interfaces;

namespace SvnBridge.Infrastructure
{
    public class RetryOnSocketExceptionsInterceptor : IInterceptor
    {
        private ILogger logger;

        public RetryOnSocketExceptionsInterceptor(ILogger logger)
        {
            this.logger = logger;
        }

        public void Invoke(IInvocation invocation)
        {
            SocketException se = null;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    invocation.Proceed();
                    return;
                }
                catch (SocketException e)
                {
                    se = e;
                    // we will retry here, since we assume that the failure is trasient
                    logger.Info("Socket Error occured, attempt #" + (i + 1) + ", retrying...", e);
                }
            }
            logger.Error("All retries failed", se);
        }
    }
}