using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace ProxyService.Models
{
    [DataContract]

    public class Position
    {
        [DataMember]
        public double Latitude { get; set; }

        [DataMember]
        public double Longitude { get; set; }

    }

    //JCDECAUX


    public class Availabilities
    {
        public int Bikes { get; set; }
        public int Stands { get; set; }
        public int MechanicalBikes { get; set; }
        public int ElectricalBikes { get; set; }
        public int ElectricalInternalBatteryBikes { get; set; }
        public int ElectricalRemovableBatteryBikes { get; set; }
    }

    public class Stand
    {
        public Availabilities Availabilities { get; set; }
        public int Capacity { get; set; }
    }

    public class Station : ICacheableItem
    {
        public int Number { get; set; }
        public string ContractName { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public Position Position { get; set; }
        public bool Banking { get; set; }
        public bool Bonus { get; set; }
        public bool Overflow { get; set; }
        public string Status { get; set; }
        public bool Connected { get; set; }
        public Stand TotalStands { get; set; }
        public Stand MainStands { get; set; }
        public Stand OverflowStands { get; set; }
        public DateTime LastUpdate { get; set; }

    }

    public class JCContract : ICacheableItem
    {
        public string Name { get; set; }
        public string Commercial_Name { get; set; }
        public string Country_Code { get; set; }
        public List<string> Cities { get; set; }

    }


    //ORS
    public class GeoCodeProperties
    {
        [JsonProperty("locality")]
        public string Locality { get; set; }     }

    public class GeoCodeResponse : ICacheableItem
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

    public class RouteResponse : ICacheableItem
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
}
