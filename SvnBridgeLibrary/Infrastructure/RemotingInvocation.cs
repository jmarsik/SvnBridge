using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace SvnBridge.Infrastructure
{
    internal class RemotingInvocation : IInvocation
    {
        private readonly IMethodCallMessage message;
        private readonly object target;
        private object _returnValue;
        private readonly RealProxy realProxy;
        private readonly object[] args;

        public RemotingInvocation(RealProxy realProxy, IMethodCallMessage message, object target)
        {
            this.message = message;
            this.target = target;
            this.realProxy = realProxy;
            this.args = (object[])this.message.Properties["__Args"];
        }

        public object[] Arguments
        {
            get { return args; }
        }

        public object GetArgumentValue(int index)
        {
            throw new NotSupportedException();
        }

        public MethodInfo GetConcreteMethod()
        {
            return (MethodInfo)message.MethodBase;
        }

        public MethodInfo Method
        {
            get { return GetConcreteMethod(); }
        }

        public void Proceed()
        {

            try
            {
                ReturnValue = message.MethodBase.Invoke(target, Arguments);
            }
            catch (TargetInvocationException e)
            {
                Exception exception = e.InnerException;

                typeof(Exception).GetMethod("InternalPreserveStackTrace",
                                             BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(exception, new object[0]);

                throw exception;
            }
        }

        public object Proxy
        {
            get { return realProxy.GetTransparentProxy(); }
        }

        public object ReturnValue
        {
            get { return _returnValue; }
            set { _returnValue = value; }
        }
    }
}