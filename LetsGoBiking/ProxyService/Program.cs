using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace ProxyService
{
    public class Program
    {

        static void Main(string[] args)
        {
            Uri baseAddress = new Uri("http://localhost:7001/Proxy/");
            ServiceHost host = null;
            //host.AddServiceEndpoint(typeof(ProxyService),new BasicHttpBinding(),"");

            //ServiceMetadataBehavior smb = new ServiceMetadataBehavior { HttpGetEnabled = true };
            //host.Description.Behaviors.Add(smb);

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
