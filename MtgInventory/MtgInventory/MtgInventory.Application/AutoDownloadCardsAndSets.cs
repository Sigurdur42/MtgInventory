using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MkmApi;
using MkmApi.Entities;
using MtgBinder.Domain.Scryfall;
using MtgInventory.Service.Database;
using MtgInventory.Service.Models;
using MtgInventory.Service.Settings;
using ScryfallApiServices;

namespace MtgInventory.Service
{
    public class AutoDownloadCardsAndSets
    {
        private readonly CardDatabase _cardDatabase;
        private readonly ILogger _logger;
        private readonly IApiCallStatistic _mkmApiCallStatistic;
        private readonly IMkmRequest _mkmRequest;
        private readonly IScryfallApiCallStatistic _scryfallApiCallStatistic;
        private readonly IScryfallService _scryfallService;
        private readonly ISettingsService _settingsService;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private HashSet<string> _knownEmptyLookups = new HashSet<string>();
        private Task? _runningTask;

        internal AutoDownloadCardsAndSets(
            ILoggerFactory loggerFactory,
            ISettingsService settingsService,
            CardDatabase cardDatabase,
            IApiCallStatistic mkmApiCallStatistic,
            IScryfallApiCallStatistic scryfallApiCallStatistic,
            IScryfallService scryfallService,
            IMkmRequest mkmRequest)
        {
            _logger = loggerFactory.CreateLogger<AutoDownloadCardsAndSets>();
            _settingsService = settingsService;
            _cardDatabase = cardDatabase;
            _mkmApiCallStatistic = mkmApiCallStatistic;
            _scryfallApiCallStatistic = scryfallApiCallStatistic;
            _scryfallService = scryfallService;
            _mkmRequest = mkmRequest;

            CardsUpdated += (sender, e) => { };
            SetsUpdated += (sender, e) => { };
        }

        public event EventHandler CardsUpdated;

        public event EventHandler SetsUpdated;

        public void AutoDownloadMkmDetails(
            MkmAuthenticationData authenticationData,
            DetailedSetInfo set)
        {
            var cardsOfSet = _cardDatabase?.MagicCards
                ?.Query()
                ?.Where(c => c.SetCode == set.SetCode)
                ?.ToArray() ?? new DetailedMagicCard[0];

            AutoDownloadMkmDetails(authenticationData, cardsOfSet, "");
        }

        public void AutoDownloadMkmDetails(
            MkmAuthenticationData authenticationData,
            DetailedMagicCard[] cards,
            string queryName)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    foreach (var card in cards.Where(c => !string.IsNullOrWhiteSpace(c.MkmId)).Distinct())
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

                        _logger.LogDebug($"Downloading additional MKM info for {card.SetCodeMkm} - {card.NameEn}");

                        var alreadyKnownEmpty = _knownEmptyLookups.Contains(queryName);
                        var downloadSuccessful = true;

                        if (alreadyKnownEmpty && !string.IsNullOrEmpty(queryName))
                        {
                            DownloadMkmAdditionalDataById(authenticationData, card.MkmId);
                            return;
                        }

                        // Now download all details for this card:
                        if (!card.IsBasicLand)
                        {
                            if (string.IsNullOrWhiteSpace(queryName))
                            {
                                downloadSuccessful = DownloadMkmAdditionalData(authenticationData, card.NameEn, true);
                            }
                            else
                            {
                                downloadSuccessful = DownloadMkmAdditionalData(authenticationData, queryName, false);
                            }
                        }
                        else
                        {
                            downloadSuccessful = false;
                        }

                        if (!downloadSuccessful)
                        {
                            _knownEmptyLookups.Add(queryName);
                            _knownEmptyLookups.Add(card.NameEn);

                            if (!string.IsNullOrEmpty(card.MkmId))
                            {
                                DownloadMkmAdditionalDataById(authenticationData, card.MkmId);
                            }
                        }
                    }

