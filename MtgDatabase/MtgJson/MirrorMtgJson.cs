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
    }
}