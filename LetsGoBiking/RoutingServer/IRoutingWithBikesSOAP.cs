using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RoutingServer
{
    [ServiceContract]
    public interface IRoutingWithBikesSOAP
    {
        [OperationContract]
        string GetBestRoute(string start, string end);

        [OperationContract]
        string GetStations(string contract);
    }
}
