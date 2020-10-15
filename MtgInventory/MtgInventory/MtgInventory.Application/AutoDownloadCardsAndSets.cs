using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MkmApi;
using MtgBinder.Domain.Scryfall;
using MtgInventory.Service.Database;
using MtgInventory.Service.Models;
using MtgInventory.Service.Settings;
using ScryfallApiServices;
using Serilog;

namespace MtgInventory.Service
{
    public class AutoDownloadCardsAndSets
    {
        private const string _logPrefix = "[ADL] ";
        private readonly ISettingsService _settingsService;
        private readonly CardDatabase _cardDatabase;
        private readonly IApiCallStatistic _mkmApiCallStatistic;
        private readonly IScryfallApiCallStatistic _scryfallApiCallStatistic;
        private readonly IScryfallService _scryfallService;
        private readonly MkmRequest _mkmRequest;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private Task? _runningTask;

        public AutoDownloadCardsAndSets(
            ISettingsService settingsService,
            CardDatabase cardDatabase,
            IApiCallStatistic mkmApiCallStatistic,
            IScryfallApiCallStatistic scryfallApiCallStatistic,
            IScryfallService scryfallService,
            MkmRequest mkmRequest)
        {
            _settingsService = settingsService;
            _cardDatabase = cardDatabase;
            _mkmApiCallStatistic = mkmApiCallStatistic;
            _scryfallApiCallStatistic = scryfallApiCallStatistic;
            _scryfallService = scryfallService;
            _mkmRequest = mkmRequest;

            CardsUpdated += (sender, e) => { };
        }

        public event EventHandler CardsUpdated;

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _runningTask = Task.Factory.StartNew(
                () => RunAutoDownload(_cancellationTokenSource.Token),
                _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            if (_runningTask != null && _runningTask.Status == TaskStatus.Running)
            {
                _cancellationTokenSource.Cancel(false);
                _runningTask.Wait();
            }
        }

        public ScryfallSet[] DownloadScryfallSetsData(bool rebuildDetailedSetInfo)
        {
            Log.Debug($"{_logPrefix}Loading Scryfall expansions...");
            var scryfallSets = _scryfallService.RetrieveSets()
                .OrderByDescending(s => s.Name)
                .Select(s => new ScryfallSet(s))
                .ToArray();

            _cardDatabase.InsertScryfallSets(scryfallSets);

            if (rebuildDetailedSetInfo)
            {
                _cardDatabase.RebuildSetData();
            }
            _cardDatabase.UpdateScryfallStatistics(_scryfallApiCallStatistic);

            Log.Debug($"{_logPrefix}Done loading Scryfall expansions...");

            CardsUpdated?.Invoke(this, EventArgs.Empty);

            return scryfallSets;
        }

