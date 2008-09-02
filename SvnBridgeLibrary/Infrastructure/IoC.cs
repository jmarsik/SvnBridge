using System;
using System.Collections;

namespace SvnBridge.Infrastructure
{
    public static class IoC
    {
        private static Container Container;

    	static IoC()
    	{
    		Reset();
    	}

        public static void Register(Type service, Type impl)
        {
            Container.Register(service, impl);
        }

        public static T Resolve<T>()
        {
            return (T)Container.Resolve(typeof(T), new Hashtable());
        }

        public static T Resolve<T>(IDictionary constructorParams)
        {
            return (T)Container.Resolve(typeof(T), constructorParams);
        }

        public static void Reset()
        {
            Container = new Container();
        }
    }
}