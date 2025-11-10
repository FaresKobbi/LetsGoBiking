using System;
using System.ServiceModel;

namespace ProxyService
{
    public class Program
    {

        static void Main(string[] args)
        {
            Uri baseAddress = new Uri("http://localhost:7001/Proxy/");
            ServiceHost host = null;

            try
            {
                host =  new ServiceHost(typeof(ProxyService));

                host.Open();

                Console.WriteLine("✅ ProxyService is running...");
                Console.WriteLine("Listening on: " + baseAddress);
                Console.WriteLine("WSDL available at: " + baseAddress + "?wsdl");
                Console.WriteLine("Press <Enter> to stop the service.");
                Console.ReadLine();

                host.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error: " + ex.Message);

                Console.ReadLine();
                host.Abort();
            }

        }
    }
}