                    _logger.LogDebug($"Done dlownloading additional MKM info...");
                }
                catch (Exception error)
                {
                    _logger.LogError($"AutoDownload MKM caught exception: {error}");
                }
                finally
                {
                    this.CardsUpdated?.Invoke(this, EventArgs.Empty);
                }
            });
        }

        public void DownloadMkmSetsAndProducts()
        {
            if (!_settingsService.Settings.MkmAuthentication.IsValid())
            {
                _logger.LogWarning($"MKM authentication configuration is missing - cannot access MKM API.");
                return;
            }

            _logger.LogDebug($"Loading MKM expansions...");

            var expansions = _mkmRequest.GetExpansions(_settingsService.Settings.MkmAuthentication, 1).ToArray();
            _cardDatabase.InsertExpansions(expansions);

            _logger.LogDebug($"Loading MKM products...");
            using var products = _mkmRequest.GetProductsAsCsv(_settingsService.Settings.MkmAuthentication);

            _logger.LogDebug($"Inserting products into database...");
            _cardDatabase.InsertProductInfo(products.Products, expansions);

            _cardDatabase.UpdateMkmStatistics(_mkmApiCallStatistic);

            CardsUpdated?.Invoke(this, EventArgs.Empty);
        }

        public ScryfallSet[] DownloadScryfallSetsData()
        {
            _logger.LogDebug($"Loading Scryfall expansions...");
            var scryfallSets = _scryfallService.RetrieveSets()
                .Where(s => !s.IsDigital)
                .OrderByDescending(s => s.Name)
                .Select(s => new ScryfallSet(s))
                .ToArray();

            _cardDatabase.InsertScryfallSets(scryfallSets);

            _cardDatabase.UpdateScryfallStatistics(_scryfallApiCallStatistic);

            _logger.LogDebug($"Done loading Scryfall expansions...");

            CardsUpdated?.Invoke(this, EventArgs.Empty);
            SetsUpdated?.Invoke(this, EventArgs.Empty);

            return scryfallSets;
        }

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

        private bool DownloadMkmAdditionalData(
            MkmAuthenticationData authenticationData,
            string cardName,
            bool exactNameMatch)
        {
            _logger.LogInformation($"Downloading MKM data for '{cardName}'");
            var productData = _mkmRequest.FindProducts(authenticationData, cardName, exactNameMatch).ToArray();
            _logger.LogInformation($"{productData.Length} MKM data for '{cardName}' found");

            if (productData.Length == 0)
            {
                return false;
            }

            UpdateFromMkmProducts(productData);

            return true;
        }

        private void DownloadMkmAdditionalDataById(
            MkmAuthenticationData authenticationData,
            string mkmId)
        {
            _logger.LogInformation($"Downloading MKM data for id '{mkmId}'");
            var productData = _mkmRequest.GetProductData(authenticationData, mkmId);
            UpdateFromMkmProducts(new[] { productData });
        }

        private void DownloadScryfallCardsForSet(DetailedSetInfo set)
        {
            if (string.IsNullOrWhiteSpace(set.SetCodeScryfall))
            {
                // This is a MKM only set
                return;
            }

            // Delete Scryfall cards for this set
            _cardDatabase?.ScryfallCards?.DeleteMany(c => c.Set == set.SetCodeScryfall);

            // Download new cards
            var cards = _scryfallService.RetrieveCardsForSetCode(set.SetCodeScryfall)
                .Select(c => new ScryfallCard(c))
                .ToArray();

            if (!cards.Any())
            {
                // Assume that the down´load failed - do not insert and do not mark as complete
                return;
            }

            _cardDatabase.InsertScryfallCards(cards);

            var prices = cards.Select(c => new CardPrice(c));
            _cardDatabase?.CardPrices?.InsertBulk(prices);
            _cardDatabase?.EnsureCardPriceIndex();

            CardsUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void RunAutoDownload(CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation($" Starting auto download");
            try
            {
                // Sets are always updated as a whole
                var knownSets = _cardDatabase.MagicSets
                    ?.Query()
                    ?.OrderBy(s => s.SetLastUpdated)
                    ?.ToArray();

                var lastDownloaded = knownSets
                    ?.OrderBy(s => s.SetLastDownloaded)?.FirstOrDefault() ?? new DetailedSetInfo();

                var lastUpdated = knownSets
                    ?.FirstOrDefault() ?? new DetailedSetInfo();

                var isSetDataDownloadRequired = lastDownloaded.SetLastDownloaded.AddDays(_settingsService.Settings.RefreshSetDataAfterDays).Date <= DateTime.Now.Date;
                var isSetDataOutdated = isSetDataDownloadRequired || lastUpdated.SetLastUpdated.AddDays(_settingsService.Settings.RefreshSetDataAfterDays).Date <= DateTime.Now.Date;

                // Check set updates - mkm and Scryfall are done in one go
                if (isSetDataDownloadRequired)
                {
                    // Sets are too old - update
                    _logger.LogInformation($" Sets are too old - will update now");

                    DownloadMkmSetsAndProducts();
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    DownloadScryfallSetsData();
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    _cardDatabase.RebuildSetData();
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    SetsUpdated?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    _logger.LogDebug($" Set informations do not need to be downloaded");
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
                    _logger.LogDebug($" Set informations are up to date");
                }

                var allSets = _cardDatabase.MagicSets
                    ?.FindAll()
                    ?.OrderByDescending(s => s.ReleaseDateParsed)
                    ?.ToArray() ?? new DetailedSetInfo[0];

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
                        _logger.LogDebug($" Downloading cards for set {detailedSetInfo.SetCode} ({detailedSetInfo.SetName})...");

                        // Cards need to be downloaded again
                        DownloadScryfallCardsForSet(detailedSetInfo);
                        detailedSetInfo.CardsLastDownloaded = DateTime.Now;
                        _cardDatabase?.MagicSets?.Update(detailedSetInfo);

                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }
                    }

                    if (cardsUpdatedOutdated)
                    {
                        _logger.LogDebug($" Rebuilding cards for set {detailedSetInfo.SetCode}...");

                        _cardDatabase?.RebuildCardsForSet(detailedSetInfo);
                        detailedSetInfo.CardsLastUpdated = DateTime.Now;
                        _cardDatabase?.MagicSets?.Update(detailedSetInfo);
                    }
                }

                CardsUpdated?.Invoke(this, EventArgs.Empty);
                SetsUpdated?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation($" Finished auto download in {stopwatch.Elapsed}");
            }
        }

        private void UpdateFromMkmProducts(Product[] productData)
        {
            var remaining = productData.Length;
            foreach (var product in productData)
            {
                var detailArray = _cardDatabase?.MagicCards
                    ?.Query()
                    ?.Where(c => c.MkmId == product.IdProduct)
                    ?.ToArray();

                foreach (var details in detailArray)
                {
                    _logger.LogInformation($"remaining: {--remaining}: Updating details for '{details?.SetCode}' '{product.NameEn}'");

                    if (details != null)
                    {
                        details.UpdateFromMkm(product);
                        _cardDatabase?.MagicCards?.Update(details);
                    }

                    var additional = _cardDatabase?.MkmAdditionalInfo
                        ?.Query()
                        ?.Where(c => c.MkmId == product.IdProduct)
                        ?.FirstOrDefault();

                    if (additional == null)
                    {
                        additional = new MkmAdditionalCardInfo() { MkmId = product.IdProduct };
                        _cardDatabase?.MkmAdditionalInfo?.Insert(additional);
                    }

                    additional.UpdateFromProduct(product);
                    _cardDatabase?.MkmAdditionalInfo?.Update(additional);
                }
            }
        }
    }
}