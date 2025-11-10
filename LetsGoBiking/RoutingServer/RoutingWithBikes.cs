using RoutingServer.ProxyService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoutingServer
{
    public class RoutingWithBikes : IRoutingWithBikesSOAP, IRoutingWithBikesREST
    {

        ProxyServiceClient proxyServiceClient = new ProxyServiceClient();



        public List<JCContract> GetAllContracts()
        {
            Console.WriteLine($"[RoutingWithBikes] SOAP Call from Proxy getting all contract");
            return proxyServiceClient.GetAllContracts().ToList();
        }

        public List<Station> GetStationsForContract(string contract)
        {   
            Console.WriteLine($"[RoutingWithBikes] SOAP Call from Proxy getting stations from contract: {contract}");
            return proxyServiceClient.GetStationsForContract(contract).ToList();
        }

    }
}
