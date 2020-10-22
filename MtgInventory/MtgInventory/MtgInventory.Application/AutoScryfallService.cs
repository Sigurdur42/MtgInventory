using System;
using System.Linq;
using MtgBinder.Domain.Scryfall;
using MtgInventory.Service.Database;
using MtgInventory.Service.Models;
using MtgInventory.Service.Settings;
using Serilog;

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

        public AutoScryfallService(
            CardDatabase cardDatabase,
            IScryfallService scryfallService,
            ISettingsService settingService)
        {
            _cardDatabase = cardDatabase;
            _scryfallApi = scryfallService;
            _settingService = settingService;
        }

        public CardPrice AutoDownloadPrice(
            string name,
            string setCode,
            Guid scryfallId)
        {
            if (Guid.Empty == scryfallId)
            {
                Log.Warning($"No valid scryfall id - skipping price download for {name}-{setCode}");

                return new CardPrice();
            }

            var latestPrice = GetLatestPrice(scryfallId);

            if (latestPrice.UpdateDate.Value.AddDays(_settingService.Settings.RefreshPriceAfterDays) < DateTime.Now)
            {
                Log.Information($"Price for scryfall {name}-{setCode} is outdated - downloading current one");

                var result = _scryfallApi
                    .RetrieveCardsForSetCode(setCode)
                    .Select(c => new CardPrice(new ScryfallCard(c)))
                    .ToArray();

                if (result.Any())
                {
                    _cardDatabase.CardPrices.Insert(result);
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