using MtgBinder.Wpf.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MtgBinder.Wpf.Views
{
    /// <summary>
    /// Interaction logic for MainCardSearchView.xaml
    /// </summary>
    public partial class MainCardSearchView : UserControl
    {
        public MainCardSearchView()
        {
            InitializeComponent();
        }

        internal MainCardSearchViewModel ViewModel => DataContext as MainCardSearchViewModel;

        private void OnSearchButtonClick(object sender, RoutedEventArgs e)
        {
            // TODO: Wrap this with try catch
            ViewModel?.StartCardSearch();
        }

        private void OnLoadMissingImages(object sender, RoutedEventArgs e)
        {
            ViewModel?.LoadMissingCardImages();
        }

        private void OnUpdateCards(object sender, RoutedEventArgs e)
        {
            ViewModel?.UpdateCardInfo();
        }
    }
}