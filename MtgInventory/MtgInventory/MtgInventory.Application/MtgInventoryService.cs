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
        }

        public void ShutDown()
        {
            Log.Information($"{nameof(ShutDown)}: Shutting down application service");

            _cardDatabase.Dispose();
        }

        public void DownloadMkmProducts()
        {
            var stopwatch = Stopwatch.StartNew();

            _cardDatabase.ClearDetailedCards();

            var scryfallCardDownload = Task.Factory.StartNew(() =>
            {
                Log.Debug($"{nameof(DownloadMkmProducts)}: Loading Scryfall expansions...");
                var scryfallSets = _scryfallService.RetrieveSets().ToArray().OrderByDescending(s=>s.Name).ToArray();
                _cardDatabase.InsertScryfallSets(scryfallSets);

                _cardDatabase.ClearScryfallCards();
                var remainingSets = scryfallSets.Length;
                foreach (var set in scryfallSets)
                {
                    Log.Debug($"{nameof(DownloadMkmProducts)}: Loading Scryfall cards for set {set.Code} ({remainingSets} remaining)...");
                    var cards = _scryfallService.RetrieveCardsForSetCode(set.Code);
                    _cardDatabase.InsertScryfallCards(cards);
                    remainingSets--;
                }

                Log.Debug($"{nameof(DownloadMkmProducts)}: Done loading Scryfall cards ...");
            });

            var mkmTask = Task.Factory.StartNew(() =>
            {
                Log.Debug($"{nameof(DownloadMkmProducts)}: Loading MKM expansions...");

                var expansions = _mkmRequest.GetExpansions(1);
                _cardDatabase.InsertExpansions(expansions);

                Log.Debug($"{nameof(DownloadMkmProducts)}: Loading MKM products...");
                using var products = _mkmRequest.GetProductsAsCsv();

                Log.Debug($"{nameof(DownloadMkmProducts)}: Inserting products into database...");
                _cardDatabase.InsertProductInfo(products.Products, expansions);

                _cardDatabase.UpdateMkmStatistics(MkmApiCallStatistic);
            });

            mkmTask.Wait();
            scryfallCardDownload.Wait();

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

            if (string.IsNullOrEmpty(product.MkmWebSite))
            {
                Log.Information($"{prefix}: Downloading additional info...");

                // TODO: Update detailed card

                // We need to download the product details first
                if (string.IsNullOrEmpty(product.MkmId))
                {
                    Log.Warning($"Card {product} does not exist on MKM. ");
                    return;
                }
                var p = _mkmRequest.GetProductData(product.MkmId);
                product.UpdateFromProduct(p);

                _cardDatabase.MagicCards.Update(product);

                _cardDatabase.UpdateMkmStatistics(MkmApiCallStatistic);
            }

            // Now open a browser with the url
            Log.Debug($"{prefix}: Opening MKM product page...");
            Browser.OpenBrowser(product.MkmWebSite);
        }

        public IEnumerable<DetailedMagicCard> MkmFindProductsByName(string name)
        {
            Log.Debug($"{nameof(MkmFindProductsByName)}: {name}");
            return _cardDatabase.MagicCards
                .Query()
                // .Where(p => p.CategoryId == 1)
                .Where(p => p.NameEn.Contains(name, StringComparison.InvariantCultureIgnoreCase))
                .ToList()
                .OrderBy(p => p.NameEn)
                .ThenBy(p => p.SetName)
                .ToList();
        }

        public void EnrichDeckListWithDetails(DeckList deckList)
        {
            Log.Debug($"{nameof(EnrichDeckListWithDetails)} for deck {deckList.Name}");
            var expansions = _cardDatabase.MkmExpansion
                .Query()
                .OrderByDescending(e => e.ReleaseDateParsed)
                .ToList();

            foreach (var card in deckList.Mainboard.Where(c => c.MkmId == null))
            {
                var found = _cardDatabase.MkmProductInfo
                    .Query()
                    .Where(c => c.Name.Equals(card.Name))
                    .Where(c => c.ExpansionCode != null)
                    .ToList();

                var validExpansions = expansions
                    .Where(e => found.Any(f => f.ExpansionCode == e.Abbreviation))
                    .OrderByDescending(e => e.ReleaseDateParsed)
                    .ToArray();

                if (found.Any())
                {
                    var use = found.First(f => f.ExpansionCode == validExpansions.First().Abbreviation);
                    card.MkmId = use.Id;
                    card.SetCode = use.ExpansionCode;
                    card.SetName = use.ExpansionName;
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