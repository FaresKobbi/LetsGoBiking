using RoutingServer.ProxyService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RoutingServer
{
    public class RoutingWithBikes : IRoutingWithBikesSOAP, IRoutingWithBikesREST
    {

        ProxyServiceClient proxyServiceClient = new ProxyServiceClient();



        public List<JCContract> GetAllContracts()
        {
            Console.WriteLine($"[RoutingWithBikes] SOAP Call from Proxy getting all contract");
            return proxyServiceClient.GetAllContracts().ToList();
        }

        public string GetBestRoute(string start, string end)
        {
            // 1️⃣ Géocodage
            GeoCodeResponse startCord = proxyServiceClient.GetCoordinates(start, 1440);
            GeoCodeResponse endCord = proxyServiceClient.GetCoordinates(end, 1440);

            double[] startCordDouble = startCord.Features.FirstOrDefault(f => f?.Geometry?.Coordinates != null).Geometry.Coordinates.ToArray();
            double[] endCordDouble = endCord.Features.FirstOrDefault(f => f?.Geometry?.Coordinates != null).Geometry.Coordinates.ToArray();

            // 2️⃣ Récupération de l'itinéraire
            RouteResponse route = proxyServiceClient.GetRoute(
                "foot-walking",
                CoordsToString(startCordDouble),
                CoordsToString(endCordDouble),
                1440
            );

            // 3️⃣ Extraction et affichage des coordonnées
            var coordinates = route.Features
                .FirstOrDefault()
                ?.Geometry
                ?.Coordinates
                ?.Select(coord => new { Latitude = coord[1], Longitude = coord[0] })
                .ToList();

            if (coordinates == null || coordinates.Count == 0)
            {
                Console.WriteLine("⚠️ Aucune coordonnée trouvée dans la route !");
                return "No coordinates found";
            }

            Console.WriteLine("🗺️ Liste complète des points de la route :");
            int i = 1;
            foreach (var point in coordinates)
            {
                Console.WriteLine($"{i++:D3}: Lat={point.Latitude}, Lon={point.Longitude}");
            }

            // 4️⃣ Tu peux aussi afficher la durée
            double duration = route.Features.FirstOrDefault()?.Properties?.Summary?.DurationInSeconds ?? 0;
            Console.WriteLine($"\n⏱️ Durée estimée : {duration / 60:F2} minutes");

            return "Success";
        }


        public List<Station> GetStationsForContract(string contract)
        {   
            Console.WriteLine($"[RoutingWithBikes] SOAP Call from Proxy getting stations from contract: {contract}");
            return proxyServiceClient.GetStationsForContract(contract).ToList();
        }

        private string CoordsToString(double[] coords) => $"{coords[0].ToString(CultureInfo.InvariantCulture)},{coords[1].ToString(CultureInfo.InvariantCulture)}";

    }
}
