using Microsoft.Extensions.Logging;
using MtgScryfall.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

namespace MtgScryfall
{
    public class ScryfallApi : IScryfallApi
    {
        private const string _baseAddress = "https://api.scryfall.com/";
        private readonly ILogger _logger;

        private DateTime _lastRequest = DateTime.UtcNow;

        public ScryfallApi(ILoggerFactory factory)
        {
            _logger = factory?.CreateLogger(nameof(ScryfallApi));
        }

        public RequestResult GetAllSets()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger?.LogDebug($"{nameof(GetAllSets)}: Starting request...");
            using (var client = CreateHttpClient())
            {
                var response = client.GetAsync("sets").Result;

                stopwatch.Stop();
                _logger?.LogDebug($"{nameof(GetAllSets)}: Request took {stopwatch.Elapsed} and returned with status {response.StatusCode}");
                _lastRequest = DateTime.UtcNow;
                return response.CreateResult();
            }
        }

        public RequestResult GetCardsByPage(int page)
        {
            using (var client = CreateHttpClient())
            {
                var response = client.GetAsync($"cards?page={page}").Result;
                _lastRequest = DateTime.UtcNow;
                return response.CreateResult();
            }
        }

        public RequestResult GetCardsBySet(string setCode, int page)
        {
            using (var client = CreateHttpClient())
            {
                AutoDelay();
                _logger?.LogDebug($"Loading cards for set {setCode} (page {page})");
                var response = client.GetAsync($"cards/search?page={page};order=cmc&q=++e:{setCode}%20unique:prints").Result;
                _lastRequest = DateTime.UtcNow;
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

        private void AutoDelay()
        {
            const int waitTime = 120;
            var now = DateTime.UtcNow;
            if ((now - _lastRequest).TotalMilliseconds < waitTime)
            {
                _logger?.LogDebug($"Auto delaying request to avoid too heavy traffic.");
                Thread.Sleep(waitTime);
            }

            _lastRequest = now;
        }

        private HttpClient CreateHttpClient()
        {
            AutoDelay();
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