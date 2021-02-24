using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LocalSettings;
using Microsoft.Extensions.Logging;
using MtgDatabase.Models;
using MtgJson;

namespace MtgDatabase.MtgJson
{
    public class MirrorMtgJson : IMirrorMtgJson
    {
        public const string SettingCardDate = "MtgJsonCardUpdated";
        public const string SettingCardOutdatedAfterDays = "MtgJsonCardOutdatedAfterDays";
        public const string SettingPriceDate = "MtgJsonPriceUpdated";
        public const string SettingPriceOutdatedAfterDays = "MtgJsonPriceOutdatedAfterDays";
        private readonly IMtgJsonService _jsonService;
        private readonly ILogger<MirrorMtgJson> _logger;
        private readonly ILocalSettingService _settingService;
        private DateTime _cardsUpdated;

        private DateTime _priceUpdated;

        public MirrorMtgJson(
            IMtgJsonService jsonService,
            ILogger<MirrorMtgJson> logger,
            ILocalSettingService settingService)
        {
            _jsonService = jsonService;
            _logger = logger;
            _settingService = settingService;

            _cardsUpdated = _settingService.GetComplexValue(SettingCardDate, DateTime.MinValue);
            _priceUpdated = _settingService.GetComplexValue(SettingPriceDate, DateTime.MinValue);
        }

        public bool AreCardsOutdated
        {
            get
            {
                var cardDays = _settingService.GetInt(SettingCardOutdatedAfterDays);
                if (cardDays < 2)
                {
                    cardDays = 7;
                    _settingService.Set(SettingCardOutdatedAfterDays, cardDays);
                }

                return _cardsUpdated.AddDays(cardDays).Date <= DateTime.Today.Date;
            }
        }

        public bool IsPriceOutdated
        {
            get
            {
                var cardDays = _settingService.GetInt(SettingPriceOutdatedAfterDays);
                if (cardDays < 1)
                {
                    cardDays = 1;
                    _settingService.Set(SettingPriceOutdatedAfterDays, cardDays);
                }

                return _cardsUpdated.AddDays(cardDays).Date <= DateTime.Today.Date;
            }
        }

        public async Task<IList<QueryableMagicCard>> DownloadDatabase(bool force)
        {
            if (!force && !AreCardsOutdated)
            {
                _logger.LogInformation($"Skipping card database mirror - cards are not outdated yet.");
                return new List<QueryableMagicCard>();
            }

            var tempFile = Path.GetTempFileName();
            try
            {
                _logger.LogInformation("Downloading AllPrintings now");
                using var httpClient = new HttpClient();
                using var response = await httpClient.GetAsync("https://mtgjson.com/api/v5/AllPrintingsCSVFiles.zip");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to download file: {response.StatusCode}");
                    return new List<QueryableMagicCard>();
                }

                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    await using var fileStream = File.Create(tempFile);
                    await stream.CopyToAsync(fileStream);
                }

                _logger.LogInformation($"Downloaded AllPrintingsCSVFiles to temp folder - starting analysis");

                DateTime cardDate = _cardsUpdated;

                var cardFactory = new MtgJsonCardFactory();

                _jsonService.DownloadAllPrintingsZip(
                    new FileInfo(tempFile),
                    (header) =>
                    {
                        // TODO
                        // cardDate = DateTime.Parse(header.Date, );
                        return true;
                        // return _mtgJsonLiteDbService.OnPriceDataHeaderLoaded(header);
                    },
                    (sets) =>
                    {
                        cardFactory.LoadedSets = sets.ToArray();
                        return true;
                    },
                    (cards) =>
                    {
                        cardFactory.AllCards = cards.ToDictionary(c => c.Id);
                        return true;
                    },
                    (foreignData) =>
                    {
                        cardFactory.ForeignByCard = foreignData.GroupBy(f => f.CardId).ToArray();
                        return true;
                    },
                    (legalities) =>
                    {
                        cardFactory.LegalitiesByCard = legalities.GroupBy(f => f.CardId).ToArray();
                        return true;
                    });

                _cardsUpdated = DateTime.Now;
                _settingService.SetComplexValue(SettingCardDate, _cardsUpdated);
                return cardFactory.CreateCards();
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        public async Task<IList<QueryableMagicCard>> UpdatePriceData(IList<QueryableMagicCard> allCards, bool force)
        {
            if (!force && !IsPriceOutdated)
            {
                _logger.LogInformation($"Skipping card price mirror - cards are not outdated yet.");
                return allCards;
            }

            var byCardId = allCards.ToDictionary(c => c.Id);
            var insertTasks = new List<Task>();

            await _jsonService.DownloadPriceDataAsync(
                    //new FileInfo(@"C:\pCloudSync\MtgInventory\AllPrices.json"),
                    (header) =>
                    {
                        // Console.WriteLine($"Header: Header: {header.Date} - Version: {header.Version}");
                        return true;
                    },
                    (filteredBatch) =>
                    {
                        var filteredArray = filteredBatch.ToArray();
                        var insertTask = Task.Factory.StartNew(() =>
                        {
                            foreach (var jsonCardPrice in filteredArray)
                            {
                                if (!byCardId.TryGetValue(jsonCardPrice.Id, out var card))
                                {
                                    continue;
                                }

                                // We can do MKM only at this time
                                foreach (var jsonCardPriceItem in jsonCardPrice.Items)
                                {
                                    if (jsonCardPriceItem.IsFoil.Equals("foil", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        card.EurFoil = (decimal)jsonCardPriceItem.Price;
                                    }
                                    else
                                    {
                                        card.Eur = (decimal)jsonCardPriceItem.Price;
                                    }
                                }
                            }

                            ////var items = filteredBatch
                            ////    .AsParallel()
                            ////    .SelectMany(i =>
                            ////    {
                            ////        return i.Items.ToArray().Select(b => new DbPriceItem()
                            ////        {
                            ////            CardId = i.Id,
                            ////            Date = b.Date,
                            ////            BuylistOrRetail = b.BuylistOrRetail,
                            ////            Currency = b.Currency,
                            ////            IsFoil = b.IsFoil == "foil",
                            ////            PaperOrOnline = b.PaperOrOnline,
                            ////            Price = b.Price,
                            ////            Seller = b.Seller,
                            ////            Type = b.Type
                            ////        }).ToArray();
                            ////    })
                            ////    .ToArray();

                            // _logger.LogInformation($"Inserting {items.Length} price rows...");
                        });

                        insertTasks.Add(insertTask);
                    },
                    new MtgJsonPriceFilter());

            while (insertTasks.Any())
            {
                var task = insertTasks.FirstOrDefault();
                if (task != null)
                {
                    _logger.LogInformation($"{insertTasks.Count} insert tasks still in queue");
                    task.Wait();
                    insertTasks.Remove(task);
                }
            }

            _priceUpdated = DateTime.Now;
            _settingService.SetComplexValue(SettingPriceDate, _priceUpdated);

            return allCards;
        }
    }
}