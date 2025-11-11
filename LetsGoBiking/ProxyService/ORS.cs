using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProxyService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProxyService
{
    internal class ORS
    {
        private readonly string API_KEY = "eyJvcmciOiI1YjNjZTM1OTc4NTExMTAwMDFjZjYyNDgiLCJpZCI6IjU5MGNjNjlkN2RmNTQyYmRiNTJlNmQ4OTQ5YjNlMjUwIiwiaCI6Im11cm11cjY0In0="; // Remplace par ta clé
        private readonly HttpClient http = new HttpClient();
        // --- GEOCODE ---
        public GeoCodeResponse GetCoordinates(string address)
        {
            string url = $"https://api.openrouteservice.org/geocode/search?api_key={API_KEY}&text={Uri.EscapeDataString(address)}";

            Console.WriteLine($"[ORS] Requesting GeoCode for '{address}'");
            var response = http.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            string json = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<GeoCodeResponse>(json);
        }

        // --- ROUTE ---
        public RouteResponse GetRoute(string profile, string startCoords, string endCoords)
        {
            // Exemple : profile = "cycling-regular" ou "foot-walking"
            string url = $"https://api.openrouteservice.org/v2/directions/{profile}?api_key={API_KEY}&start={startCoords}&end={endCoords}";

            Console.WriteLine($"[ORS] Requesting route: start={startCoords} end={endCoords}");
            var response = http.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            string json = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<RouteResponse>(json);
        }
    }
}
