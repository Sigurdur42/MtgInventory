using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MkmApi;
using MtgBinder.Domain.Scryfall;
using MtgInventory.Service.Database;
using MtgInventory.Service.Decks;
using MtgInventory.Service.Models;
using ScryfallApi.Client;
using Serilog;

namespace MtgInventory.Service
{
    /// <summary>
    /// This is the main service class which encapsulates all functionality
    /// </summary>
    public sealed class MtgInventoryService : IDisposable
    {
        private readonly CardDatabase _cardDatabase;
        private MkmRequest _mkmRequest;
        private FileInfo _mkmAuthenticationDataFile;

        private IScryfallService _scryfallService;

        private bool _isUpdatingDetailedCards;
        private MkmPriceService _mkmPriceService;

        public MtgInventoryService()
        {
            SystemFolders = new SystemFolders();
            _cardDatabase = new CardDatabase();
            _scryfallService = new ScryfallService(new ScryfallApiClient(new System.Net.Http.HttpClient()
            {
                BaseAddress = new Uri("https://api.scryfall.com/")
            }, null, null));
        }

        public SystemFolders SystemFolders { get; }

        public MkmAuthenticationData MkmAuthenticationData { get; private set; }

        public string MkmProductsSummary { get; private set; }
        public string ScryfallProductsSummary { get; private set; }
        public string InternalProductsSummary { get; private set; }

        public IApiCallStatistic MkmApiCallStatistic { get; private set; }

        public IEnumerable<DetailedSetInfo> AllSets => _cardDatabase.MagicSets.FindAll();

        public void Dispose()
        {
            ShutDown();
        }

        public void Initialize(IApiCallStatistic mkmApiCallStatistic)
        {
            Log.Information($"{nameof(Initialize)}: Initializing application service");

            var reader = new AuthenticationReader();

            _mkmAuthenticationDataFile = new FileInfo(Path.Combine(SystemFolders.BaseFolder.FullName, ".mkmAuthenticationData"));
            Log.Debug($"{nameof(Initialize)}: Loading MKM authentication data from '{_mkmAuthenticationDataFile.FullName}'...");

            MkmAuthenticationData = reader.ReadFromYaml(_mkmAuthenticationDataFile);

            _cardDatabase.Initialize(SystemFolders.BaseFolder);

            MkmApiCallStatistic = mkmApiCallStatistic;
            var fromDatabase = _cardDatabase.GetMkmCallStatistic();
            mkmApiCallStatistic.CountToday = fromDatabase.CountToday;
            mkmApiCallStatistic.CountTotal = fromDatabase.CountTotal;
            mkmApiCallStatistic.Today = fromDatabase.Today;
            mkmApiCallStatistic.Id = fromDatabase.Id;

            UpdateProductSummary();

            _mkmRequest = new MkmRequest(MkmAuthenticationData, MkmApiCallStatistic);

            _mkmPriceService = new MkmPriceService(_cardDatabase, _mkmRequest);
        }

        public void ShutDown()
        {
            Log.Information($"{nameof(ShutDown)}: Shutting down application service");

            _cardDatabase.Dispose();
        }

        public ScryfallSet[] DownloadScryfallSets(bool rebuildDetailedSetInfo)
        {
            Log.Debug($"{nameof(DownloadAllProducts)}: Loading Scryfall expansions...");
            var scryfallSets = _scryfallService.RetrieveSets().OrderByDescending(s => s.Name).Select(s => new ScryfallSet(s)).ToArray();
            _cardDatabase.InsertScryfallSets(scryfallSets);

            if (rebuildDetailedSetInfo)
            {
                _cardDatabase.RebuildSetData();
            }

            Log.Debug($"{nameof(DownloadAllProducts)}: Done loading Scryfall expansions...");
            return scryfallSets;
        }

        internal Task DownloadScryfallData()
        {
            return Task.Factory.StartNew(() =>
            {
                var scryfallSets = DownloadScryfallSets(false);

                _cardDatabase.ClearScryfallCards();
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
            });
        }

