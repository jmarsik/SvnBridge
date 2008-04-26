using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Win32;
using SvnBridge.Interfaces;
using SvnBridge.Net;

namespace SvnBridge.Cache
{
    public class RegistryBasedPersistentCache : IPersistentCache
    {
        private RegistryKey registryKey;

        private static int UnitOfWorkNestingLevel
        {
            get
            {
                object item = PerRequest.Items["persistent.file.cache.current.UnitOfWorkNestingLevel"];
                if (item == null)
                    return 0;
                return (int)item;
            }
            set { PerRequest.Items["persistent.file.cache.current.UnitOfWorkNestingLevel"] = value; }
        }

        private static IDictionary<string, PersistentItem> CurrentItems
        {
            get { return (IDictionary<string, PersistentItem>)PerRequest.Items["persistent.file.cache.current.items"]; }
            set { PerRequest.Items["persistent.file.cache.current.items"] = value; }
        }

        #region ICanValidateMyEnvironment Members

        public void ValidateEnvironment()
        {
            registryKey = Registry.CurrentUser.CreateSubKey("SvnBridge");
        }

        #endregion

        #region IPersistentCache Members

        public CachedResult Get(string key)
        {
            CachedResult result = null;
            UnitOfWork(delegate
            {
                if (CurrentItems.ContainsKey(key))
                {
                    result = new CachedResult(CurrentItems[key].Item);
                    return;
                }

                if (Contains(key) == false)
                    return;

                AddToCurrentUnitOfWork(key);
                PersistentItem deserialized = GetDeserializedObject(key);
                CurrentItems[key] = deserialized;
                result = new CachedResult(deserialized.Item);
            });
            return result;
        }

        public void Set(string key, object obj)
        {
            UnitOfWork(delegate
            {
                AddToCurrentUnitOfWork(key);
                CurrentItems[key] = new PersistentItem(key, obj);
            });
        }

        public bool Contains(string key)
        {
            bool contains = false;
            UnitOfWork(delegate
            {
                if (CurrentItems.ContainsKey(key))
                {
                    contains = true;
                    return;
                }
                contains = registryKey.OpenSubKey(key) != null;
            });
            return contains;
        }

        public void Clear()
        {
            string[] names = registryKey.GetSubKeyNames();
            foreach (string subkey in names)
            {
                registryKey.DeleteSubKey(subkey);
            }
        }

        public void Add(string key, string value)
        {
            UnitOfWork(delegate
            {
                AddToCurrentUnitOfWork(key);
                CachedResult result = Get(key);
                ISet<string> set = new HashSet<string>();
                if (result != null)
                    set = (ISet<string>)result.Value;
                if (value!=null)
                    set.Add(value);
                Set(key, set);
            });
        }

        public List<T> GetList<T>(string key)
        {
            List<T> items = new List<T>();
            UnitOfWork(delegate
            {
                CachedResult result = Get(key);
                if (result == null)
                    return;

                if (result.Value is T)
                {
                    items.Add((T)result.Value);
                    return;
                }

                foreach (string itemKey in (IEnumerable<string>)result.Value)
                {
                    CachedResult itemResult = Get(itemKey);
                    if (itemResult != null)
                        items.Add((T)itemResult.Value);
                }
            });
            return items;
        }

        public void UnitOfWork(Action action)
        {
            UnitOfWorkNestingLevel += 1;
            if (UnitOfWorkNestingLevel == 1)
            {
                CurrentItems = new Dictionary<string, PersistentItem>(StringComparer.InvariantCultureIgnoreCase);
            }
            bool hasException = false;
            try
            {
                action();
                if (UnitOfWorkNestingLevel == 1)
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    foreach (PersistentItem item in CurrentItems.Values)
                    {
                        if (item.Changed == false)
                            continue;
                        MemoryStream ms = new MemoryStream();
                        bf.Serialize(ms, item);
                        RegistryKey key = registryKey.CreateSubKey(item.Name);
                        key.SetValue("SerializedObject", ms.ToArray(), RegistryValueKind.Binary);
                    }
                }
            }
            catch
            {
                hasException = true;
                throw;
            }
            finally
            {
                if (hasException == false && UnitOfWorkNestingLevel == 1)
                {
                    CurrentItems = null;
                }
                UnitOfWorkNestingLevel -= 1;
            }
        }

        #endregion


        private PersistentItem GetDeserializedObject(string key)
        {
            RegistryKey cacheKey = registryKey.CreateSubKey(key);
            BinaryFormatter formatter = new BinaryFormatter();
            byte[] buffer = (byte[])cacheKey.GetValue("SerializedObject");

            return (PersistentItem)formatter.Deserialize(new MemoryStream(buffer));
        }

        /// <summary>
        /// This should lock the file
        /// </summary>
        /// <param name="key"></param>
        private void AddToCurrentUnitOfWork(string key)
        {
            
        }

        #region Nested type: PersistentItem

        [Serializable]
        public class PersistentItem
        {
            [NonSerialized]
            public bool Changed;
            public object Item;
            public string Name;

            public PersistentItem()
            {
            }


            public PersistentItem(string name, object item)
            {
                Name = name;
                Item = item;
                Changed = true;
            }
        }

        #endregion
    }
}