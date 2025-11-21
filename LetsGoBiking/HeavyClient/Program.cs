using HeavyClient.RoutingWithBikesSOAP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeavyClient
{
    internal class Program
    {
        public static void PrintItinerary(ItinaryResponse response)
        {
            // 1. Vérification de l'objet racine
            if (response == null)
            {
                Console.WriteLine("❌ L'objet itinéraire est null.");
                return;
            }

            // 2. Vérification de la liste des chemins
            if (response.fullPath == null || !response.fullPath.Any())
            {
                Console.WriteLine("⚠️ Aucun itinéraire à afficher (liste vide).");
                return;
            }

            Console.WriteLine("=== 🗺️ Détails de l'itinéraire ===");

            foreach (var path in response.fullPath)
            {
                if (path == null) continue;

                // Affichage du profil (Walk, Bike, etc.)
                Console.WriteLine($"📌 Profile: {path.profile ?? "Inconnu"}");

                // Affichage de la géométrie
                if (path.geometry != null && path.geometry.Any())
                {
                    List<string> pointsStrings = new List<string>();
                    foreach (var p in path.geometry)
                    {
                        if (p != null)
                        {
                            // Join fonctionne aussi bien sur List<double> que double[]
                            pointsStrings.Add($"[{string.Join(", ", p)}]");
                        }
                    }
                    // Affiche tous les points sur une ligne (attention si la liste est longue)
                    Console.WriteLine($"📍 Géométrie : [{string.Join(", ", pointsStrings)}]");
                }
                else
                {
                    Console.WriteLine("📍 Géométrie : [] (Aucune géométrie)");
                }

                Console.WriteLine("\n--------------------------------------\n");
            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("=== HEAVY CLIENT - TESTING ROUTING SERVER ===");

            try
            {
                var client = new RoutingWithBikesSOAPClient();
                Console.WriteLine("Connected to: " + client.Endpoint.Address);

                while (true)
                {
                    Console.WriteLine("\n------------------------------------------------");
                    Console.Write("Entrez l'adresse de DÉPART (ou 'exit'): ");
                    string start = Console.ReadLine();
                    if (start.ToLower() == "exit") break;

                    Console.Write("Entrez l'adresse d'ARRIVÉE: ");
                    string end = Console.ReadLine();

                    Console.WriteLine("Calcul de l'itinéraire en cours...");

                    // Appel SOAP
                    ItinaryResponse route = client.SOAP_GetBestRoute(start, end);
                    PrintItinerary(route);
                }

                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ ERREUR CRITIQUE : " + ex.Message);
                Console.ReadLine();
            }
        }

    }
}
