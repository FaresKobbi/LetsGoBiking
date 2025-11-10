using HeavyClient.RoutingWithBikesSOAP;
using System;

namespace HeavyClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                RoutingWithBikesSOAPClient routingWithBikesSOAPClient = new RoutingWithBikesSOAPClient();  


                Console.WriteLine(routingWithBikesSOAPClient.SOAP_GetAllContracts());

                Console.WriteLine(routingWithBikesSOAPClient.SOAP_GetAllContracts());

                Console.WriteLine(routingWithBikesSOAPClient.SOAP_GetStationsForContract("lyon"));

                Console.WriteLine(routingWithBikesSOAPClient.SOAP_GetStationsForContract("lyon"));

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
            }
        }
    }
}
