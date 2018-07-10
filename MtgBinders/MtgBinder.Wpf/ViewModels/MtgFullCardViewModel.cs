using MtgBinders.Domain.Services;
using MtgBinders.Domain.ValueObjects;
using System.ComponentModel;

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

        public void UpdateCardFromScryfall(IMtgDatabaseService databaseService)
        {
            databaseService.UpdateCardDetails(FullCard);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FullCard)));
        }
    }
}