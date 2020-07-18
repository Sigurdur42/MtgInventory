using MtgBinder.Avalonia.ViewModels.Lookup;

namespace MtgBinder.Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(
            CardLookupViewModel cardLookupViewModel)
        {
            CardLookupViewModel = cardLookupViewModel;
        }

        public CardLookupViewModel CardLookupViewModel { get; }
    }
}