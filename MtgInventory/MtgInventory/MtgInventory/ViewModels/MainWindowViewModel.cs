using MtgInventory.Service;
using ReactiveUI;
using System.Threading.Tasks;

namespace MtgInventory.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _mkmProductsSummary;

        public MainWindowViewModel()
        {
            Task.Factory.StartNew(() =>
            {
                MainService.Initialize();
                MkmProductsSummary = MainService?.MkmProductsSummary;
            });
        }

        public string SystemBaseFolder => MainService?.SystemFolders.BaseFolder.FullName;
        public MtgInventoryService MainService { get; } = new MtgInventoryService();

        public string MkmProductsSummary
        {
            get => _mkmProductsSummary;
            set => this.RaiseAndSetIfChanged(ref _mkmProductsSummary, value);
        }

        public void ShutDown()
        {
            MainService?.ShutDown();
        }

        public void OnDownloadMkmProducts()
        {
            MainService?.DownloadMkmProducts();
            MkmProductsSummary = MainService?.MkmProductsSummary;
        }
    }
}