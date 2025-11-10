using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Web;

namespace RoutingServer
{
    [ServiceContract]
    public interface IRoutingWithBikesREST
    {

        //Chercher si ResponseFormat est une information que je donne à mon client leger. Auquel cas vori si je peux
        //donner un responseformat de type une class dans la quelle on parse la réponde reçu du call au proxy et service externe

        [OperationContract]
        [WebGet(UriTemplate = "/route?start={start}&end={end}", ResponseFormat = WebMessageFormat.Json)]
        string GetBestRoute(string start, string end);


        [OperationContract]
        [WebGet(UriTemplate ="/stations?contract={contract}", ResponseFormat = WebMessageFormat.Json)]
        string GetStations(string contract);

    }
}
