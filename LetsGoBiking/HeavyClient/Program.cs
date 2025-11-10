using HeavyClient.RoutingWithBikes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavyClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            RoutingWithBikesSOAPClient routingSOAP = new RoutingWithBikesSOAPClient();
            Console.WriteLine(routingSOAP.GetStations("lyon"));

            Console.WriteLine(routingSOAP.GetStations("lyon"));

            Console.WriteLine(routingSOAP.GetStations("toulouse"));

            Console.ReadLine();
        }
    }
}
