using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MtgBinder.Avalonia.ViewModels.Lookup;

namespace MtgBinder.Avalonia.Views
{
    public class CardLookupView : UserControl
    {
        public CardLookupView()
        {
            this.InitializeComponent();
        }

  

        public void SetViewModel(CardLookupViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = ViewModel;
        }

        public CardLookupViewModel ViewModel { get; private set; }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}