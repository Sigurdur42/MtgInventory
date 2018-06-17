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
    }
}