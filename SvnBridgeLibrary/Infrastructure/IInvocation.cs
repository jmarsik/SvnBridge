using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace SvnBridge.Infrastructure
{
    public interface IInvocation
    {
        object[] Arguments{ get; }
        void Proceed();
        MethodInfo Method { get;}
        object ReturnValue { get; set; }
    }
}