using System.Windows;
using System.Windows.Controls;

namespace MtgInventoryWpf
{
    /// <summary>
    /// Interaction logic for CardSearchView.xaml
    /// </summary>
    public partial class CardSearchView : UserControl
    {
        public CardSearchView()
        {
            InitializeComponent();
        }

        private async void OnSearchButtonClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is CardSearchViewModel viewModel)
            {
                await viewModel.PerformSearch();
            }
        }
    }
}
