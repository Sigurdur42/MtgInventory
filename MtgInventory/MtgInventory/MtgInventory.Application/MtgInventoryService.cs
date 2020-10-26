using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using MkmApi;
using MtgBinder.Domain.Scryfall;
using MtgInventory.Service.Database;
using MtgInventory.Service.Decks;
using MtgInventory.Service.Models;
using MtgInventory.Service.Settings;
using ScryfallApi.Client;
using ScryfallApiServices;
using Serilog;

namespace MtgInventory.Service
{
    /// <summary>
    /// This is the main service class which encapsulates all functionality
    /// </summary>
    public sealed class MtgInventoryService : IDisposable
    {
        private readonly CardDatabase _cardDatabase;
        private readonly SettingsService _settingsService = new SettingsService();
        private AutoDownloadCardsAndSets _autoDownloadCardsAndSets;
        private AutoDownloadImageCache _autoDownloadImageCache;
        private IAutoScryfallService _autoScryfallService;
        private bool _isUpdatingDetailedCards;
        private MkmPriceService _mkmPriceService;
        private MkmRequest _mkmRequest;
        private IScryfallService _scryfallService;

        public MtgInventoryService()
        {
            SystemFolders = new SystemFolders();
            _cardDatabase = new CardDatabase();
            SetsUpdated += (sender, e) => { };
        }

        public event EventHandler SetsUpdated;

        public IEnumerable<DetailedSetInfo> AllSets => _cardDatabase?.MagicSets?.FindAll() ?? new DetailedSetInfo[0];
        public IAutoScryfallService AutoScryfallService => _autoScryfallService;
        public string InternalProductsSummary { get; private set; }
        public IApiCallStatistic MkmApiCallStatistic { get; private set; }
        public MkmAuthenticationData MkmAuthenticationData => _settingsService.Settings.MkmAuthentication;
        public string MkmProductsSummary { get; private set; }
        public IScryfallApiCallStatistic ScryfallApiCallStatistic { get; private set; }
        public string ScryfallProductsSummary { get; private set; }
        public MtgInventorySettings Settings => _settingsService.Settings;
        public SystemFolders SystemFolders { get; }

        public void AutoDownloadCardDetailsForSet(DetailedSetInfo set)
            => _autoDownloadCardsAndSets.AutoDownloadMkmDetails(MkmAuthenticationData, set);

        public void Dispose()
        {
            ShutDown();
        }

        public void DownloadAllProducts()
        {
            var stopwatch = Stopwatch.StartNew();

            _cardDatabase.ClearDetailedCards();

            var scryfallCardDownload = DownloadScryfallData();
            var mkmTask = Task.Factory.StartNew(DownloadMkmSetsAndProducts);

            _cardDatabase.UpdateScryfallStatistics(ScryfallApiCallStatistic);

            mkmTask.Wait();
            scryfallCardDownload.Wait();

            RebuildInternalDatabase();

            UpdateProductSummary();
            stopwatch.Stop();

            Log.Information($"Updating complete database took {stopwatch.Elapsed}");
        }

        public void DownloadMkmSetsAndProducts()
        {
            _autoDownloadCardsAndSets.DownloadMkmSetsAndProducts();
        }

        public IEnumerable<DetailedStockItem> DownloadMkmStock()
        {
            Log.Debug($"{nameof(DownloadMkmStock)} now...");
            if (!_settingsService.Settings.MkmAuthentication.IsValid())
            {
                Log.Warning($"MKM authentication configuration is missing - cannot access MKM API.");
                return new DetailedStockItem[0];
            }

            var result = _mkmRequest
                .GetStockAsCsv(MkmAuthenticationData)
                .Select(s => new DetailedStockItem(s))
                .ToArray();

            UpdateCallStatistics();

            // Now find the scryfall ids

            var mkmIds = result.Select(r => r.IdProduct).ToArray();

            var detailedCards = _cardDatabase.MagicCards.Query()
                .Where(c => mkmIds.Contains(c.MkmId))
                .ToArray();

            foreach (var card in detailedCards)
            {
                var stock = result.Where(c => c.IdProduct == card.MkmId).ToArray();
                foreach (var item in stock)
                {
                    item.ScryfallId = card.ScryfallId;
                }
            }

            Log.Debug($"{nameof(DownloadMkmStock)} loaded {result.Length} items");

            ////_autoDownloadCardsAndSets.AutoDownloadMkmDetails(
            ////    MkmAuthenticationData,
            ////    detailedCards,
            ////    "");

            return result;
        }

