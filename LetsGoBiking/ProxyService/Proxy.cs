using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ProxyService
{
    // REMARQUE : vous pouvez utiliser la commande Renommer du menu Refactoriser pour changer le nom de classe "Service1" à la fois dans le code et le fichier de configuration.
    public class ProxyService : IProxyService
    {
        private readonly GenericProxyCache<CachedItem<String>> proxyCache
            = new GenericProxyCache<CachedItem<String>>();

        private JCDecaux jcdecaux = new JCDecaux();

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
