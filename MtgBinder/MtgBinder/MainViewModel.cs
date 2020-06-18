using MtgBinder.Configuration;
using MtgBinder.Database;
using MtgBinder.Decks;
using MtgBinder.Domain.Database;
using MtgBinder.Inventory;
using MtgBinder.LogProgress;
using MtgBinder.Lookup;
using PropertyChanged;

namespace MtgBinder
{
    [AddINotifyPropertyChangedInterface]
    public class MainViewModel
    {
        private readonly IBinderConfigurationRepository _binderConfigurationRepository;

        public MainViewModel(
            IBinderConfigurationRepository binderConfigurationRepository,
            UserDataFolderProvider userDataFolderProvider,
            CardDatabaseViewModel cardDatabaseViewModel,
            LookupViewModel lookupViewModel,
            LoadDeckViewModel loadDeckViewModel,
            LogProgressViewModel logProgressViewModel,
            InventoryViewModel inventoryViewModel)
        {
            UserDataFolderProvider = userDataFolderProvider;
            CardDatabaseViewModel = cardDatabaseViewModel;
            LookupViewModel = lookupViewModel;
            LoadDeckViewModel = loadDeckViewModel;
            LogProgressViewModel = logProgressViewModel;
            InventoryViewModel = inventoryViewModel;
            _binderConfigurationRepository = binderConfigurationRepository;
            Configuration = _binderConfigurationRepository.ReadConfiguration();
        }

        public UserDataFolderProvider UserDataFolderProvider { get; }
        public CardDatabaseViewModel CardDatabaseViewModel { get; }
        public LookupViewModel LookupViewModel { get; }
        public LoadDeckViewModel LoadDeckViewModel { get; }
        public LogProgressViewModel LogProgressViewModel { get; }
        public InventoryViewModel InventoryViewModel { get; }
        public BinderConfiguration Configuration { get; }
    }
}