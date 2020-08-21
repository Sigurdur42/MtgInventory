using MtgInventory.Service;
using System.Threading.Tasks;

namespace MtgInventory.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string SystemBaseFolder => MainService?.SystemFolders.BaseFolder.FullName;

        public MtgInventoryService MainService { get; } = new MtgInventoryService();

        public MainWindowViewModel()
        {
            Task.Factory.StartNew(() => MainService.Initialize());
        }

        public void ShutDown()
        {
            MainService?.ShutDown();
        }
    }
}