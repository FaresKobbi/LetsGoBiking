using ProxyService.Models;
using System;
using System.Collections.Generic;

namespace ProxyService
{
    public class ProxyService : IProxyService
    {
        private readonly GenericProxyCache<CachedItem<List<ICacheableItem>>> proxyCache
            = new GenericProxyCache<CachedItem<List<ICacheableItem>>>();

        private readonly JCDecaux jcdecaux = new JCDecaux();

        public List<JCContract> GetAllContracts()
        {
            const string key = "all_contracts";

            if (proxyCache.Contains(key))
            {
                Console.WriteLine("[CACHE] Hit for contracts");
                return proxyCache.Get(key).Value.ConvertAll(item => (JCContract)item);
            }

            Console.WriteLine("[CACHE] Miss for contracts");
            List<JCContract> contracts = jcdecaux.GetAllContracts();


            if (contracts != null)
            {
                CachedItem<List<ICacheableItem>> item = proxyCache.Get(key, 86400);
                item.Value = new List<ICacheableItem>(contracts);
            }

            return contracts;
        }

        public List<Station> GetStationsForContract(string contract)
        {
            string key = $"stations_{contract.ToLower()}";

            if (proxyCache.Contains(key))
            {
                Console.WriteLine($"[CACHE] Hit for {contract}");
                return proxyCache.Get(key).Value.ConvertAll(item => (Station)item);
            }

            Console.WriteLine($"[CACHE] Miss for {contract}");
            List<Station> stations = jcdecaux.GetStationsForContract(contract);

            if (stations != null)
            {
                CachedItem<List<ICacheableItem>> item = proxyCache.Get(key, 120);
                item.Value = new List<ICacheableItem>(stations);
            }

            return stations;
        }
    }
}
