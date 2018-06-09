using Microsoft.Extensions.Logging;
using MtgScryfall.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MtgScryfall
{
    public class ScryfallApi : IScryfallApi
    {
        private const string _baseAddress = "https://api.scryfall.com/";
        private readonly ILogger _logger;

        public ScryfallApi(ILoggerFactory factory)
        {
            _logger = factory?.CreateLogger<ScryfallApi>();
        }

        public RequestResult GetAllSets()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger?.LogDebug($"{nameof(GetAllSets)}: Starting request...");
            var client = CreateHttpClient();
            var response = client.GetAsync("sets").Result;

            stopwatch.Stop();
            _logger?.LogDebug($"{nameof(GetAllSets)}: Request took {stopwatch.Elapsed} and returned with status {response.StatusCode}");

            return response.CreateResult();
        }

        public RequestResult GetCardsByPage(int page)
        {
            var client = CreateHttpClient();
            var response = client.GetAsync($"cards?page={page}").Result;

            return response.CreateResult();
        }

        public RequestResult GetCardsBySet(string setCode, int page)
        {
            using (var client = CreateHttpClient())
            {
                _logger?.LogDebug($"Loading cards for set {setCode} (page {page}");
                var response = client.GetAsync($"cards/search?page={page};order=cmc&q=++e:{setCode}").Result;

                return response.CreateResult();
            }
        }

        public CardDataRequestResult GetCardsBySet(string setCode)
        {
            var allCards = new List<CardData>();
            var result = GetCardsBySet(setCode, 1).DeserializeCardData();
            allCards.AddRange(result.CardData);

            var page = 2;
            while (result.HasMoreData)
            {
                result = GetCardsBySet(setCode, page).DeserializeCardData();
                allCards.AddRange(result.CardData);
                page++;
            }

            result.CardData = allCards.ToArray();

            _logger?.LogInformation($"Loaded {result.CardData.Length} cards for set {setCode}");
            return result;
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