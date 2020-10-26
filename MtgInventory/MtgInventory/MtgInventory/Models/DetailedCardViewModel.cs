using System.Threading.Tasks;
using MtgInventory.Service;
using MtgInventory.Service.Models;
using ReactiveUI;

namespace MtgInventory.Models
{
    public class DetailedCardViewModel : ReactiveObject
    {
        private CardPrice _cardPrice = new CardPrice();
        private decimal? _marketPrice;

        private string _image = "";

        public DetailedCardViewModel(DetailedMagicCard detailedMagicCard)
        {
            Card = detailedMagicCard;
            Task.Factory.StartNew(() =>
            {
                var localFile = AutoDownloadImageCache.Instance?.GetOrDownload(Card, "normal");
                if (localFile == null || !localFile.Exists)
                {
                    Image = "";
                }
                else
                {
                    Image = localFile.FullName;
                }
            });
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

        public string Image
        {
            get => _image;
            set => this.RaiseAndSetIfChanged(ref _image, value);
        }

        public DetailedMagicCard Card { get; }

        public string CollectorNumber => Card?.CollectorNumber?.Replace("★", "*") ?? "";
    }
}