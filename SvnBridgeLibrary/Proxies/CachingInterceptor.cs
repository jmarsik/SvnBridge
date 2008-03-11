using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml.Serialization;
using SvnBridge.Interfaces;

namespace SvnBridge.Proxies
{
    public class CachingInterceptor : IInterceptor
    {
        private readonly ICache cache;

        private readonly ReaderWriterLock rwLock = new ReaderWriterLock();
        private readonly Dictionary<Type, XmlSerializer> serializers = new Dictionary<Type, XmlSerializer>();

        public CachingInterceptor(ICache cache)
        {
            this.cache = cache;
        }

        public void Invoke(IInvocation invocation)
        {
            string cacheKey = GetCacheKey(invocation);
            CachedResult cached = cache.Get(cacheKey);
            if (cached != null)
            {
                invocation.ReturnValue =  cached.Value;
                return;
            }

            invocation.Proceed();

            cache.Set(cacheKey, invocation.ReturnValue);
        }

       

        private string GetCacheKey(IInvocation invocation)
        {
            StringWriter sw = new StringWriter();
            rwLock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                foreach (object argument in invocation.Arguments)
                {
                    EnsureArgumentHasXmlSerializer(argument);
                    WriteArgumentAsString(argument, sw);
                    sw.Write("_");
                }
            }
            finally
            {
                rwLock.ReleaseReaderLock();
            }
            return sw.GetStringBuilder().ToString();
        }

        private void WriteArgumentAsString(object argument, TextWriter writer)
        {
            if (argument == null)
            {
                writer.Write("NULL");
                return;
            }
            XmlSerializer serializer = serializers[argument.GetType()];
            if (serializer != null)
            {
                serializer.Serialize(writer, argument);
            }
            else
            {
                writer.Write(argument);
            }
        }

        private void EnsureArgumentHasXmlSerializer(object argument)
        {
            if (argument != null)
            {
                Type argumentType = argument.GetType();
                if (serializers.ContainsKey(argumentType) == false)
                {
                    LockCookie writerLock = rwLock.UpgradeToWriterLock(Timeout.Infinite);
                    try
                    {
                        if (serializers.ContainsKey(argumentType) == false)
                        {
                            serializers[argumentType] = GetSerializer(argumentType);
                        }
                    }
                    finally
                    {
                        rwLock.DowngradeFromWriterLock(ref writerLock);
                    }
                }
            }
        }

        private static XmlSerializer GetSerializer(Type type)
        {
            if (type.GetCustomAttributes(typeof (XmlRootAttribute), true).Length == 0)
            {
                return null;
            }
            return new XmlSerializer(type);
        }
    }
}