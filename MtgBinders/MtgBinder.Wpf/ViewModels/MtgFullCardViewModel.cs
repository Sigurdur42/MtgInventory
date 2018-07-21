using MtgBinders.Domain.Services;
using MtgBinders.Domain.ValueObjects;
using System.ComponentModel;
using System.Threading.Tasks;

namespace MtgBinder.Wpf.ViewModels
{
    public class MtgFullCardViewModel : INotifyPropertyChanged
    {
        public MtgFullCardViewModel(MtgFullCard fullCard)
        {
            FullCard = fullCard;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MtgFullCard FullCard { get; }

        public decimal? PriceUsd => FullCard.PriceUsd;
        public decimal? PriceTix => FullCard.PriceTix;
        public decimal? PriceEur => FullCard.PriceEur;

        public void UpdateCardFromScryfall(IMtgDatabaseService databaseService)
        {
            databaseService.UpdateCardDetails(FullCard);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FullCard)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PriceUsd)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PriceTix)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PriceEur)));
        }

        public void UpdateCardFromScryfallAsync(IMtgDatabaseService databaseService)
        {
            Task.Factory.StartNew(() => UpdateCardFromScryfall(databaseService));
        }
    }
}