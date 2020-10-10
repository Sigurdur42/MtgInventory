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
        private const string _logPrefix = "[ADL]";
        private readonly ISettingsService _settingsService;
        private readonly CardDatabase _cardDatabase;
        private readonly IApiCallStatistic _mkmApiCallStatistic;
        private readonly IScryfallApiCallStatistic _scryfallApiCallStatistic;
        private readonly IScryfallService _scryfallService;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private Task? _runningTask;

        public AutoDownloadCardsAndSets(
            ISettingsService settingsService,
            CardDatabase cardDatabase,
            IApiCallStatistic mkmApiCallStatistic,
            IScryfallApiCallStatistic scryfallApiCallStatistic,
            IScryfallService scryfallService)
        {
            _settingsService = settingsService;
            _cardDatabase = cardDatabase;
            _mkmApiCallStatistic = mkmApiCallStatistic;
            _scryfallApiCallStatistic = scryfallApiCallStatistic;
            _scryfallService = scryfallService;
        }

        public void Start()
        {
            _runningTask = Task.Factory.StartNew(
                () => RunAutoDownload(_cancellationTokenSource.Token),
                _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            if (_runningTask != null && _runningTask.Status == TaskStatus.Running)
            {
                _cancellationTokenSource.Cancel(false);
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
            var expansions = mkmRequest.GetExpansions(_settingsService.Settings.MkmAuthentication, 1);
            _cardDatabase.InsertExpansions(expansions);

            Log.Debug($"{_logPrefix}Loading MKM products...");
            using var products = mkmRequest.GetProductsAsCsv(_settingsService.Settings.MkmAuthentication);

            Log.Debug($"{_logPrefix}Inserting products into database...");
            _cardDatabase.InsertProductInfo(products.Products, expansions);

            _cardDatabase.UpdateMkmStatistics(_mkmApiCallStatistic);
        }

        private void RunAutoDownload(CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            Log.Information($"{_logPrefix} Starting auto download");
            try
            {
                // Sets are always updated as a whole
                var lastUpdated = _cardDatabase.MagicSets
                    .Query()
                    .OrderBy(s => s.LastUpdated)
                    .FirstOrDefault() ?? new DetailedSetInfo()
                    {
                        LastUpdated = DateTime.Now.AddDays(-1000)
                    };

                // Check set updates - mkm and Scryfall are done in one go
                if (lastUpdated.LastUpdated.AddDays(_settingsService.Settings.RefreshSetDataAfterDays).Date <= DateTime.Now.Date)
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

                    if (lastUpdated.CardsLastUpdated.AddDays(_settingsService.Settings.RefreshSetDataAfterDays).Date > DateTime.Now.Date)
                    {
                        continue;
                    }

                    // Cards need to be updated

                    DownloadScryfallCardsForSet(detailedSetInfo);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    _cardDatabase.RebuildCardsForSet(detailedSetInfo);
                    detailedSetInfo.CardsLastUpdated = DateTime.Now;
                    _cardDatabase.MagicSets.Update(detailedSetInfo);
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
        }
    }
}