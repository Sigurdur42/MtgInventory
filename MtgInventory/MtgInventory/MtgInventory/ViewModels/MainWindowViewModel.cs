using MtgInventory.Service;

namespace MtgInventory.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string SystemBaseFolder => MainService?.SystemFolders.BaseFolder.FullName;

        public MtgInventoryService MainService { get; } = new MtgInventoryService();
    }
}