        public void DownloadAllProducts()
        {
            var stopwatch = Stopwatch.StartNew();

            _cardDatabase.ClearDetailedCards();

            var scryfallCardDownload = DownloadScryfallData();

            var mkmTask = Task.Factory.StartNew(() =>
            {
                Log.Debug($"{nameof(DownloadAllProducts)}: Loading MKM expansions...");

                var expansions = _mkmRequest.GetExpansions(1);
                _cardDatabase.InsertExpansions(expansions);

                Log.Debug($"{nameof(DownloadAllProducts)}: Loading MKM products...");
                using var products = _mkmRequest.GetProductsAsCsv();

                Log.Debug($"{nameof(DownloadAllProducts)}: Inserting products into database...");
                _cardDatabase.InsertProductInfo(products.Products, expansions);

                _cardDatabase.UpdateMkmStatistics(MkmApiCallStatistic);
            });

            mkmTask.Wait();
            scryfallCardDownload.Wait();

            RebuildInternalDatabase();

            UpdateProductSummary();
            stopwatch.Stop();

            Log.Information($"Updating complete database took {stopwatch.Elapsed}");
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

                    _cardDatabase.RebuildDetailedDatabase();
                    UpdateProductSummary();

                    stopwatch.Stop();
                    Log.Information($"{nameof(RebuildInternalDatabase)}: Rebuild done in {stopwatch.Elapsed}");
                }
                finally
                {
                    _isUpdatingDetailedCards = false;
                }
            });
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
            if (product == null)
            {
                return;
            }

            var prefix = $"{nameof(OpenMkmProductPage)}({product.Id} {product.NameEn} {product.SetCode})";

            var url = _cardDatabase.FindAdditionalMkmInfo(product.MkmId)?.MkmWebSite;

            if (string.IsNullOrEmpty(url))
            {
                Log.Information($"{prefix}: Downloading additional info...");

                // TODO: Update detailed card

                // We need to download the product details first
                if (string.IsNullOrEmpty(product.MkmId))
                {
                    Log.Warning($"Card {product} does not exist on MKM. ");
                    return;
                }
                url = _mkmRequest.GetProductData(product.MkmId)?.WebSite;
                _cardDatabase.UpdateMkmAdditionalInfo(product.MkmId, url);

                _cardDatabase.UpdateMkmStatistics(MkmApiCallStatistic);
            }

            // Now open a browser with the url
            Log.Debug($"{prefix}: Opening MKM product page...");
            Browser.OpenBrowser(url);
        }

        public IEnumerable<DetailedMagicCard> FindDetailedCardsByName(QueryCardOptions query)
        {
            Log.Debug($"{nameof(FindDetailedCardsByName)}: {query}");

            var databaseQuery = _cardDatabase.MagicCards
                .Query();

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

            return
                databaseQuery
                .ToList()
                .OrderBy(p => p.NameEn)
                .ThenBy(p => p.SetName)
                .ToList();
        }

        public void EnrichDeckListWithDetails(DeckList deckList)
        {
            Log.Debug($"{nameof(EnrichDeckListWithDetails)} for deck {deckList.Name}");

            foreach (var card in deckList.Mainboard.Where(c => c.CardId == null))
            {
                var found = _cardDatabase.MagicCards
                    .Query()
                    .Where(c => c.NameEn.Equals(card.Name))
                    .Where(c => c.SetCode != null)
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

        public IEnumerable<MkmStockItemExtended> DownloadMkmStock()
        {
            Log.Debug($"{nameof(DownloadMkmStock)} now...");

            var result = _mkmRequest
                .GetStockAsCsv()
                .Select(s => new MkmStockItemExtended(s))
                .ToArray();

            _cardDatabase.UpdateMkmStatistics(MkmApiCallStatistic);

            Log.Debug($"{nameof(DownloadMkmStock)} loaded {result.Length} items");

            return result;
        }

        private void UpdateProductSummary()
        {
            MkmProductsSummary = $"{_cardDatabase.MkmProductInfo.Count()} products in {_cardDatabase.MkmExpansion.Count()} sets";
            ScryfallProductsSummary = $"{_cardDatabase.ScryfallCards.Count()} cards in {_cardDatabase.ScryfallSets.Count()} sets";
            InternalProductsSummary = $"{_cardDatabase.MagicCards.Count()} cards";
        }
    }
}