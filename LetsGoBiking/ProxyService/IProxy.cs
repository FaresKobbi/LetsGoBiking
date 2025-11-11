using ProxyService.Models;
using System.Collections.Generic;
using System.ServiceModel;

namespace ProxyService
{
    [ServiceContract]
    public interface IProxyService
    {
        [OperationContract]
        List<JCContract> GetAllContracts();

        [OperationContract]
        List<Station> GetStationsForContract(string contract);

        [OperationContract]
        GeoCodeResponse GetCoordinates(string address, int expiration);

        [OperationContract]
        RouteResponse GetRoute(string profile, string startCoords, string endCoords, int expiration);
    }

}

