using System.Net.Sockets;
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
            if (se == null)
                return;
            logger.Error("All retries failed", se);
            ExceptionHelper.PreserveStackTrace(se);
            throw se;
        }
    }
}