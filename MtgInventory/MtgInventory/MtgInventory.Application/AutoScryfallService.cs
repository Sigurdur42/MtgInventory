using System;
using System.Linq;
using MtgBinder.Domain.Scryfall;
using MtgInventory.Service.Database;
using MtgInventory.Service.Models;
using Serilog;

namespace MtgInventory.Service
{
    public sealed class AutoScryfallService
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

        public CardPrice AutoDownloadPrice(DetailedMagicCard card)
        {
            var latestPrice = GetLatestPrice(card.ScryfallId);

            // TODO: Define how old the price shall be max
            if (latestPrice == null
                || latestPrice.UpdateDate.Value.AddDays(1) > DateTime.Now
                || (!latestPrice.ScryfallEur.HasValue && latestPrice.Source == CardPriceSource.Scryfall))
            {
                Log.Information($"Price for scryfall {card.NameEn}-{card.SetCode} is outdated - downloading current one");

                var result = _scryfallApi
                    .RetrieveCardsByCardNameAndSet(card.NameEn, card.SetCode, ScryfallApi.Client.Models.SearchOptions.RollupMode.Prints)
                    .Select(c => new CardPrice(new ScryfallCard(c)))
                    .ToArray();

                if (result.Any())
                {
                    _cardDatabase.CardPrices.Insert(result);
                    _cardDatabase.EnsureCardPriceIndex();
                }

                latestPrice = result.FirstOrDefault(c => c.Id == card.ScryfallId);
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