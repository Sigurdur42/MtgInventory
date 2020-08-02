using MtgBinder.Avalonia.ViewModels.Lookup;
using MtgBinder.Avalonia.ViewModels.Stock;

namespace MtgBinder.Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(
            CardLookupViewModel cardLookupViewModel,
            StockViewModel stockViewModel)
        {
            CardLookupViewModel = cardLookupViewModel;
            StockViewModel = stockViewModel;
        }

        public CardLookupViewModel CardLookupViewModel { get; }
        public StockViewModel StockViewModel { get; }
    }
}