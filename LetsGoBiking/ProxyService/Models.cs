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
        [JsonProperty("number")]
        public int Number { get; set; }
        [JsonProperty("contractName")]
        public string ContractName { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("adress")]
        public string Address { get; set; }
        [JsonProperty("position")]
        public Position Position { get; set; }
        [JsonProperty("banking")]
        public bool Banking { get; set; }
        [JsonProperty("bonus")]
        public bool Bonus { get; set; }
        [JsonProperty("overflow")]
        public bool Overflow { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("connected")]
        public bool Connected { get; set; }
        [JsonProperty("totalStands")]
        public Stand TotalStands { get; set; }
        [JsonProperty("mainStands")]
        public Stand MainStands { get; set; }
        [JsonProperty("overflowStands")]
        public Stand OverflowStands { get; set; }
        [JsonProperty("lastUpdate")]
        public DateTime LastUpdate { get; set; }
        public override string ToString()
        {
            int vélosMécaniques = TotalStands?.Availabilities?.MechanicalBikes ?? 0;
            int placesDisponibles = TotalStands?.Availabilities?.Stands ?? 0;

            return $"[Station {Number} - {Name} ({ContractName})] Vélos: {vélosMécaniques}, Places: {placesDisponibles}";
        }

    }

    public class JCContract : ICacheableItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("commercial_name")]
        public string Commercial_Name { get; set; }
        [JsonProperty("country_code")]
        public string Country_Code { get; set; }
        [JsonProperty("cities")]
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
