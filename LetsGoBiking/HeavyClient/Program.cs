using HeavyClient.RoutingWithBikesSOAP;
using System;
using System.Linq;

namespace HeavyClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var client = new RoutingWithBikesSOAPClient("BasicHttpBinding_IRoutingWithBikesSOAP");

                Console.WriteLine("→ Endpoint: " + client.Endpoint.Address);
                var contracts = client.SOAP_GetAllContracts();
                Console.WriteLine($"Contrats reçus: {contracts}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.ReadLine();
        }
    }
}
