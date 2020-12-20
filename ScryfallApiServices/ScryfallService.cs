using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ScryfallApi.Client;
using ScryfallApi.Client.Models;
using ScryfallApiServices.Database;
using ScryfallApiServices.Models;
using JsonReader = Newtonsoft.Json.JsonReader;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

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

        public ILiteCollection<ScryfallCard>? ScryfallCards => _database.ScryfallCards;

        public ILiteCollection<ScryfallSet>? ScryfallSets => _database.ScryfallSets;

        public void ShutDown() => _database.ShutDown();

        public void RefreshLocalMirror(
            bool cleanDatabase,
            bool downloadSetsOnly)
        {
            // TODO: make this abortable

            if (cleanDatabase)
            {
                _database.ClearDatabase();
            }

            DownloadSetData(cleanDatabase);

            if (!downloadSetsOnly)
            {
                DownloadCardsForAllSets();
            }
        }

        public async Task DownloadBulkData() =>
            await Task.Run(async () =>
            {
                using (var httpClient = new HttpClient())
                {
                    _logger.LogTrace("Getting bulk info for all cards...");
                    using (var response = await httpClient.GetAsync(" https://api.scryfall.com/bulk-data/all_cards"))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        var definition = new
                        {
                            updated_at = DateTime.MinValue,
                            download_uri = "",
                            content_encoding = ""
                        };

                        var responseRead = JsonConvert.DeserializeAnonymousType(apiResponse, definition);

                        _logger.LogTrace($"Bulk data last updated at {responseRead.updated_at}...");

                        _logger.LogTrace("Downloading all data now ...");

                        using (var downloadStream = await httpClient.GetStreamAsync(responseRead.download_uri))
                        {
                            var downloadedCards = ReadFromStream(downloadStream);

                            // var baseFolder = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MtgDatabase"));
                            // using (var fileStream = new FileStream(Path.Combine(baseFolder.FullName, "Downloaded.json"),
                            //     FileMode.Create, FileAccess.Write, FileShare.None, 10000, true))
                            // {
                            //     await downloadStream.CopyToAsync(fileStream);
                            // }

                            //  // using var decompressionStream = new GZipStream(downloadStream, CompressionMode.Decompress);
                            //  using var decompressedStreamReader = new StreamReader(downloadStream, Encoding.UTF8, 
                            //      false, 
                            //      bufferSize: 1024 * 1024 * 1024);
                            //
                            //  var cardDefinition = new
                            //  {
                            //      name = "",
                            //      printed_name= "",
                            //      lang = "EN",
                            //  };
                            //
                            //  var completeJson = await decompressedStreamReader.ReadToEndAsync();
                            //  
                            // var result = JsonConvert.DeserializeAnonymousType(completeJson, cardDefinition);
                        }

                        // reservationList = JsonConvert.DeserializeObject<List<Reservation>>(apiResponse);
                    }
                }
            });

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
                scryfallCard.UpdateDateUtc = DateTime.MinValue;
            }

            _database.ScryfallCards?.Update(cards);
        }

        public void MarkSetsAsOutdated()
        {
            var sets = _database.ScryfallSets?.FindAll()?.ToArray() ?? Array.Empty<ScryfallSet>();
            foreach (var set in sets)
            {
                set.UpdateDateUtc = DateTime.MinValue;
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

        private IEnumerable<ScryfallBulkCard> ReadFromStream(Stream inputStream)
        {
            var result = new List<ScryfallBulkCard>();
            var serializer = new JsonSerializer();

            using (StreamReader sr = new StreamReader(inputStream))
            {
                using (JsonReader reader = new JsonTextReader(sr))
                {
                    while (reader.Read())
                    {
                        // deserialize only when there's "{" character in the stream
                        if (reader.TokenType == JsonToken.StartObject)
                        {
                            var single = serializer.Deserialize<ScryfallBulkCard>(reader);
                            result.Add(single);
                        }
                    }
                }
            }

            return result;
        }

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
            const int pageSize = 10;
            var pages = (int)Math.Round(allSets.Length / (decimal)pageSize, MidpointRounding.AwayFromZero);
            for (var page = 0; page < pages; ++page)
            {
                _logger.LogTrace($"Retrieving page {page} of {pages} now...");
                var pageSelect = allSets.Skip(page * pageSize).Take(pageSize);
                var query = string.Join(" OR ", pageSelect.Select(s => $"e:{s.Code}"));
                var canQuery = !string.IsNullOrWhiteSpace(query);
                var cards = canQuery ? InternalSearch(query, SearchOptions.RollupMode.Prints) : Array.Empty<ScryfallCard>();
                if (!cards.Any())
                {
                    // Assume that the download failed - do not insert and do not mark as complete
                    return;
                }

                _database.InsertOrUpdateScryfallCards(cards);
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

                var cards = RetrieveCardsForSetCode(setCode);
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

                var scryfallCards = new List<ScryfallCard>();
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
                        scryfallCards.AddRange(cards.Data.Select(c => new ScryfallCard(c)));
                    }

                    // TODO: Handle errors
                } while (cards.HasMore);

                return scryfallCards.ToArray();
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