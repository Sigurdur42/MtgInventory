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
            IAutoAupdateMtgDatabaseService autoAupdateMtgDatabaseService)
        {
            InitializeComponent();

            DataContext = mainViewModel;


            this.Closing += (sender, args) => autoAupdateMtgDatabaseService.Stop();
        }
    }
}
