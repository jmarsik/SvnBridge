using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace SvnBridge.Infrastructure
{
    public static class ProxyFactory
    {
        public static T Create<T>(T instance, IInterceptor interceptor)
        {
            return (T) Create(typeof (T), instance, interceptor);
        }

        public static object Create(Type type, object instance, IInterceptor interceptor)
        {
            return new RemotingProxy(type, interceptor, instance).GetTransparentProxy();
        }

        public class RemotingProxy : RealProxy
        {
            private readonly IInterceptor interceptor;
            private readonly object target;

            public RemotingProxy(Type classToProxy,
                                 IInterceptor interceptor,
                                 object target)
                : base(classToProxy)
            {
                this.interceptor = interceptor;
                this.target = target;
            }

            public override IMessage Invoke(IMessage msg)
            {
                IMethodCallMessage callMessage = msg as IMethodCallMessage;

                if (callMessage == null)
                {
                    return null;
                }
                RemotingInvocation invocation = new RemotingInvocation(this, callMessage, target);
                interceptor.Invoke(invocation);
                return ReturnValue(invocation.ReturnValue, invocation.Arguments, callMessage);
            }

            private static IMessage ReturnValue(object value, IMethodCallMessage mcm)
            {
                return new ReturnMessage(value, null, 0, mcm.LogicalCallContext, mcm);
            }

            private static IMessage ReturnValue(object value, object[] outParams, IMethodCallMessage mcm)
            {
                return new ReturnMessage(value, outParams, outParams == null ? 0 : outParams.Length, mcm.LogicalCallContext, mcm);
            }
        }
    }
}