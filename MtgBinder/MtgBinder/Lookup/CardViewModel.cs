using MtgBinder.Domain.Database;
using MtgDomain;

namespace MtgBinder.Lookup
{
    public class CardViewModel
    {
        private readonly ICardDatabase _database;
        private CardPrice _price;

        public CardViewModel(CardInfo card, ICardDatabase database)
        {
            _database = database;
            Card = card;
        }

        public CardInfo Card { get; }

        public CardPrice Price
        {
            get { return _price ??= _database.LookupLatestPrice(Card.ScryfallId); }
        }

        public string DisplayPriceEur => Price?.Eur?.ToString("F2") ?? "---";
        public string DisplayPriceEurFoil => Price?.EurFoil?.ToString("F2") ?? "---";
        public string DisplayPriceUsd => Price?.Usd?.ToString("F2") ?? "---";
        public string DisplayPriceUsdFoil => Price?.UsdFoil?.ToString("F2") ?? "---";
        public string DisplayPriceTix => Price?.Tix?.ToString("F2") ?? "---";
    }
}