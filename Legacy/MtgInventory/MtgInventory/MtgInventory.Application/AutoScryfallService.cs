using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using MtgInventory.Service.Database;
using MtgInventory.Service.Models;
using MtgInventory.Service.Settings;
using ScryfallApiServices;
using ScryfallApiServices.Models;

namespace MtgInventory.Service
{
    public interface IAutoScryfallService
    {
        CardPrice AutoDownloadPrice(string name, string setCode, Guid scryfallId);

        CardPrice GetLatestPrice(Guid scryfallId);
    }

    public sealed class AutoScryfallService : IAutoScryfallService
    {
        private readonly CardDatabase _cardDatabase;
        private readonly IScryfallService _scryfallApi;
        private readonly ISettingsService _settingService;
        private readonly ILogger _logger;

        public AutoScryfallService(
            ILoggerFactory loggerFactory,
            CardDatabase cardDatabase,
            IScryfallService scryfallService,
            ISettingsService settingService)
        {
            _cardDatabase = cardDatabase;
            _scryfallApi = scryfallService;
            _settingService = settingService;
            _logger = loggerFactory.CreateLogger<AutoScryfallService>();
        }

        public CardPrice AutoDownloadPrice(
            string name,
            string setCode,
            Guid scryfallId)
        {
            if (Guid.Empty == scryfallId)
            {
                _logger.LogWarning($"No valid scryfall id - skipping price download for {name}-{setCode}");

                return new CardPrice();
            }

            var latestPrice = GetLatestPrice(scryfallId);

            if (latestPrice.UpdateDate.Value.AddDays(_settingService.Settings.RefreshPriceAfterDays) < DateTime.Now)
            {
                _logger.LogInformation($"Price for scryfall {name}-{setCode} is outdated - downloading current one");

                var result = _scryfallApi
                    .RetrieveCardsForSetCode(setCode)
                    .Select(c => new CardPrice(new ScryfallCard(c)))
                    .ToArray();

                if (result.Any())
                {
                    _cardDatabase.CardPrices?.Insert(result);
                    _cardDatabase.EnsureCardPriceIndex();
                }

                latestPrice = result.FirstOrDefault(c => c.ScryfallId == scryfallId);
            }

            return latestPrice;
        }

        public CardPrice GetLatestPrice(Guid scryfallId)
        {
            var query = _cardDatabase.CardPrices.Query();

            var result = query.Where(c => c.ScryfallId == scryfallId).OrderByDescending(p => p.UpdateDate).FirstOrDefault();

            return result ?? new CardPrice();
        }
    }
}