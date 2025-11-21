using RoutingServer.ProxyService;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace RoutingServer
{
    [ServiceContract]
    public interface IRoutingWithBikesREST
    {

        [OperationContract(Name = "REST_GetAllContracts")]
        [WebGet(UriTemplate = "/contracts", ResponseFormat = WebMessageFormat.Json)]
        List<JCContract> GetAllContracts();


        [OperationContract(Name = "REST_GetStationsForContract")]
        [WebGet(UriTemplate ="/stations?contract={contract}", ResponseFormat = WebMessageFormat.Json)]
        List<Station> GetStationsForContract(string contract);

        [OperationContract(Name = "REST_GetBestRoute")]
        [WebGet(UriTemplate = "/route?start={start}&end={end}", ResponseFormat = WebMessageFormat.Json)]
        ItinaryResponse GetBestRoute(string start, string end);

    }
}
