using System;
using System.ServiceModel;

namespace RoutingServer
{
    public class Program
    {
        static void Main(string[] args)
        {

            ServiceHost host = null;
            try
            {
                host = new ServiceHost(typeof(RoutingWithBikes));

                host.Open();

                Console.WriteLine("✅ RoutingWithBikes service is running.");
                Console.WriteLine("Available endpoints:");
                foreach (var ep in host.Description.Endpoints)
                {
                    Console.WriteLine($" - {ep.Address.Uri}  →  {ep.Binding.Name}");
                }

                Console.WriteLine("\nPress <Enter> to stop the service.");
                Console.ReadLine();

                host.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error: " + ex);
                Console.ReadLine ();
                host?.Abort();
            }
        }
    }
}
