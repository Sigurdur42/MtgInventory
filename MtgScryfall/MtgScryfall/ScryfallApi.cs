using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MtgScryfall
{
    public class ScryfallApi : IScryfallApi
    {
        private const string _baseAddress = "https://api.scryfall.com/";

        public RequestResult GetAllSets()
        {
            var client = CreateHttpClient();
            var response = client.GetAsync("sets").Result;

            return response.CreateResult();
        }

        public RequestResult GetCardsByPage(int page)
        {
            var client = CreateHttpClient();
            var response = client.GetAsync($"cards?page={page}").Result;

            return response.CreateResult();
        }

        private HttpClient CreateHttpClient()
        {
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(_baseAddress)
            };

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }
    }
}