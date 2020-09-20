using MtgInventory.Service.Models;
using ReactiveUI;

namespace MtgInventory.Models
{
    public class DetailedCardViewModel : ReactiveObject
    {
        private CardPrice _cardPrice;
        private decimal? _marketPrice;

        public DetailedCardViewModel(DetailedMagicCard detailedMagicCard)
        {
            Card = detailedMagicCard;
        }

        public CardPrice CardPrice
        {
            get => _cardPrice;
            set
            {
                this.RaiseAndSetIfChanged(ref _cardPrice, value);
                MarketPrice = _cardPrice?.GetMarketPrice(false);
            }
        }

        public decimal? MarketPrice
        {
            get => _marketPrice;
            set => this.RaiseAndSetIfChanged(ref _marketPrice, value);
        }

        public DetailedMagicCard Card { get; }
    }
}