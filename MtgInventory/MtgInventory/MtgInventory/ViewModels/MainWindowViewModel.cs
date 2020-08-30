using MkmApi;
using MtgInventory.Service;
using MtgInventory.Service.Models;
using ReactiveUI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MtgInventory.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _mkmProductsSummary;

        private string _mkmProductLookupName;

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

        public string MkmProductLookupName
        {
            get => _mkmProductLookupName;
            set => this.RaiseAndSetIfChanged(ref _mkmProductLookupName, value);
        }

        private IEnumerable<MkmProductInfo> _mkmProductsFound;
        public IEnumerable<MkmProductInfo> MkmProductsFound
        {
            get => _mkmProductsFound;
            set => this.RaiseAndSetIfChanged(ref _mkmProductsFound, value);
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

        public void OnSearchMkmProduct()
        {
            MkmProductsFound = MainService?.MkmFindProductsByName(_mkmProductLookupName);
        }

        public void OnOpenMkmProductPage(MkmProductInfo info)
        {
            MainService?.OpenMkmProductPage(info);
        }
    }
}