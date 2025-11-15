using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RoutingServer
{
        //ORS
        public class GeoCodeProperties
        {
            [JsonProperty("locality")]
            public string Locality { get; set; }
        }

        public class GeoCodeResponse 
        {
            [JsonProperty("features")]
            public List<GeoCodeFeature> Features { get; set; } = new List<GeoCodeFeature>();
        }
        public class GeoCodeFeature
        {
            [JsonProperty("geometry")]
            public GeoCodeGeometry Geometry { get; set; }

            [JsonProperty("properties")]
            public GeoCodeProperties Properties { get; set; }
        }
        public class GeoCodeGeometry
        {
            [JsonProperty("coordinates")]
            public List<double> Coordinates { get; set; }
        }

        public class RouteResponse
        {
            [JsonProperty("features")]
            public List<RouteFeature> Features { get; set; } = new List<RouteFeature>();

            public string CacheKey => throw new NotImplementedException();
        }
        public class RouteFeature
        {
            [JsonProperty("geometry")]
            public RouteGeometry Geometry { get; set; }
            [JsonProperty("properties")]
            public RouteProperties Properties { get; set; }
        }
        public class RouteGeometry
        {
            // [[lng, lat], [lng, lat], ...]
            [JsonProperty("coordinates")]
            public List<List<double>> Coordinates { get; set; }
        }
        public class RouteProperties
        {
            [JsonProperty("summary")]
            public RouteSummary Summary { get; set; }
        }
        public class RouteSummary
        {
            [JsonProperty("duration")]
            public double DurationInSeconds { get; set; }
        }

    //ROUTE

    [DataContract] 
    public class RouteDetails
    {
        [DataMember]
        public string RouteType { get; set; } 

        [DataMember]
        public double TotalDuration { get; set; } 

        [DataMember]
        public List<RouteSegment> Segments { get; set; } = new List<RouteSegment>();
    }

    [DataContract]
    public class RouteSegment
    {
        [DataMember]
        public string Mode { get; set; } 

        [DataMember]
        public string StartName { get; set; } 

        [DataMember]
        public string EndName { get; set; } 

        [DataMember]
        public double Duration { get; set; }

        [DataMember]
        public List<List<double>> Geometry { get; set; }
    }
}
