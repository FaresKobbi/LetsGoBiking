using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class JCDecaux
    {
        private const string API_KEY = "e21efd98ac9873f5b32a0ca8d193b32240b8d4ad";
        private static readonly HttpClient client = new HttpClient();

        public string GetStations(string contract)
        {
            try
            {
                string url = $"https://api.jcdecaux.com/vls/v1/stations?contract={contract}&apiKey={API_KEY}";
                Console.WriteLine($"[JCDecaux] Calling REST API for {contract}");
                var response = client.GetStringAsync(url).Result;
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] JCDecaux API call failed: {ex.Message}");
                return null;
            }
        }
    }
}