        public void DownloadScryfallCardData()
        {
            _cardDatabase.ClearScryfallCards();
            var scryfallSets = _cardDatabase.ScryfallSets.FindAll().ToArray();
            var remainingSets = scryfallSets.Length;
            foreach (var set in scryfallSets)
            {
                Log.Debug($"{nameof(DownloadAllProducts)}: Loading Scryfall cards for set {set.Code} ({remainingSets} remaining)...");
                var cards = _scryfallService.RetrieveCardsForSetCode(set.Code).Select(c => new ScryfallCard(c)).ToArray();
                _cardDatabase.InsertScryfallCards(cards);
                remainingSets--;
                // Insert prices from Scryfall
                var prices = cards.Select(c => new CardPrice(c));
                _cardDatabase.CardPrices.InsertBulk(prices);
            }
            _cardDatabase.EnsureCardPriceIndex();
            Log.Debug($"{nameof(DownloadAllProducts)}: Done loading Scryfall cards ...");
        }

        public ScryfallSet[] DownloadScryfallSetsData()
        {
            return _autoDownloadCardsAndSets.DownloadScryfallSetsData();
        }

        public void EnrichDeckListWithDetails(DeckList deckList)
        {
            Log.Debug($"{nameof(EnrichDeckListWithDetails)} for deck {deckList.Name}");

            foreach (var card in deckList.Mainboard.Where(c => c.CardId == Guid.Empty))
            {
                var found = _cardDatabase.MagicCards
                    .Query()
                    .Where(c => c.NameEn.Equals(card.Name))
                    .Where(c => !string.IsNullOrWhiteSpace(c.SetCode))
                    .ToList()
                    .Where(c => c.SetReleaseDate.HasValue)
                    .OrderByDescending(c => c.SetReleaseDate)
                    .ToArray();

                if (found.Any())
                {
                    var use = found.First();
                    card.CardId = use.Id;
                    card.SetCode = use.SetCode;
                    card.SetName = use.SetName;
                }
            }
        }

        public IEnumerable<DetailedSetInfo> FilterSets(QuerySetFilter querySetFilter)
        {
            var query = _cardDatabase.MagicSets?.Query();

            if (!string.IsNullOrWhiteSpace(querySetFilter.Name))
            {
                query = query?.Where(s => s.SetName.Contains(querySetFilter.Name));
            }

            if (querySetFilter.HideOnlyOneSide)
            {
                query = query?.Where(s => !s.IsKnownMkmOnlySet && !s.IsKnownScryfallOnlySet);
            }
            if (querySetFilter.HideKnownSets)
            {
                query = query?.Where(s => s.MigrationStatus != SetMigrationStatus.Migrated);
            }

            return query?.ToArray() ?? new DetailedSetInfo[0];
        }

        public IEnumerable<DetailedMagicCard> FindDetailedCardsByName(QueryCardOptions query)
        {
            if (_cardDatabase?.MagicCards == null || !query.IsValid)
            {
                return new DetailedMagicCard[0];
            }

            Log.Debug($"{nameof(FindDetailedCardsByName)}: {query}");

            var databaseQuery = _cardDatabase.MagicCards.Query();

            if (!string.IsNullOrEmpty(query.Name))
            {
                databaseQuery = databaseQuery.Where(p => p.NameEn.Contains(query.Name, StringComparison.InvariantCultureIgnoreCase));
            }

            if (query.IsBasicLand)
            {
                databaseQuery = databaseQuery.Where(q => q.IsBasicLand);
            }

            if (query.IsToken)
            {
                databaseQuery = databaseQuery.Where(q => q.IsToken);
            }

            if (query.IsSetName)
            {
                databaseQuery = databaseQuery.Where(q => q.SetName == query.SetName);
            }

            var result = databaseQuery
                .ToList()
                .OrderBy(p => p.NameEn)
                .ThenBy(p => p.SetName)
                .ToList();

            _autoDownloadCardsAndSets.AutoDownloadMkmDetails(MkmAuthenticationData, result.ToArray(), query.Name);

            Log.Debug($"{nameof(FindDetailedCardsByName)}: {query} returned {result.Count} records");

            return result;
        }

