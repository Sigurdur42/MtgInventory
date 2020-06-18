using MtgBinder.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MtgBinder.Wpf.Views
{
    /// <summary>
    /// Interaction logic for DeckListsView.xaml
    /// </summary>
    public partial class DeckListsView : UserControl
    {
        public DeckListsView()
        {
            InitializeComponent();
        }

        internal DeckListsViewModel ViewModel => DataContext as DeckListsViewModel;
    }
}