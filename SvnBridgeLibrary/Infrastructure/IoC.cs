using System;
using System.Collections;

namespace SvnBridge.Infrastructure
{
    public static class IoC
    {
        public static Container Container = new Container();

        public static T Resolve<T>(IDictionary dependencies)
        {
            return Container.Resolve<T>(dependencies);
        }

        public static T Resolve<T>()
        {
            return Container.Resolve<T>(new Hashtable());
        }


        public static object Resolve(Type type,
                                     IDictionary dependencies)
        {
            return Container.Resolve(type, dependencies);
        }

        public static void Reset()
        {
            Container = new Container();
        }
    }
}