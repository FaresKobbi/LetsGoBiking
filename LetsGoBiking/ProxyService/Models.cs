using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ProxyService.Models
{
    [DataContract]
    public class Position
    {
        [DataMember]
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [DataMember]
        [JsonProperty("longitude")]
        public double Longitude { get; set; }
    }

    public class Availabilities
    {
        [JsonProperty("bikes")]
        public int Bikes { get; set; }

        [JsonProperty("stands")]
        public int Stands { get; set; }

        [JsonProperty("mechanicalBikes")]
        public int MechanicalBikes { get; set; }

        [JsonProperty("electricalBikes")]
        public int ElectricalBikes { get; set; }

        [JsonProperty("electricalInternalBatteryBikes")]
        public int ElectricalInternalBatteryBikes { get; set; }

        [JsonProperty("electricalRemovableBatteryBikes")]
        public int ElectricalRemovableBatteryBikes { get; set; }
    }

    public class Stand
    {
        [JsonProperty("availabilities")]
        public Availabilities Availabilities { get; set; }

        [JsonProperty("capacity")]
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

        [JsonProperty("address")]
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
}