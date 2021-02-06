using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MtgDatabase.Models;

namespace MtgInventoryWpf
{
    /// <summary>
    /// Interaction logic for CardListView.xaml
    /// </summary>
    public partial class CardListView : UserControl
    {
        public CardListView()
        {
            InitializeComponent();
        }

        private async void OnPasteFromClipboard(object sender, RoutedEventArgs e)
        {
            if (DataContext is CardListViewModel viewModel)
            {
                await viewModel.CopyFromClipboard();
            }
        }



        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            var model = (sender as ComboBox)?.DataContext as CardListViewLineViewModel;
            model?.UpdateReprints();

        }

        private void OnChangeSetOfCard(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }

            var model = (sender as ComboBox)?.DataContext as CardListViewLineViewModel;

            if (e.AddedItems[0] is QueryableMagicCard newSelection)
            {
                model?.ChangeSetOfCard(newSelection);
            }

        }
    }
}
