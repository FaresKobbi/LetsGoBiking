using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    [ServiceContract]
    public interface IProxyJCDecaux
    {
            [OperationContract]
            string GetStations(string contract);

    }
}
