using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Xml.Schema;
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
        private readonly IScryfallApiCallStatistic _apiCallStatistic;
        private readonly ScryfallApiClient _apiClient;
        private readonly GoodCiticenAutoSleep _autoSleep = new GoodCiticenAutoSleep();
        private readonly IScryfallDatabase _database;
        private readonly ILogger<ScryfallService> _logger;

        private ScryfallConfiguration _configuration = new ScryfallConfiguration();

        public ScryfallService(
            ILoggerFactory loggerFactory,
            IScryfallApiCallStatistic scryfallApiCallStatistic,
            IScryfallDatabase database)
        {
            _apiClient = new ScryfallApiClient(new HttpClient
                {
                    BaseAddress = new Uri("https://api.scryfall.com/")
                });

            _apiCallStatistic = scryfallApiCallStatistic;
            _database = database;
            _logger = loggerFactory.CreateLogger<ScryfallService>();
        }

        public void ShutDown() => _database.ShutDown();

        public void RefreshLocalMirror(
            bool cleanDatabase)
        {
            // TODO: make this abortable 

            if (cleanDatabase)
            {
                _database.ClearDatabase();
            }

            DownloadSetData(false);
            DownloadCardsForAllSets();
        }
        
        public ScryfallCard[] RefreshLocalMirrorForSet(string setCode)
        {
            DownloadSetData(false);
            return DownloadCardsForSet(setCode);
        }

        public void MarkSetCardsAsOutdated(string setCode)
        {
            var cards = _database.ScryfallCards
                            ?.Query()
                            ?.Where(c => c.Set == setCode)
                            ?.ToArray()
                        ?? Array.Empty<ScryfallCard>();

            foreach (var scryfallCard in cards)
            {
                scryfallCard.UpdateDateUtc=DateTime.MinValue;
            }

            _database.ScryfallCards?.Update(cards);
        }

        public void MarkSetsAsOutdated()
        {
            var sets = _database.ScryfallSets?.FindAll()?.ToArray() ?? Array.Empty<ScryfallSet>();
            foreach (var set in sets)
            {
                set.UpdateDateUtc=DateTime.MinValue;
            }

            _database.ScryfallSets?.Update(sets);
        }

        public void Configure(DirectoryInfo folder, ScryfallConfiguration configuration)
        {
            _logger.LogDebug($"Configuring service using '{folder.FullName}'");
            _configuration = configuration;
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

            return result.Select(s => new ScryfallSet(s)).ToArray();
        }

        public ScryfallCard[] RetrieveCardsForSetCode(string setCode)
        {
            var query = $"e:{setCode}";
            return InternalSearch(query, SearchOptions.RollupMode.Prints);
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

        public ILiteCollection<ScryfallCard>? ScryfallCards => _database.ScryfallCards;

        public ILiteCollection<ScryfallSet>? ScryfallSets => _database.ScryfallSets;

        private void DownloadCardsForAllSets()
        {
            var download = _configuration.IsCardOutdated(GetOldestCardByUpdateDate());
            if (!download)
            {
                _logger.LogTrace("All cards are up to date - skipping download");
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var allSets = _database.ScryfallSets?.FindAll()?.ToArray() ?? Array.Empty<ScryfallSet>();
            foreach (var scryfallSet in allSets)
            {
                DownloadCardsForSet(scryfallSet.Code);
            }

            stopwatch.Stop();
            _logger.LogInformation($"Downloading all cards for all sets took {stopwatch.Elapsed}");
        }

        private DateTime? GetOldestCardOfSet(string setCode)
        {
            var found = _database.ScryfallCards
                ?.Query()
                ?.Where(c => c.Set == setCode)
                ?.OrderBy(c => c.UpdateDateUtc)
                ?.FirstOrDefault();

            return found?.UpdateDateUtc;
        }

        private DateTime? GetOldestCardByUpdateDate()
        {
            var found = _database.ScryfallCards
                ?.Query()
                ?.OrderBy(c => c.UpdateDateUtc)
                ?.FirstOrDefault();

            return found?.UpdateDateUtc;
        }

        private DateTime? GetOldestSetByUpdateDate()
        {
            var found = _database.ScryfallSets
                ?.Query()
                ?.OrderBy(c => c.UpdateDateUtc)
                ?.FirstOrDefault();
            return found?.UpdateDateUtc;
        }

        private ScryfallCard[] DownloadCardsForSet(string setCode)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var oldestCardOfSet = GetOldestCardOfSet(setCode);
                var requireDownload = _configuration.IsCardOutdated(oldestCardOfSet);
                if (!requireDownload)
                {
                    _logger.LogTrace($"Cards for set {setCode} are already downloaded - skipping download");
                    return Array.Empty<ScryfallCard>();
                }

                var cards = RetrieveCardsForSetCode(setCode)
                    .Select(c => new ScryfallCard(c))
                    .ToArray();

                if (!cards.Any())
                {
                    // Assume that the download failed - do not insert and do not mark as complete
                    return Array.Empty<ScryfallCard>();
                }

                _database.InsertOrUpdateScryfallCards(cards);
                return cards;
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation($"Download for set {setCode} took {stopwatch.Elapsed}");
            }
        }

        private void DownloadSetData(bool forceRetrieve)
        {
            if (!forceRetrieve)
            {
                forceRetrieve = _configuration.IsSetOutdated(GetOldestSetByUpdateDate());
            }

            if (forceRetrieve)
            {
                var stopwatch = Stopwatch.StartNew();
                var scryfallSets = RetrieveSets()
                    .OrderByDescending(s => s.Name)
                    .ToArray();

                _database.InsertOrUpdateScryfallSets(scryfallSets);
                stopwatch.Stop();
                _logger.LogInformation($"Download {scryfallSets.Length} took {stopwatch.Elapsed}");
            }
            else
            {
                _logger?.LogTrace("Sets are not yet outdated - skipping download");
            }
        }

        private ScryfallCard[] InternalSearch(
            string lookupPattern,
            SearchOptions.RollupMode rollupMode)
        {
            try
            {
                var page = 1;
                ResultList<Card> cards;

                var searchOptions = new SearchOptions
                {
                    Mode = rollupMode,
                    IncludeExtras = true,
                    Direction = SearchOptions.SortDirection.Asc,
                    Sort = SearchOptions.CardSort.Name
                };

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

                return result.Select(c => new ScryfallCard(c)).ToArray();
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
    }
}