        public void DownloadMkmSetsAndProducts()
        {
            if (!_settingsService.Settings.MkmAuthentication.IsValid())
            {
                Log.Warning($"{_logPrefix}MKM authentication configuration is missing - cannot access MKM API.");
                return;
            }

            Log.Debug($"{_logPrefix}Loading MKM expansions...");

            var mkmRequest = new MkmRequest(_mkmApiCallStatistic);
            var expansions = mkmRequest.GetExpansions(_settingsService.Settings.MkmAuthentication, 1).ToArray();
            _cardDatabase.InsertExpansions(expansions);

            Log.Debug($"{_logPrefix}Loading MKM products...");
            using var products = mkmRequest.GetProductsAsCsv(_settingsService.Settings.MkmAuthentication);

            Log.Debug($"{_logPrefix}Inserting products into database...");
            _cardDatabase.InsertProductInfo(products.Products, expansions);

            _cardDatabase.UpdateMkmStatistics(_mkmApiCallStatistic);

            CardsUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void AutoDownloadMkmDetails(
            MkmAuthenticationData authenticationData,
            DetailedMagicCard[] cards,
            string queryName)
        {
            Task.Factory.StartNew(() =>
            {
                // TODO: Cancellation
                foreach (var card in cards.Where(c=>!string.IsNullOrWhiteSpace(c.MkmId)))
                {
                    if (!card.MkmDetailsRequired)
                    {
                        // All details do exist
                        continue;
                    }

                    // refresh to ensure that a previous call did not update this
                    var refreshed = _cardDatabase.MagicCards.Find(c => c.Id == card.Id).First();
                    if (!refreshed.MkmDetailsRequired)
                    {
                        continue;
                    }

                    // Now download all details for this card:
                    if (string.IsNullOrWhiteSpace(queryName))
                    {
                        DownloadMkmAdditionalData(authenticationData, card.NameEn);
                    }
                    else
                    {
                        DownloadMkmAdditionalData(authenticationData, queryName);
                    }
                }
            });
        }

        public void DownloadMkmAdditionalData(
            MkmAuthenticationData authenticationData,
            string cardName)
        {
            Log.Information($"Downloading MKM data for '{cardName}'");
            var productData = _mkmRequest.FindProducts(authenticationData, cardName, false).ToArray();
            Log.Information($"{productData.Length} MKM data for '{cardName}' found");

            var remaining = productData.Length;
            foreach (var product in productData)
            {
                var details = _cardDatabase.MagicCards.Query()
                    .Where(c => c.MkmId == product.IdProduct)
                    .FirstOrDefault();

                Log.Information($"remaining: {--remaining}: Updating details for '{details?.SetCode}' '{product.NameEn}'");

                if (details != null)
                {
                    details.UpdateFromMkm(product);
                    _cardDatabase.MagicCards.Update(details);
                }

                var additional = _cardDatabase.MkmAdditionalInfo
                    .Query()
                    .Where(c => c.MkmId == product.IdProduct)
                    .FirstOrDefault();

                if (additional == null)
                {
                    additional = new MkmAdditionalCardInfo()
                    {
                        MkmId = product.IdProduct
                    };
                    _cardDatabase.MkmAdditionalInfo.Insert(additional);
                }

                additional.UpdateFromProduct(product);
                _cardDatabase.MkmAdditionalInfo.Update(additional);
            }
        }

        private void RunAutoDownload(CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            Log.Information($"{_logPrefix} Starting auto download");
            try
            {
                // Sets are always updated as a whole
                var knownSets = _cardDatabase.MagicSets
                    .Query()
                    .OrderBy(s => s.SetLastUpdated)
                    .ToArray();

                var lastDownloaded = knownSets
                    .OrderBy(s => s.SetLastDownloaded).FirstOrDefault() ?? new DetailedSetInfo();

                var lastUpdated = knownSets
                    .FirstOrDefault() ?? new DetailedSetInfo();

                var isSetDataDownloadRequired = lastDownloaded.SetLastDownloaded.AddDays(_settingsService.Settings.RefreshSetDataAfterDays).Date <= DateTime.Now.Date;
                var isSetDataOutdated = isSetDataDownloadRequired || lastUpdated.SetLastUpdated.AddDays(_settingsService.Settings.RefreshSetDataAfterDays).Date <= DateTime.Now.Date;

                // Check set updates - mkm and Scryfall are done in one go
                if (isSetDataDownloadRequired)
                {
                    // Sets are too old - update
                    Log.Information($"{_logPrefix} Sets are too old - will update now");

                    DownloadMkmSetsAndProducts();
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    DownloadScryfallSetsData(true);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                }
                else
                {
                    Log.Debug($"{_logPrefix} Set informations do not need to be downloaded");
                }

                if (isSetDataOutdated)
                {
                    _cardDatabase.RebuildSetData();
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                }
                else
                {
                    Log.Debug($"{_logPrefix} Set informations are up to date");
                }

                var allSets = _cardDatabase.MagicSets.FindAll().OrderBy(s => s.ReleaseDateParsed).ToArray();
                foreach (var detailedSetInfo in allSets)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var cardsDownloadOutdated = detailedSetInfo.CardsLastDownloaded.AddDays(_settingsService.Settings.RefreshSetDataAfterDays).Date <= DateTime.Now.Date;
                    var cardsUpdatedOutdated = cardsDownloadOutdated || detailedSetInfo.CardsLastUpdated.AddDays(_settingsService.Settings.RefreshSetDataAfterDays).Date <= DateTime.Now.Date;

                    if (cardsDownloadOutdated)
                    {
                        Log.Debug($"{_logPrefix} Downloading cards for set {detailedSetInfo.SetCode}...");

                        // Cards need to be downloaded again
                        DownloadScryfallCardsForSet(detailedSetInfo);
                        detailedSetInfo.CardsLastDownloaded = DateTime.Now;
                        _cardDatabase.MagicSets.Update(detailedSetInfo);

                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }
                    }

                    if (cardsUpdatedOutdated)
                    {
                        Log.Debug($"{_logPrefix} Rebuilding cards for set {detailedSetInfo.SetCode}...");

                        _cardDatabase.RebuildCardsForSet(detailedSetInfo);
                        detailedSetInfo.CardsLastUpdated = DateTime.Now;
                        _cardDatabase.MagicSets.Update(detailedSetInfo);
                    }

                    if (cardsUpdatedOutdated)
                    {
                        CardsUpdated?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            finally
            {
                stopwatch.Stop();
                Log.Information($"{_logPrefix} Finished auto download in {stopwatch.Elapsed}");
            }
        }

        private void DownloadScryfallCardsForSet(DetailedSetInfo set)
        {
            if (string.IsNullOrWhiteSpace(set.SetCodeScryfall))
            {
                // This is a MKM only set
                return;
            }

            // Delete Scryfall cards for this set
            _cardDatabase.ScryfallCards.DeleteMany(c => c.Set == set.SetCodeScryfall);

            // Download new cards
            var cards = _scryfallService.RetrieveCardsForSetCode(set.SetCodeScryfall)
                .Select(c => new ScryfallCard(c))
                .ToArray();
            _cardDatabase.InsertScryfallCards(cards);

            var prices = cards.Select(c => new CardPrice(c));
            _cardDatabase.CardPrices.InsertBulk(prices);
            _cardDatabase.EnsureCardPriceIndex();

            CardsUpdated?.Invoke(this, EventArgs.Empty);
        }
    }
}