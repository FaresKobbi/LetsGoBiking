using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace ProxyService
{
    internal class GenericProxyCache<T> where T : new()
    {
        private ObjectCache cache = MemoryCache.Default;
        public DateTimeOffset dt_default = ObjectCache.InfiniteAbsoluteExpiration;

        public T Get(string CacheItemName)
        {
            if (cache.Contains(CacheItemName))
                return (T)cache.Get(CacheItemName);

            T item = default(T);
            cache.Set(CacheItemName, item, dt_default);
            return item;
        }

        public T Get(string CacheItemName, double dt_seconds)
        {
            if (cache.Contains(CacheItemName))
                return (T)cache.Get(CacheItemName);

            T item = new T();
            cache.Set(CacheItemName, item, DateTimeOffset.Now.AddSeconds(dt_seconds));
            return item;
        }

        public T Get(string CacheItemName, DateTimeOffset dt)
        {
            if (cache.Contains(CacheItemName))
                return (T)cache.Get(CacheItemName);

            T item = default(T);
            cache.Set(CacheItemName, item, dt);
            return item;
        }


        public bool Contains(string CacheItemName)
        {
            return cache.Contains(CacheItemName);
        }

    }


    public class CachedItem<T>
    {
        public T Value { get; set; }
        public CachedItem() { }
    }

}
