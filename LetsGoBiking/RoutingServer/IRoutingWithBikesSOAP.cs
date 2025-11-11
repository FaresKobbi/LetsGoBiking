using RoutingServer.ProxyService;
using System.Collections.Generic;
using System.ServiceModel;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingServer
{
    [ServiceContract]
    public interface IRoutingWithBikesSOAP
    {
        [OperationContract(Name = "SOAP_GetAllContracts")]
        List<JCContract> GetAllContracts();

        [OperationContract(Name = "SOAP_GetStationsForContract")]
        List<Station> GetStationsForContract(string contract);

        [OperationContract(Name = "SOAP_GetBestRoute")]
        string GetBestRoute(string start, string end);
    }
}
