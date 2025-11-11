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
        private readonly ORS orsClient = new ORS();

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
                CachedItem<List<ICacheableItem>> item = proxyCache.Get(key, 300);
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

        public GeoCodeResponse GetCoordinates(string address, int expiration)
        {
            string key = $"geocode_{address.ToLower()}";

            if (proxyCache.Contains(key))
            {
                Console.WriteLine($"[CACHE] Hit for {address}");
                return (GeoCodeResponse)proxyCache.Get(key).Value[0];
            }

            Console.WriteLine($"[CACHE] Miss for {address}");
            List<GeoCodeResponse> result =new List<GeoCodeResponse>();
            result.Add(orsClient.GetCoordinates(address));

            if (result != null)
            {
                CachedItem<List<ICacheableItem>> item = proxyCache.Get(key, expiration * 60);
                item.Value = new List<ICacheableItem>(result);
            }

            return result[0];
        }
        public RouteResponse GetRoute(string profile, string startCoords, string endCoords, int expiration)
        {
            string key = $"route_{profile}_{startCoords}_{endCoords}".ToLower();

            if (proxyCache.Contains(key))
            {
                Console.WriteLine($"[CACHE] Hit for {key}");
                return (RouteResponse)proxyCache.Get(key).Value[0];
            }

            Console.WriteLine($"[CACHE] Miss for route {key}");
            List<RouteResponse> result = new List<RouteResponse>();
            result.Add(orsClient.GetRoute(profile, startCoords, endCoords));

            if (result != null)
            {
                CachedItem<List<ICacheableItem>> item = proxyCache.Get(key, expiration*60);
                item.Value = new List<ICacheableItem>(result);
            }

            return result[0];
        }

    }
}
