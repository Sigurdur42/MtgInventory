using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LiteDB;
using Microsoft.Extensions.Logging;
using ScryfallApi.Client;
using ScryfallApi.Client.Models;
using ScryfallApiServices.Database;
using ScryfallApiServices.Models;

namespace ScryfallApiServices
{
    public class ScryfallService : IScryfallService
    {
        private readonly ScryfallApiClient _apiClient;
        private readonly IScryfallApiCallStatistic _apiCallStatistic;
        private readonly IScryfallDatabase _database;
        private readonly GoodCiticenAutoSleep _autoSleep = new GoodCiticenAutoSleep();
        private readonly ILogger<ScryfallService> _logger;

        public ScryfallService(
            ILoggerFactory loggerFactory,
            IScryfallApiCallStatistic scryfallApiCallStatistic,
            IScryfallDatabase database)
        {
            _apiClient = new ScryfallApiClient(new System.Net.Http.HttpClient()
                {
                    BaseAddress = new Uri("https://api.scryfall.com/")
                }, 
                loggerFactory.CreateLogger<ScryfallApiClient>());

            _apiCallStatistic = scryfallApiCallStatistic;
            _database = database;
            _logger = loggerFactory.CreateLogger<ScryfallService>();
        }

        public void ShutDown()
        {
            _database.ShutDown();
        }

        public void RefreshLocalMirror(
            bool cleanDatabase)
        {
            // TODO: make this abortable 
            
            if (cleanDatabase)
            {
                _database.ClearDatabase();
            }

            DownloadSetData();
            DownloadCardsForAllSets();
        }

        private void DownloadCardsForAllSets()
        {
            var stopwatch = Stopwatch.StartNew();
            var allSets = _database.ScryfallSets?.FindAll()?.ToArray() ?? Array.Empty<ScryfallSet>();
            foreach (var scryfallSet in allSets)
            {
                DownloadCardsForSet(scryfallSet);
            }
            
            stopwatch.Stop();
            _logger.LogInformation($"Downloading all cards for all sets took {stopwatch.Elapsed}");
        }

        private DateTime? GetOldestCardOfSet(ScryfallSet set)
        {
            var found = _database.ScryfallCards
                ?.Query()
                ?.Where(c => c.Set == set.Code)
                ?.OrderBy(c => c.UpdateDateUtc)
                ?.FirstOrDefault();

            return found?.UpdateDateUtc;
        }
        
        private void DownloadCardsForSet(ScryfallSet set)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var lastCardUpdate = GetOldestCardOfSet(set);
                if (lastCardUpdate != null)
                {
                    _logger.LogTrace($"Cards for set {set.Name} are already downloaded - skipping download");
                    // TODO: configure outdated date
                    return;
                }
                
                var cards = RetrieveCardsForSetCode(set.Code)
                    .Select(c => new ScryfallCard(c))
                    .ToArray();

                if (!cards.Any())
                {
                    // Assume that the download failed - do not insert and do not mark as complete
                    return;
                }

                _database.InsertScryfallCards(cards);
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation($"Download for set {set.Name} took {stopwatch.Elapsed}");
            }
        }
        private void DownloadSetData()
        {
            var stopwatch = Stopwatch.StartNew();
            var scryfallSets = RetrieveSets()
                .OrderByDescending(s => s.Name)
                .ToArray();

            _database.InsertScryfallSets(scryfallSets);
            stopwatch.Stop();
            _logger.LogInformation($"Download {scryfallSets.Length} took {stopwatch.Elapsed}");
        }

        public void Configure(DirectoryInfo folder)
        {
            _logger.LogDebug($"Configuring service using '{folder.FullName}'");
            _database.Configure(folder);
        }

        public IEnumerable<ScryfallSet> RetrieveSets()
        {
            _autoSleep.AutoSleep();

            var sets = _apiClient.Sets.Get().Result;
            IncrementCallStatistic();
            var result = sets.Data.ToArray();
            foreach (var item in result)
            {
                item.Code = item.Code?.ToUpperInvariant();
            }

            return result.Select(s=>new ScryfallSet(s)).ToArray();
        }

        public ScryfallCard[] RetrieveCardsForSetCode(string setCode)
        {
            var query = $"e:{setCode}";
            return InternalSearch(query, SearchOptions.RollupMode.Prints)                ;
        }

        public ScryfallCard[] RetrieveCardsByCardName(string cardName, SearchOptions.RollupMode rollupMode)
        {
            var query = $"{cardName}";
            return InternalSearch(query, rollupMode);
        }

        public ScryfallCard[] RetrieveCardsByCardNameAndSet(string cardName, string setCode, SearchOptions.RollupMode rollupMode)
        {
            var query = $"!'{cardName}' e:{setCode}";
            return InternalSearch(query, rollupMode);
        }

        private ScryfallCard[] InternalSearch(
            string lookupPattern,
            SearchOptions.RollupMode rollupMode)
        {
            try
            {
                var page = 1;
                ResultList<Card> cards;

                var searchOptions = new SearchOptions() { Mode = rollupMode, IncludeExtras = true, Direction = SearchOptions.SortDirection.Asc, Sort = SearchOptions.CardSort.Name };

                var result = new List<Card>();
                do
                {
                    _autoSleep.AutoSleep();
                    cards = _apiClient.Cards.Search(lookupPattern, page, searchOptions).Result;
                    IncrementCallStatistic();

                    // if (page == 1 && cards.HasMore)
                    // {
                    //     // Initialize progress
                    //     var pageCount = cards.TotalCards / cards.Data.Count;
                    // }

                    ++page;

                    if (cards.Data != null)
                    {
                        result.AddRange(cards.Data);
                    }

                    // TODO: Handle errors
                } while (cards.HasMore);

                return result.Select(c=>new ScryfallCard(c)).ToArray();
            }
            catch (Exception error)
            {
                _logger.LogError($"Cannot run search for {lookupPattern}: {error}");
                return Array.Empty<ScryfallCard>();
            }
        }

        private void IncrementCallStatistic()
        {
            var today = DateTime.Now.Date;
            if (_apiCallStatistic.Today != today)
            {
                _apiCallStatistic.Today = today;
                _apiCallStatistic.ScryfallCountToday = 0;
            }

            _apiCallStatistic.ScryfallCountToday += 1;
            _apiCallStatistic.ScryfallCountTotal += 1;
        }

        public ILiteCollection<ScryfallCard>? ScryfallCards => _database.ScryfallCards;

        public ILiteCollection<ScryfallSet>? ScryfallSets => _database.ScryfallSets;
    }
}