using System;
using System.Collections;

namespace SvnBridge.Infrastructure
{
    public static class IoC
    {
        public static Container Container;

    	static IoC()
    	{
    		Reset();
    	}

        public static T Resolve<T>()
        {
            return (T)Container.Resolve(typeof(T), new Hashtable());
        }

        public static T Resolve<T>(IDictionary dependencies)
        {
            return (T)Container.Resolve(typeof(T), dependencies);
        }

        public static object Resolve(Type type, IDictionary dependencies)
        {
            return Container.Resolve(type, dependencies);
        }

        public static void Reset()
        {
            Container = new Container();
        }
    }
}