        public void GenerateMissingSetData()
        {
            var unmatchedSets = _cardDatabase.MagicSets
                ?.Query()
                ?.Where(s => s.MigrationStatus == SetMigrationStatus.Unknown)
                ?.ToArray()
                ?.OrderBy(s => s.SetCodeMkm)
                ?.ThenBy(s => s.SetCodeScryfall)
                                ?.ToArray()
                                ?? new DetailedSetInfo[0];

            var targetFile = new FileInfo(Path.Combine(SystemFolders.BaseFolder.FullName, "UnmatchedSets.csv"));
            if (!targetFile.Directory?.Exists ?? false)
            {
                targetFile.Directory?.Create();
            }

            var parts = unmatchedSets
                .Select(s => $"{s.SetNameScryfall};{s.SetCodeScryfall};{s.SetNameMkm};{s.SetCodeMkm}")
                .ToArray();
            var headline = $"ScryfallName;ScryfallCode;MkmName;MkmCode";
            File.WriteAllText(targetFile.FullName, headline + Environment.NewLine + string.Join(Environment.NewLine, parts));

            Log.Information($"Wrote {unmatchedSets.Length} set infos to {targetFile.FullName}");

            var mkmDetailIds = _cardDatabase.MkmAdditionalInfo
                ?.FindAll()
                ?.Select(d => d.MkmId)
                ?.Distinct()
                ?.ToArray()
                ?? new string[0];

            Log.Debug($"Found {mkmDetailIds.Length} items in MKM additional details");
            var knownSets = new List<string>();
            var productsIndexed = _cardDatabase.MkmProductInfo
                ?.FindAll()
                ?.ToDictionary(p => p.Id)
                ?? new Dictionary<string, MkmProductInfo>();

            foreach (var id in mkmDetailIds)
            {
                if (productsIndexed.TryGetValue(id, out var found))
                {
                    knownSets.Add(found.ExpansionCode);
                }
            }

            knownSets = knownSets.Distinct().ToList();
            var missingSets = _cardDatabase.MagicSets
                ?.Query()
                ?.Where(s => !knownSets.Contains(s.SetCodeMkm))
                ?.ToArray()
                ?? new DetailedSetInfo[0];

            targetFile = new FileInfo(Path.Combine(SystemFolders.BaseFolder.FullName, "MissingDetailsSets.csv"));
            File.WriteAllText(targetFile.FullName, string.Join(Environment.NewLine, missingSets.OrderBy(s=>s.SetName).Select(s => s.SetName)));
        }

        public void GenerateReferenceCardData()
        {
            var stopwatch = Stopwatch.StartNew();
            Log.Information($"Start generating card reference data...");

            var allCards = _cardDatabase?.MagicCards?.FindAll()
                ?.Where(c => !c.IsScryfallOnly && !c.MkmDetailsRequired)
                ?.ToArray() ?? new DetailedMagicCard[0];

            var result = allCards.Select(card => card.GenerateReferenceData()).ToList();

            ////var reader = new CardReferenceDataReader();
            ////var targetFile = new FileInfo(Path.Combine(SystemFolders.BaseFolder.FullName, "ReferenceData.yaml"));

            ////reader.Write(targetFile, result.ToArray());

            stopwatch.Stop();
            Log.Information($"Done generating card reference data in {stopwatch.Elapsed}");
        }

        public void GenerateReferenceSetData()
        {
            var unmatchedSets = _cardDatabase.MagicSets
                ?.Query()
                ?.Where(s => s.MigrationStatus != SetMigrationStatus.Unknown || s.SetCodeMkm == s.SetCodeScryfall)
                ?.ToArray()
                ?.Where(s => !string.IsNullOrEmpty(s.SetCodeMkm))
                ?.OrderBy(s => s.SetCodeMkm)
                ?.Select(s => new SetReferenceData()
                {
                    MkmCode = s.SetCodeMkm,
                    ScryfallCode = s.SetCodeScryfall,
                })
                ?.ToArray()
                ?? new SetReferenceData[0];

            var targetFile = new FileInfo(Path.Combine(SystemFolders.BaseFolder.FullName, "ReferenceSet.csv"));
            if (!targetFile.Directory?.Exists ?? false)
            {
                targetFile.Directory?.Create();
            }

            using (var writer = new StreamWriter(targetFile.FullName))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(unmatchedSets);
            }

