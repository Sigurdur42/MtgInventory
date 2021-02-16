using System.Windows;
using MtgDatabase;

namespace MtgInventoryWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(
            MainViewModel mainViewModel,
            IAutoUpdateMtgDatabaseService autoUpdateMtgDatabaseService)
        {
            InitializeComponent();

            DataContext = mainViewModel;


            this.Closing += (sender, args) => autoUpdateMtgDatabaseService.Stop(true);
        }
    }
}
