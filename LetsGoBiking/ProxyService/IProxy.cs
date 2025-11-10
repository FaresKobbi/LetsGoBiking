using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ProxyService
{
    [ServiceContract]
    public interface IProxyService
    {
        [OperationContract]
        string GetStations(string contract);

    }

    // TODO: ajoutez vos opérations de service ici
}

