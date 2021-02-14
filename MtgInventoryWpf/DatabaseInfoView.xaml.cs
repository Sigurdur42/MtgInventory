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

namespace MtgInventoryWpf
{
    /// <summary>
    /// Interaction logic for DatabaseInfoView.xaml
    /// </summary>
    public partial class DatabaseInfoView : UserControl
    {
        public DatabaseInfoView()
        {
            InitializeComponent();
        }

        private void OnDownloadAllImages(object sender, RoutedEventArgs e)
        {
            (DataContext as DatabaseInfoViewModel)?.DownloadAllImages();
        }
        private void OnDownloadDatabase(object sender, RoutedEventArgs e)
        {
            (DataContext as DatabaseInfoViewModel)?.DownloadDatabase();
        }
    }
}
