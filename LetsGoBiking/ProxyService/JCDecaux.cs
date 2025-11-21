using Newtonsoft.Json;
using ProxyService.Models;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;

namespace ProxyService
{
    internal class JCDecaux
    {
        private const string API_KEY = "e21efd98ac9873f5b32a0ca8d193b32240b8d4ad";
        private static readonly HttpClient client = new HttpClient();
        public List<JCContract> GetAllContracts()
        {

            string url = $"https://api.jcdecaux.com/vls/v3/contracts?apiKey={API_KEY}";
            var response = client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            var json = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<List<JCContract>>(json);
        }
        public List<Station> GetStationsForContract(string contractName)
        {
            string url = $"https://api.jcdecaux.com/vls/v3/stations?contract={contractName}&apiKey={API_KEY}";
            var response = client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();

            var json = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<List<Station>>(json);
        }
    }
}
