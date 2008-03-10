using System;

namespace SvnBridge.Infrastructure
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class InterceptorAttribute : Attribute
    {
        public InterceptorAttribute(Type interceptor)
        {
            Interceptor = interceptor;
        }

        public Type Interceptor;

    }
}