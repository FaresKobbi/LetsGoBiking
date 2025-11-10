using RoutingServer.ProxyService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingServer
{
    public class RoutingWithBikes : IRoutingWithBikesSOAP, IRoutingWithBikesREST
    {

        ProxyServiceClient proxyServiceClient = new ProxyServiceClient();



        //OpenStreetMap CALLS
        public string GetBestRoute(string start, string end)
        {
            Console.WriteLine($"[RoutingWithBikes] Calcul d’itinéraire de {start} à {end}");

            // Exemple : ici tu appelleras ton Proxy SOAP
            // var proxy = new ProxyClient();
            // string stations = proxy.GetStations("Lyon");

            return $"Itinéraire optimisé entre {start} et {end}";
        }


        //JCDecaux CALLS
        public string GetStations(string contract)
        {   
            Console.WriteLine($"[RoutingWithBikes] SOAP Call from Proxy getting stations from contract: {contract}");
            return proxyServiceClient.GetStations(contract);
        }

    }
}
