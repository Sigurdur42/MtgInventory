using System;
using System.Linq;
using MtgBinder.Domain.Scryfall;
using MtgInventory.Service.Database;
using MtgInventory.Service.Models;
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

        public AutoScryfallService(
            CardDatabase cardDatabase,
            IScryfallService scryfallService)
        {
            _cardDatabase = cardDatabase;
            _scryfallApi = scryfallService;
        }

        public CardPrice AutoDownloadPrice(string name, string setCode, Guid scryfallId)
        {
            if (Guid.Empty  == scryfallId)
            {
                Log.Warning($"No valid scryfall id - skipping price download for {name}-{setCode}");

                return new CardPrice();
            }

            var latestPrice = GetLatestPrice(scryfallId);

            if (latestPrice == null
                || latestPrice.UpdateDate.Value.AddDays(1) > DateTime.Now
                || (!latestPrice.ScryfallEur.HasValue && latestPrice.Source == CardPriceSource.Scryfall))
            {
                Log.Information($"Price for scryfall {name}-{setCode} is outdated - downloading current one");

                var result = _scryfallApi
                    .RetrieveCardsByCardNameAndSet(name, setCode, ScryfallApi.Client.Models.SearchOptions.RollupMode.Prints)
                    .Select(c => new CardPrice(new ScryfallCard(c)))
                    .ToArray();

                if (result.Any())
                {
                    _cardDatabase.CardPrices.Insert(result);
                    _cardDatabase.EnsureCardPriceIndex();
                }

                latestPrice = result.FirstOrDefault(c => c.Id == scryfallId);
            }

            return latestPrice;
        }

        public CardPrice GetLatestPrice(Guid scryfallId)
        {
            CardPrice result = null;
            var query = _cardDatabase.CardPrices.Query();

            result = query.Where(c => c.ScryfallId == scryfallId).FirstOrDefault();

            return result;
        }
    }
}