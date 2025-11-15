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

        public RouteDetails GetBestRoute(string start, string end)
        {
            GeoCodeResponse startGeo = orsClient.GetCoordinates(start); 
            GeoCodeResponse endGeo = orsClient.GetCoordinates(end);

            var startFeature = startGeo.Features.FirstOrDefault(f => f?.Geometry?.Coordinates != null);
            var endFeature = endGeo.Features.FirstOrDefault(f => f?.Geometry?.Coordinates != null);

            double[] startCoords = startFeature?.Geometry.Coordinates.ToArray();
            double[] endCoords = endFeature?.Geometry.Coordinates.ToArray();

            string startCoordsStr = CoordsToString(startCoords);
            string endCoordsStr = CoordsToString(endCoords);

            string startCity = startFeature?.Properties?.Locality;
            string endCity = endFeature?.Properties?.Locality;

            RouteDetails bestRoute = GetBaseWalkRoute(start, end, startCoordsStr, endCoordsStr);


            StationDataContainer stationData = PrepareStationData(startCoords, endCoords, startCity, endCity);

            if (stationData == null){
                return bestRoute;
            }

            bestRoute = RunRoutingTournament(bestRoute, stationData, start, end, startCoords, endCoords);

            // 5. RETOUR DU GAGNANT
            Console.WriteLine($"[GetBestRoute] Itinéraire final choisi: {bestRoute.RouteType} ({bestRoute.TotalDuration}s)");
            return bestRoute;

        }

        private class StationDataContainer
        {
            public Station ContractA { get; set; }
            public Station ContractB { get; set; }
            public List<Station> ViableAStart { get; set; }
            public List<Station> ViableAEnd { get; set; }
            public List<Station> ViableBStart { get; set; }
            public List<Station> ViableBEnd { get; set; }
        }

        private RouteDetails GetBaseWalkRoute(string start, string end, string startCoordsStr, string endCoordsStr)
        {
            RouteResponse walkOnlyRoute = orsClient.GetRoute("foot-walking", startCoordsStr, endCoordsStr); 
            RouteFeature walkFeatures = walkOnlyRoute.Features.FirstOrDefault();

            if (walkFeatures == null) throw new Exception("Aucune 'feature' retournée pour la route à pied.");

            return new RouteDetails
            {
                RouteType = "WALK",
                TotalDuration = walkFeatures.Properties.Summary.DurationInSeconds, 
                Segments = new List<RouteSegment>
                {
                    new RouteSegment
                    {
                        Mode = "foot-walking",
                        StartName = start,
                        EndName = end,
                        Duration = walkFeatures.Properties.Summary.DurationInSeconds,
                        Geometry = walkFeatures.Geometry.Coordinates 
                    }
                }
            };
        }

        private StationDataContainer PrepareStationData(double[] startCoords, double[] endCoords, string startCity, string endCity)
        {
            // 3. RÉCUPÉRATION
            List<JCContract> allContracts = proxyServiceClient.GetAllContracts().ToList();
            var relevantContracts = new List<JCContract>();
            string normStartCity = startCity?.Trim().ToLower();
            string normEndCity = endCity?.Trim().ToLower();

            foreach (var contract in allContracts)
            {
                bool matchStart = IsContractRelevant(contract, normStartCity);
                bool matchEnd = IsContractRelevant(contract, normEndCity);

                if (matchStart || matchEnd)
                {
                    relevantContracts.Add(contract);
                }
            }

            if (relevantContracts.Count == 0)
            {
                Console.WriteLine("[PrepareStationData] Aucun contrat JCDecaux ne correspond aux villes détectées.");
                return null;
            }

            List<Station> allStations = new List<Station>();
            foreach (var contract in relevantContracts.Distinct()){            
                Console.WriteLine($"[PrepareStationData] Chargement des stations pour le contrat : {contract.Name}");
                allStations.AddRange(proxyServiceClient.GetStationsForContract(contract.Name));
            }

            Console.WriteLine($"[PrepareStationData] {allStations.Count} stations totales récupérées.");

            // 4. IDENTIFICATION
            Station closestStationToA = allStations.Where(s => s?.Position != null)
                .OrderBy(s => (s.Position.Latitude - startCoords[1]) * (s.Position.Latitude - startCoords[1]) + (s.Position.Longitude - startCoords[0]) * (s.Position.Longitude - startCoords[0]))
                .FirstOrDefault();

            Station closestStationToB = allStations.Where(s => s?.Position != null)
                .OrderBy(s => (s.Position.Latitude - endCoords[1]) * (s.Position.Latitude - endCoords[1]) + (s.Position.Longitude - endCoords[0]) * (s.Position.Longitude - endCoords[0]))
                .FirstOrDefault();

            if (closestStationToA == null || closestStationToB == null)
            {
                Console.WriteLine("[PrepareStationData] Aucune station JCDecaux trouvée.");
                return null;
            }

            string contractA = closestStationToA.ContractName;
            string contractB = closestStationToB.ContractName;
            Console.WriteLine($"[PrepareStationData] Contrat A: {closestStationToA.ToString()}, Contrat B: {contractB}");

            // 5. FILTRAGE
            return new StationDataContainer
            {
                ContractA = closestStationToA,
                ContractB = closestStationToB,
                ViableAStart = allStations.Where(s => s.ContractName == contractA && s.Status == "OPEN" && s.TotalStands?.Availabilities?.MechanicalBikes > 0).ToList(),
                ViableAEnd = allStations.Where(s => s.ContractName == contractA && s.Status == "OPEN" && s.TotalStands?.Availabilities?.Stands > 0).ToList(),
                ViableBStart = allStations.Where(s => s.ContractName == contractB && s.Status == "OPEN" && s.TotalStands?.Availabilities?.MechanicalBikes > 0).ToList(),
                ViableBEnd = allStations.Where(s => s.ContractName == contractB && s.Status == "OPEN" && s.TotalStands?.Availabilities?.Stands > 0).ToList()
            };
        }


        /// <summary>
        /// Étape 6: Gère la logique de comparaison des itinéraires (le "Tournoi").
        /// </summary>
        private RouteDetails RunRoutingTournament(RouteDetails bestRoute, StationDataContainer stationData, string start, string end, double[] startCoords, double[] endCoords)
        {

            {

                List<RouteDetails> challengers = new List<RouteDetails>();

                //GROS PROBLEME ICI TOUJOURS CE CAS CAR IL N'Y A JAMAIS DE CONCTRACTNAME
                if (stationData.ContractA.ContractName == stationData.ContractB.ContractName)
                {
                    Console.WriteLine("[RunRoutingTournament] Cas: Même Contrat");
                    // On cherche une station de départ dans A (avec vélos) et une d'arrivée dans A (avec places)
                    var c1 = EvaluateSingleBikeLegRoute(
                        "BIKE_SINGLE_CONTRACT",
                        start, end, startCoords, endCoords,
                        stationData.ViableAStart, stationData.ViableAEnd // Note: ViableAEnd car A=B
                    );
                    if (c1 != null) challengers.Add(c1);
                }
                else
                {
                    Console.WriteLine("[RunRoutingTournament] Cas: Contrats Différents");

                    // --- Hybride A (Vélo A -> Marche jusqu'à B) ---
                    // On prend un vélo dans A, on le pose dans A (mais le plus proche possible de B)
                    var c2 = EvaluateSingleBikeLegRoute(
                        "HYBRID_BIKE_A",
                        start, end, startCoords, endCoords,
                        stationData.ViableAStart, stationData.ViableAEnd
                    );
                    if (c2 != null) challengers.Add(c2);

                    // --- Hybride B (Marche depuis A -> Vélo B) ---
                    // On prend un vélo dans B (le plus proche de A), on le pose dans B
                    var c3 = EvaluateSingleBikeLegRoute(
                        "HYBRID_WALK_B",
                        start, end, startCoords, endCoords,
                        stationData.ViableBStart, stationData.ViableBEnd
                    );
                    if (c3 != null) challengers.Add(c3);

                    // --- Multi (Vélo A -> Transfert -> Vélo B) ---
                    // Celui-ci reste complexe, on garde sa méthode dédiée
                    var c4 = EvaluateMultiContractRoute(
                        start, end, startCoords, endCoords,
                        stationData.ViableAStart, stationData.ViableAEnd,
                        stationData.ViableBStart, stationData.ViableBEnd
                    );
                    if (c4 != null) challengers.Add(c4);
                }

                // LE VERDICT
                foreach (var challenger in challengers)
                {
                    // Console.WriteLine($"[RunRoutingTournament] Candidat {challenger.RouteType}: {challenger.TotalDuration}s");
                    if (challenger.TotalDuration < bestRoute.TotalDuration)
                    {
                        bestRoute = challenger;
                    }
                }

                return bestRoute;
            }
        } 



        /// <summary>
        /// Trouve la meilleure station parmi une liste, en minimisant le temps de marche.
        /// Optimisation: Pré-filtre géométriquement pour limiter les appels API ORS.
        /// </summary>
        private (Station station, RouteResponse route) FindClosestViableStation(double[] refCoords, List<Station> viableStations)
        {
            if (viableStations == null || viableStations.Count == 0) return (null, null);

            // 1. Optimisation Géométrique (Ta formule corrigée)
            // On ne garde que les 5 stations les plus proches à vol d'oiseau pour interroger ORS
            var topCandidates = viableStations
                    .OrderBy(s =>
                        (s.Position.Latitude - refCoords[1]) * (s.Position.Latitude - refCoords[1]) +
                        (s.Position.Longitude - refCoords[0]) * (s.Position.Longitude - refCoords[0])
                    )
                    .Take(5)
                    .ToList();

            // 2. Tournoi "Temps Réel" avec ORS
            Station bestStation = null;
            RouteResponse bestRoute = null;
            double minDuration = double.MaxValue;

            string refCoordsStr = CoordsToString(refCoords);

            foreach (var station in topCandidates)
            {
                string stationCoords = PositionToString(station.Position);

                // Attention à l'ordre : De A vers Station OU De Station vers B ?
                // Pour simplifier ici, on considère que la marche est symétrique (A->S ~= S->A).
                // Pour être puriste, il faudrait passer un paramètre 'bool isArrival'.
                RouteResponse route = orsClient.GetRoute("foot-walking", refCoordsStr, stationCoords);

                if (route != null && route.Features.Count > 0)
                {
                    double duration = route.Features[0].Properties.Summary.DurationInSeconds;
                    if (duration < minDuration)
                    {
                        minDuration = duration;
                        bestStation = station;
                        bestRoute = route;
                    }
                }
            }

            return (bestStation, bestRoute);
        }

        /*
                private RouteDetails EvaluateSameContractRoute(string startName, string endName,double[] startCoords, double[] endCoords,
                    List<Station> viableStartStations, List<Station> viableEndStations)
                {
                    // 1. Trouver la station de départ (Marche -> Station A)
                    var (stationA, routeToA) = FindClosestViableStation(startCoords, viableStartStations);
                    if (stationA == null) return null;

                    // 2. Trouver la station d'arrivée (Station B -> Marche)
                    var (stationB, routeFromB) = FindClosestViableStation(endCoords, viableEndStations);
                    if (stationB == null) return null;

                    // Si c'est la même station, le vélo est inutile
                    if (stationA.Number == stationB.Number) return null;

                    // 3. Calculer le trajet VÉLO (Station A -> Station B)
                    RouteResponse bikeRoute = orsClient.GetRoute("cycling-regular", PositionToString(stationA.Position), PositionToString(stationB.Position));

                    if (bikeRoute == null || bikeRoute.Features.Count == 0) return null;

                    // 4. Assemblage du résultat final
                    double walk1 = routeToA.Features[0].Properties.Summary.DurationInSeconds;
                    double bike = bikeRoute.Features[0].Properties.Summary.DurationInSeconds;
                    double walk2 = routeFromB.Features[0].Properties.Summary.DurationInSeconds;

                    RouteDetails route = new RouteDetails
                    {
                        RouteType = "BIKE_SINGLE_CONTRACT",
                        TotalDuration = walk1 + bike + walk2,
                        Segments = new List<RouteSegment>()
                    };

                    // Segment 1: Marche vers Station A
                    route.Segments.Add(new RouteSegment
                    {
                        Mode = "foot-walking",
                        StartName = startName,
                        EndName = stationA.Name,
                        Duration = walk1,
                        Geometry = routeToA.Features[0].Geometry.Coordinates
                    });

                    // Segment 2: Vélo de A vers B
                    route.Segments.Add(new RouteSegment
                    {
                        Mode = "cycling-regular",
                        StartName = stationA.Name,
                        EndName = stationB.Name,
                        Duration = bike,
                        Geometry = bikeRoute.Features[0].Geometry.Coordinates
                    });

                    // Segment 3: Marche de Station B vers Arrivée
                    // On inverse la géométrie car ORS a calculé "Arrivée -> Station" (ou l'inverse selon l'appel),
                    // mais visuellement on veut "Station -> Arrivée".
                    var geomFromB = routeFromB.Features[0].Geometry.Coordinates;
                    geomFromB.Reverse();

                    route.Segments.Add(new RouteSegment
                    {
                        Mode = "foot-walking",
                        StartName = stationB.Name,
                        EndName = endName,
                        Duration = walk2,
                        Geometry = geomFromB
                    });

                    return route;
                }*/


        private RouteDetails EvaluateMultiContractRoute(
    string startName, string endName,
    double[] startCoords, double[] endCoords,
    List<Station> viableAStart, List<Station> viableAEnd,
    List<Station> viableBStart, List<Station> viableBEnd)
        {
            // 1. Start A et End B (Les extrémités faciles)
            var (stationA_Start, routeToA) = FindClosestViableStation(startCoords, viableAStart);
            var (stationB_End, routeFromB) = FindClosestViableStation(endCoords, viableBEnd);

            if (stationA_Start == null || stationB_End == null) return null;

            // 2. Trouver la meilleure paire de transfert (Station A End -> Station B Start)
            // OPTIMISATION : On ne teste pas tout contre tout. On cherche les stations géographiquement proches.
            // On cherche la station A_End qui est la plus proche de n'importe quelle station B_Start (et inversement).

            Station bestTransferA = null;
            Station bestTransferB = null;
            double minTransferDist = double.MaxValue;

            // Double boucle naïve optimisée par distance euclidienne simple
            // Si trop lent, on pourrait limiter aux 10 premières.
            foreach (var sA in viableAEnd)
            {
                foreach (var sB in viableBStart)
                {
                    // Distance vol d'oiseau au carré (suffisant pour comparer)
                    double distSq = Math.Pow(sA.Position.Latitude - sB.Position.Latitude, 2) +
                                    Math.Pow(sA.Position.Longitude - sB.Position.Longitude, 2);

                    if (distSq < minTransferDist)
                    {
                        minTransferDist = distSq;
                        bestTransferA = sA;
                        bestTransferB = sB;
                    }
                }
            }

            if (bestTransferA == null || bestTransferB == null) return null;

            // Calcul réel de la marche de transfert
            RouteResponse transferWalk = orsClient.GetRoute("foot-walking",
                PositionToString(bestTransferA.Position),
                PositionToString(bestTransferB.Position));

            // Calcul des 2 trajets vélo
            RouteResponse bikePathA = orsClient.GetRoute("cycling-regular",
                PositionToString(stationA_Start.Position),
                PositionToString(bestTransferA.Position));

            RouteResponse bikePathB = orsClient.GetRoute("cycling-regular",
                PositionToString(bestTransferB.Position),
                PositionToString(stationB_End.Position));

            if (transferWalk == null || bikePathA == null || bikePathB == null) return null;

            // 3. Assemblage final (5 segments !)
            double t1 = routeToA.Features[0].Properties.Summary.DurationInSeconds;
            double t2 = bikePathA.Features[0].Properties.Summary.DurationInSeconds;
            double t3 = transferWalk.Features[0].Properties.Summary.DurationInSeconds;
            double t4 = bikePathB.Features[0].Properties.Summary.DurationInSeconds;
            double t5 = routeFromB.Features[0].Properties.Summary.DurationInSeconds;

            RouteDetails route = new RouteDetails
            {
                RouteType = "MULTI_CONTRACT_BIKE",
                TotalDuration = t1 + t2 + t3 + t4 + t5,
                Segments = new List<RouteSegment>()
            };

            route.Segments.Add(new RouteSegment { Mode = "foot-walking", StartName = startName, EndName = stationA_Start.Name, Duration = t1, Geometry = routeToA.Features[0].Geometry.Coordinates });
            route.Segments.Add(new RouteSegment { Mode = "cycling-regular", StartName = stationA_Start.Name, EndName = bestTransferA.Name, Duration = t2, Geometry = bikePathA.Features[0].Geometry.Coordinates });
            route.Segments.Add(new RouteSegment { Mode = "foot-walking", StartName = bestTransferA.Name, EndName = bestTransferB.Name, Duration = t3, Geometry = transferWalk.Features[0].Geometry.Coordinates });
            route.Segments.Add(new RouteSegment { Mode = "cycling-regular", StartName = bestTransferB.Name, EndName = stationB_End.Name, Duration = t4, Geometry = bikePathB.Features[0].Geometry.Coordinates });

            var geomWalk = routeFromB.Features[0].Geometry.Coordinates;
            geomWalk.Reverse();
            route.Segments.Add(new RouteSegment { Mode = "foot-walking", StartName = stationB_End.Name, EndName = endName, Duration = t5, Geometry = geomWalk });

            return route;
        }


        /// <summary>
        /// Méthode générique pour tout trajet impliquant UN SEUL trajet à vélo (Marche -> Vélo -> Marche).
        /// Fonctionne pour le cas "Même Contrat", "Hybride A" et "Hybride B".
        /// </summary>
        private RouteDetails EvaluateSingleBikeLegRoute(
            string routeTypeLabel, // Le nom qu'on veut donner au trajet (ex: "HYBRID_BIKE_A")
            string startName, string endName,
            double[] startCoords, double[] endCoords,
            List<Station> startCandidates, // Dans quelle liste chercher la station de départ ?
            List<Station> endCandidates)   // Dans quelle liste chercher la station d'arrivée ?
        {
            // 1. Trouver la meilleure station de départ (Proche de A)
            var (stationStart, routeToStart) = FindClosestViableStation(startCoords, startCandidates);
            if (stationStart == null) return null;

            // 2. Trouver la meilleure station de fin (Proche de B)
            var (stationEnd, routeFromEnd) = FindClosestViableStation(endCoords, endCandidates);
            if (stationEnd == null) return null;

            // Vérif anti-tourniquet (prendre et poser le vélo au même endroit)
            if (stationStart.Number == stationEnd.Number) return null;

            // 3. Trajet Vélo
            RouteResponse bikeRoute = orsClient.GetRoute("cycling-regular",
                PositionToString(stationStart.Position),
                PositionToString(stationEnd.Position));

            if (bikeRoute == null || bikeRoute.Features.Count == 0) return null;

            // 4. Assemblage
            double t1 = routeToStart.Features[0].Properties.Summary.DurationInSeconds;
            double t2 = bikeRoute.Features[0].Properties.Summary.DurationInSeconds;
            double t3 = routeFromEnd.Features[0].Properties.Summary.DurationInSeconds;

            RouteDetails route = new RouteDetails
            {
                RouteType = routeTypeLabel,
                TotalDuration = t1 + t2 + t3,
                Segments = new List<RouteSegment>()
            };

            // Segment 1 : Marche -> Station Start
            route.Segments.Add(new RouteSegment
            {
                Mode = "foot-walking",
                StartName = startName,
                EndName = stationStart.Name,
                Duration = t1,
                Geometry = routeToStart.Features[0].Geometry.Coordinates
            });

            // Segment 2 : Vélo Station Start -> Station End
            route.Segments.Add(new RouteSegment
            {
                Mode = "cycling-regular",
                StartName = stationStart.Name,
                EndName = stationEnd.Name,
                Duration = t2,
                Geometry = bikeRoute.Features[0].Geometry.Coordinates
            });

            // Segment 3 : Marche Station End -> Arrivée
            // (On inverse la géométrie car ORS a calculé Arrivée -> Station pour trouver la plus proche)
            var geomWalk = routeFromEnd.Features[0].Geometry.Coordinates;
            geomWalk.Reverse();
            route.Segments.Add(new RouteSegment
            {
                Mode = "foot-walking",
                StartName = stationEnd.Name,
                EndName = endName,
                Duration = t3,
                Geometry = geomWalk
            });

            return route;
        }

        private bool IsContractRelevant(JCContract contract, string normalizedCityName)
        {
            if (string.IsNullOrEmpty(normalizedCityName)) return false;

            // Vérif Nom du contrat
            if (contract.Name.ToLower().Contains(normalizedCityName)) return true;

            // Vérif Nom commercial (ex: Vélo'v)
            if (contract.Commercial_Name != null && contract.Commercial_Name.ToLower().Contains(normalizedCityName)) return true;

            // Vérif Liste des villes associées
            if (contract.Cities != null)
            {
                foreach (var city in contract.Cities)
                {
                    if (city.ToLower().Contains(normalizedCityName)) return true;
                }
            }

            return false;
        }


        private string PositionToString(Position pos)
        {
            // ORS_Coords = "longitude,latitude"
            return $"{pos.Longitude.ToString(CultureInfo.InvariantCulture)},{pos.Latitude.ToString(CultureInfo.InvariantCulture)}";
        }
        private string CoordsToString(double[] coords) => $"{coords[0].ToString(CultureInfo.InvariantCulture)},{coords[1].ToString(CultureInfo.InvariantCulture)}";

    }
}
