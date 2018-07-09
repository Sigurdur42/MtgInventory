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
    /// Interaction logic for SystemInfoView.xaml
    /// </summary>
    public partial class SystemInfoView : UserControl
    {
        public SystemInfoView()
        {
            InitializeComponent();
        }

        internal SystemPageViewModel ViewModel => DataContext as SystemPageViewModel;

        private void OnUpdateDatabaseFromScryfall(object sender, RoutedEventArgs e)
        {
            ViewModel?.UpdateDatabaseFromScryfall(false);
        }

        private void OnDownloadDatabaseFromScryfall(object sender, RoutedEventArgs e)
        {
            ViewModel?.UpdateDatabaseFromScryfall(true);
        }
         private void OnDownloadMissingImages(object sender, RoutedEventArgs e)
        {
            ViewModel?.DownloadMissingImages(true);
        }
   }
}