            Log.Information($"Wrote {unmatchedSets.Length} set infos to {targetFile.FullName}");
        }

        public void Initialize(
            IApiCallStatistic mkmApiCallStatistic,
            IScryfallApiCallStatistic scryfallApiCallStatistic)
        {
            Log.Information($"{nameof(Initialize)}: Initializing application service");

            ////var reader = new AuthenticationReader();
            ////_mkmAuthenticationDataFile = new FileInfo(Path.Combine(SystemFolders.BaseFolder.FullName, ".mkmAuthenticationData"));
            ////Log.Debug($"{nameof(Initialize)}: Loading MKM authentication data from '{_mkmAuthenticationDataFile.FullName}'...");
            ////MkmAuthenticationData = reader.ReadFromYaml(_mkmAuthenticationDataFile);

            _settingsService.Initialize(SystemFolders.BaseFolder);
            _cardDatabase.Initialize(SystemFolders.BaseFolder);

            MkmApiCallStatistic = mkmApiCallStatistic;
            var fromDatabase = _cardDatabase.GetMkmCallStatistic();
            mkmApiCallStatistic.CountToday = fromDatabase.CountToday;
            mkmApiCallStatistic.CountTotal = fromDatabase.CountTotal;
            mkmApiCallStatistic.Today = fromDatabase.Today;
            mkmApiCallStatistic.Id = fromDatabase.Id;

            ScryfallApiCallStatistic = scryfallApiCallStatistic;
            var fromDatabaseScryfall = _cardDatabase.GetScryfallApiStatistics();
            ScryfallApiCallStatistic.ScryfallCountToday = fromDatabaseScryfall.ScryfallCountToday;
            ScryfallApiCallStatistic.ScryfallCountTotal = fromDatabaseScryfall.ScryfallCountTotal;
            ScryfallApiCallStatistic.Today = fromDatabaseScryfall.Today;
            ScryfallApiCallStatistic.Id = fromDatabaseScryfall.Id;

            _scryfallService = new ScryfallService(new ScryfallApiClient(new System.Net.Http.HttpClient()
            {
                BaseAddress = new Uri("https://api.scryfall.com/")
            }, null, null),
            ScryfallApiCallStatistic);

            UpdateProductSummary();

            _mkmRequest = new MkmRequest(MkmApiCallStatistic);

            _mkmPriceService = new MkmPriceService(_cardDatabase, _mkmRequest);
            _autoScryfallService = new AutoScryfallService(
                _cardDatabase,
                _scryfallService,
                _settingsService);

            _autoDownloadCardsAndSets = new AutoDownloadCardsAndSets(
                _settingsService,
                _cardDatabase,
                MkmApiCallStatistic,
                ScryfallApiCallStatistic,
                _scryfallService,
                _mkmRequest);

            _autoDownloadCardsAndSets.CardsUpdated += (sender, args) =>
            {
                UpdateProductSummary();
                UpdateCallStatistics();
            };

            _autoDownloadCardsAndSets.SetsUpdated += (sender, args) =>
            {
                UpdateProductSummary();
                UpdateCallStatistics();
                SetsUpdated?.Invoke(this, EventArgs.Empty);
            };

            _autoDownloadCardsAndSets.Start();

            _autoDownloadImageCache = new AutoDownloadImageCache(SystemFolders.BaseFolder);
        }

        public void OpenMkmProductPage(string mkmId)
        {
            if (string.IsNullOrEmpty(mkmId))
            {
                Log.Warning($"Cannot find product with empty MKM id");
            }

            var found = _cardDatabase.MagicCards.Query().Where(p => p.MkmId == mkmId).FirstOrDefault();
            if (found == null)
            {
                Log.Warning($"Cannot find product with MKM id {mkmId}");
                return;
            }

            OpenMkmProductPage(found);
        }

