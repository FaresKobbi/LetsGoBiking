using HeavyClient.RoutingWithBikesSOAP;
using System;
using System.Linq;

namespace HeavyClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== HEAVY CLIENT - TESTING ROUTING SERVER ===");

            try
            {
                var client = new RoutingWithBikesSOAPClient();
                Console.WriteLine("→ Connected to: " + client.Endpoint.Address);

                while (true)
                {
                    Console.WriteLine("\n------------------------------------------------");
                    Console.Write("📍 Entrez l'adresse de DÉPART (ou 'exit'): ");
                    string start = Console.ReadLine();
                    if (start.ToLower() == "exit") break;

                    Console.Write("🏁 Entrez l'adresse d'ARRIVÉE: ");
                    string end = Console.ReadLine();

                    Console.WriteLine("⏳ Calcul de l'itinéraire en cours...");

                    // Appel SOAP
                    RouteDetails route = client.SOAP_GetBestRoute(start, end);

                    if (route == null)
                    {
                        Console.WriteLine("❌ Aucun itinéraire trouvé ou erreur serveur.");
                        continue;
                    }

                    // Affichage des résultats
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n✅ ITINÉRAIRE TROUVÉ !");
                    Console.WriteLine($"👉 Type: {route.RouteType}");
                    Console.WriteLine($"⏱️ Durée Totale: {route.TotalDuration} secondes ({(route.TotalDuration / 60):F1} min)");
                    Console.ResetColor();

                    Console.WriteLine("\n📜 Étapes :");
                    foreach (var segment in route.Segments)
                    {
                        string icon = segment.Mode == "cycling-regular" ? "🚴" : "🚶";
                        Console.WriteLine($"   {icon} {segment.Mode.ToUpper()} : De [{segment.StartName}] vers [{segment.EndName}] ({segment.Duration}s)");
                    }
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
