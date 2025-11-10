using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class ProxyJCDecaux : IProxyJCDecaux
    {
        private readonly GenericProxyCache<CachedItem<String>> proxyCache 
            = new GenericProxyCache<CachedItem<String>> ();

        private JCDecaux jcdecaux = new JCDecaux ();

        public string GetStations(string contract)
        {
            string key = $"stations_{contract.ToLower()}";

            if (proxyCache.Contains(key))
            {
                Console.WriteLine($"[CACHE] Hit for {contract}");
                return proxyCache.Get(key).Value;
            }

            Console.WriteLine($"[CACHE] Miss for {contract}");
            string json = jcdecaux.GetStations(contract);

            if (json != null)
            {
                //The Getter here init a CachedItem in the cache
                CachedItem<String> item = proxyCache.Get(key, 60);
                //So we can modifie its value here
                item.Value = json;
            }

            //may be null if the REST API request failed
            return json;
        }
    }
}
