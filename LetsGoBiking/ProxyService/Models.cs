using System;
using System.Collections.Generic;

namespace ProxyService.Models
{
    public class Position
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

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

        public string CacheKey => $"station_{ContractName}_{Number}";
    }

    public class JCContract : ICacheableItem
    {
        public string Name { get; set; }
        public string Commercial_Name { get; set; }
        public string Country_Code { get; set; }
        public List<string> Cities { get; set; }

        public string CacheKey => $"contract_{Name}";
    }
}
