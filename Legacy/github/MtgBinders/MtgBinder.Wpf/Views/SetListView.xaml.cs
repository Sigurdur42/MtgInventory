using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using MtgBinder.Wpf.ViewModels;

namespace MtgBinder.Wpf.Views
{
    /// <summary>
    /// Interaction logic for SetListView.xaml
    /// </summary>
    public partial class SetListView : UserControl, INotifyPropertyChanged
    {
        public SetListView()
        {
            InitializeComponent();

            rootGrid.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SetListViewModel SetListViewModel => DataContext as SetListViewModel;

        private void OnClickShowSetCards(object sender, RoutedEventArgs e)
        {
            var setCode = ((Button)sender).CommandParameter?.ToString();
            if (string.IsNullOrWhiteSpace(setCode))
            {
                return;
            }

            SetListViewModel?.DisplayAllCardsFromSet(setCode);
        }
    }
}