        public void OpenMkmProductPage(DetailedMagicCard product)
        {
            var prefix = $"{nameof(OpenMkmProductPage)}({product.Id} {product.NameEn} {product.SetCode})";

            var additionalInfo = _cardDatabase.FindAdditionalMkmInfo(product.MkmId) ?? new MkmAdditionalCardInfo();

            if (!additionalInfo.IsValid())
            {
                Log.Information($"{prefix}: Downloading additional info...");

                // We need to download the product details first
                if (string.IsNullOrEmpty(product.MkmId))
                {
                    Log.Warning($"Card {product} does not exist on MKM. ");
                    return;
                }

                if (!_settingsService.Settings.MkmAuthentication.IsValid())
                {
                    Log.Warning($"MKM authentication configuration is missing - cannot access MKM API.");
                    return;
                }

                var productDetails = _mkmRequest.GetProductData(MkmAuthenticationData, product.MkmId);
                additionalInfo = _cardDatabase.UpdateMkmAdditionalInfo(productDetails);

                UpdateCallStatistics();
            }

            // Now open a browser with the url
            Log.Debug($"{prefix}: Opening MKM product page...");
            Browser.OpenBrowser(additionalInfo.MkmWebSite);
        }

        public void RebuildCardsForSet(DetailedSetInfo set)
        {
            _autoDownloadCardsAndSets.Stop();

            set.CardsLastUpdated = DateTime.Now.AddDays(-1000);
            _cardDatabase.MagicSets.Update(set);
            _autoDownloadCardsAndSets.Start();
        }

        public Task RebuildInternalDatabase()
        {
            return Task.Factory.StartNew(() =>
            {
                if (_isUpdatingDetailedCards)
                {
                    Log.Debug($"{nameof(RebuildInternalDatabase)}: Update already running - ignoring new request");
                    return;
                }

                try
                {
                    _isUpdatingDetailedCards = true;
                    var stopwatch = Stopwatch.StartNew();
                    Log.Information($"{nameof(RebuildInternalDatabase)}: Starting database rebuild");

                    _autoDownloadCardsAndSets.Stop();

                    var allSets = _cardDatabase.MagicSets.FindAll().ToArray();
                    foreach (var set in allSets)
                    {
                        set.SetLastUpdated = DateTime.Now.AddDays(-1000);
                        set.CardsLastUpdated = DateTime.Now.AddDays(-1000);
                    }

                    _cardDatabase.MagicSets.Update(allSets);

                    _autoDownloadCardsAndSets.Start();

                    stopwatch.Stop();
                    Log.Information($"{nameof(RebuildInternalDatabase)}: Rebuild done in {stopwatch.Elapsed}");
                }
                finally
                {
                    _isUpdatingDetailedCards = false;
                }
            });
        }

        public void RebuildSetData()
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                _autoDownloadCardsAndSets.Stop();

                var knownSets = _cardDatabase.MagicSets
                    .FindAll()
                    .ToArray();

                foreach (var detailedSetInfo in knownSets)
                {
                    detailedSetInfo.SetLastUpdated = DateTime.Now.AddDays(-1000);
                }

                _cardDatabase.MagicSets.Update(knownSets);
                _autoDownloadCardsAndSets.Start();
            }
            finally
            {
                stopwatch.Stop();
                Log.Information($"Rebuild set database done in {stopwatch.Elapsed}");
            }
        }

        public void SaveSettings() => _settingsService.SaveSettings();

        public void ShutDown()
        {
            Log.Information($"{nameof(ShutDown)}: Shutting down application service");

            _autoDownloadCardsAndSets.Stop();
            _settingsService.SaveSettings();
            _settingsService.Dispose();
            _cardDatabase.Dispose();
        }

        public void UpdateCallStatistics()
        {
            _cardDatabase.UpdateMkmStatistics(MkmApiCallStatistic);
            _cardDatabase.UpdateScryfallStatistics(ScryfallApiCallStatistic);
        }

        internal Task DownloadScryfallData()
        {
            return Task.Factory.StartNew(() =>
            {
                DownloadScryfallSetsData();
                DownloadScryfallCardData();
            });
        }

        private void UpdateProductSummary()
        {
            MkmProductsSummary = $"{_cardDatabase.MkmProductInfo.Count()} products in {_cardDatabase.MkmExpansion.Count()} sets";
            ScryfallProductsSummary = $"{_cardDatabase.ScryfallCards.Count()} cards in {_cardDatabase.ScryfallSets.Count()} sets";
            InternalProductsSummary = $"{_cardDatabase.MagicCards.Count()} cards";
        }
    }
}