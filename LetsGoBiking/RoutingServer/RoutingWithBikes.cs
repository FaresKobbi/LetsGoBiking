using RoutingServer.ProxyService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace RoutingServer
{


    public class Path
    {
        public double[] start { get; set;}
        public double[] end { get; set;}
        public List<List<double>> geometry { get; set;}
        public string profile { get; set;}
        public double duration { get; set;}
    }

    public class ItinaryResponse
    {
        public List<Path> fullPath { get; set; } 

        public ItinaryResponse() { fullPath = new List<Path>(); }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (fullPath == null || !fullPath.Any())
            {
                return "Aucun itinéraire à afficher.";
            }

            foreach (var path in fullPath)
            {
                if (path == null) continue;

                sb.AppendLine($"Profile: {path.profile ?? "Inconnu"}");
                
                if (path.geometry != null && path.geometry.Count > 0)
                {
                    List<string> pointsStrings = new List<string>();
                    foreach (var p in path.geometry)
                    {
                        if (p != null)
                        {
                            pointsStrings.Add($"[{string.Join(", ", p)}]");
                        }
                    }
                    sb.AppendLine($"[{string.Join(", ", pointsStrings)}]");
                }
                else
                {
                    sb.AppendLine("[] (Aucune géométrie)");
                }
                sb.AppendLine("\n--------------------------------------\n");
            }
            return sb.ToString();

        }
    }



    public class RoutingWithBikes : IRoutingWithBikesSOAP, IRoutingWithBikesREST
    {

        ProxyServiceClient proxyServiceClient = new ProxyServiceClient();
        private readonly ORS orsClient = new ORS();

        public List<JCContract> GetAllContracts()
        {
            Console.WriteLine($"[RoutingWithBikes] SOAP Call from Proxy getting all contract");
            return proxyServiceClient.GetAllContracts().ToList();
        }
        public List<Station> GetStationsForContract(string contract)
        {   
            Console.WriteLine($"[RoutingWithBikes] SOAP Call from Proxy getting stations from contract: {contract}");
            return proxyServiceClient.GetStationsForContract(contract).ToList();
        }



        public ItinaryResponse GetBestRoute(string start, string end)
        {
            GeoCodeResponse startCord = orsClient.GetCoordinates(start);
            GeoCodeResponse endCord = orsClient.GetCoordinates(end);

            var startFeature = startCord?.Features?.FirstOrDefault(f => f?.Geometry?.Coordinates != null);
            if (startFeature == null)
            {
                Console.WriteLine($"❌ Adresse de départ introuvable : {start}");
                return null;
            }
            double[] startCoordDouble = startFeature.Geometry.Coordinates.ToArray();

            // Vérification de sécurité pour le point d'ARRIVÉE
            var endFeature = endCord?.Features?.FirstOrDefault(f => f?.Geometry?.Coordinates != null);
            if (endFeature == null)
            {
                Console.WriteLine($"❌ Adresse d'arrivée introuvable : {end}");
                return null;
            }
            double[] endCoordDouble = endFeature.Geometry.Coordinates.ToArray();

            RouteResponse walkingRoute = orsClient.GetRoute(
                "foot-walking",
                CoordsToString(startCoordDouble),
                CoordsToString(endCoordDouble)
            );
            

            List<JCContract> jCContracts = GetAllContracts();

            var coordinates = walkingRoute?.Features
                .FirstOrDefault()
                ?.Geometry
                ?.Coordinates
                ?.Select(coord => new { Latitude = coord[1], Longitude = coord[0] })
                .ToList();

            if (coordinates == null || coordinates.Count == 0)
            {
                Console.WriteLine("⚠️ Aucune coordonnée trouvée dans la route !");
                return null;
            }

            /*
            Console.WriteLine("🗺️ Liste complète des points de la route :");
            int i = 1;
            foreach (var point in coordinates) {
                Console.WriteLine($"{i++:D3}: Lat={point.Latitude}, Lon={point.Longitude}");
            }*/

            JCContract contractA = findContract(startCord, jCContracts);
            JCContract contractB = findContract(endCord, jCContracts);

            if (contractA.Name == contractB.Name) {
                return ComputeSingleContractRoute(contractA, startCoordDouble, endCoordDouble, walkingRoute);
            }


            return null;
        }


        private ItinaryResponse ComputeSingleContractRoute(JCContract contract, double[] startCoord /*lon lat*/, double[] endCoord /*lon lat*/, RouteResponse walkingRoute)
        {
            List<Station> allStationsForContract = GetStationsForContract(contract.Name);

            Station startStation = FindClosestStation(startCoord, allStationsForContract, true);
            Station endStation = FindClosestStation(endCoord,allStationsForContract, false);

            if (startStation == null || endStation == null) {
                Console.WriteLine("No available stations for single contract route");
                return BuildFullWalkingItinary(startCoord,endCoord,walkingRoute.Features.FirstOrDefault());
            }


            //Ici on inverse (JCDecaux lat,lon mais ORS lon,lat}
            double[] doubleStartStation = { startStation.Position.Longitude, startStation.Position.Latitude };
            String startStationCoord = CoordsToString(doubleStartStation);

            double[] doubleEndStation =  { endStation.Position.Longitude, endStation.Position.Latitude};
            String endStationCoord = CoordsToString(doubleEndStation);

            RouteResponse walk_To_StartStation = orsClient.GetRoute("foot-walking", 
                                                                    CoordsToString(startCoord),
                                                                    startStationCoord);

            RouteResponse startStation_To_EndStation = orsClient.GetRoute("cycling-regular",
                                                                          startStationCoord,
                                                                          endStationCoord);

            RouteResponse endStation_To_walk = orsClient.GetRoute("foot-walking", 
                                                                  endStationCoord, 
                                                                  CoordsToString(endCoord));

            RouteFeature walk_To_StartStation_Feature = walk_To_StartStation?.Features.FirstOrDefault();
            RouteFeature startStation_To_EndStation_Feature = walk_To_StartStation?.Features.FirstOrDefault();
            RouteFeature endStation_To_walk_Feature = endStation_To_walk.Features?.FirstOrDefault();

            if(walk_To_StartStation_Feature == null || 
               startStation_To_EndStation_Feature == null ||
               endStation_To_walk_Feature == null)
            {
                Console.WriteLine("️Une des étapes de l'itinéraire calculé est invalide.");
                return BuildFullWalkingItinary(startCoord,endCoord,walkingRoute.Features.FirstOrDefault());
            }


            double totalRouteDuration = walk_To_StartStation_Feature.Properties.Summary.DurationInSeconds 
                                      + startStation_To_EndStation_Feature.Properties.Summary.DurationInSeconds
                                      + endStation_To_walk_Feature.Properties.Summary.DurationInSeconds;

            double walkingRouteDuration = walkingRoute.Features.FirstOrDefault().Properties.Summary.DurationInSeconds;
            if(walkingRouteDuration < totalRouteDuration)
            {   
                return BuildFullWalkingItinary(startCoord,endCoord,walkingRoute.Features.FirstOrDefault());
            }
            ItinaryResponse response = new ItinaryResponse();

            response.fullPath.Add(BuildPath(startCoord, doubleStartStation, walk_To_StartStation_Feature, "foot-walking"));
            response.fullPath.Add(BuildPath(doubleStartStation, doubleEndStation, startStation_To_EndStation_Feature, "cycling-regular"));
            response.fullPath.Add(BuildPath(doubleEndStation, endCoord, endStation_To_walk_Feature, "foot-walking"));
            
            //MAY BE A PROBLEM WITH EH PATH BUILDING
            Console.WriteLine(response);

            return response;
        }


        private ItinaryResponse BuildFullWalkingItinary(double[] start, double[] end, RouteFeature walkingRoute)
        {
            ItinaryResponse walkingItinary = new ItinaryResponse();
            walkingItinary.fullPath.Add(BuildPath(start, end, walkingRoute, "foot-walking"));
            return walkingItinary;
        }

        private Path BuildPath(double[] start, double[] end, RouteFeature route, string profile)
        {
            Path path = new Path
            {
                start = start,
                end = end,
                geometry = route.Geometry.Coordinates,
                profile = profile
            };
            return path;
        }

        private JCContract findContract(GeoCodeResponse address, List<JCContract> jCContracts)
        {
            foreach (GeoCodeFeature feature in address.Features)
            {
                if (feature.Properties == null)
                {
                    continue;
                }
                string city = feature.Properties.Locality;
                if (city == null)
                {
                    continue;
                }
                var contract = jCContracts.FirstOrDefault(c =>
                (c.Name != null && c.Name.Equals(city, StringComparison.OrdinalIgnoreCase)) ||
                (c.Commercial_Name != null && c.Commercial_Name.Equals(city, StringComparison.OrdinalIgnoreCase)) ||
                (c.Cities != null && c.Cities.Any(listCity => listCity != null && listCity.Equals(city, StringComparison.OrdinalIgnoreCase)))
                );
                if (contract != null)
                {
                    return contract;
                }
            }

            return null;
        }

        private Station FindClosestStation(double[] coords, List<Station> stations, bool findWithBikes)
        {
            
            double lat = coords[1]; // ORS is [Lng, Lat]
            double lon = coords[0];

            Station station = stations
                .Where(s =>
                    s.Status == "OPEN" &&
                    s.TotalStands != null &&
                    s.TotalStands.Availabilities != null &&
                    (findWithBikes
                        ? s.TotalStands.Availabilities.MechanicalBikes > 0
                        : s.TotalStands.Availabilities.Stands > 0)
                )
                .OrderBy(s => Haversine(lat, lon, s.Position.Latitude, s.Position.Longitude))
                .FirstOrDefault();

            return station;         
        }

        //FOR DEBUGGING²
        private void PrintStation(Station station)
        {
            if (station == null)
            {
                Console.WriteLine("⚠️ La station est null.");
                return;
            }

            Console.WriteLine($"\n=== 🔎 DEBUG STATION {station.Number} ===");
            Console.WriteLine($"Name: {station.Name ?? "null"}");
            Console.WriteLine($"ContractName: {station.ContractName ?? "null"}");
            Console.WriteLine($"Address: {station.Address ?? "null"}");
            Console.WriteLine($"Status: {station.Status ?? "null"}");

            Console.WriteLine($"LastUpdate: {station.LastUpdate}");

            Console.WriteLine($"Connected: {station.Connected}");
            Console.WriteLine($"Banking: {station.Banking}");
            Console.WriteLine($"Bonus: {station.Bonus}");
            Console.WriteLine($"Overflow: {station.Overflow}");

            if (station.Position == null)
                Console.WriteLine("Position: null");
            else
                Console.WriteLine($"Position: [Lat: {station.Position.Latitude}, Lon: {station.Position.Longitude}]");

            void PrintStandDetails(string label, Stand s)
            {
                if (s == null)
                {
                    Console.WriteLine($"{label}: null");
                    return;
                }
                Console.WriteLine($"{label}: Capacity={s.Capacity}");

                if (s.Availabilities == null)
                {
                    Console.WriteLine($"  -> {label}.Availabilities: null");
                }
                else
                {
                    Console.WriteLine($"  -> Availabilities: Bikes={s.Availabilities.Bikes}, " +
                                      $"Stands={s.Availabilities.Stands}, " +
                                      $"Meca={s.Availabilities.MechanicalBikes}, " +
                                      $"Elec={s.Availabilities.ElectricalBikes}");
                }
            }

            PrintStandDetails("TotalStands", station.TotalStands);
            PrintStandDetails("MainStands", station.MainStands);
            PrintStandDetails("OverflowStands", station.OverflowStands);
            Console.WriteLine("==================================\n");
        }
        private double ToRadians(double angle) => (Math.PI / 180) * angle;

        private double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; // Radius of Earth in km
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
            return R * c;
        }

        private string CoordsToString(double[] coords) => $"{coords[0].ToString(CultureInfo.InvariantCulture)},{coords[1].ToString(CultureInfo.InvariantCulture)}";

    }
}
