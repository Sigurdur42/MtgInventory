using MtgBinders.Domain.Configuration;
using MtgBinders.Domain.Services;
using System.Threading.Tasks;
using System.ComponentModel;

namespace MtgBinder.Wpf.ViewModels
{
    internal class SystemPageViewModel : INotifyPropertyChanged
    {
        private readonly IBinderDomainConfigurationProvider _configurationProvider;
        private readonly IMtgSetService _setService;
        private readonly IMtgCardService _cardService;
        private readonly IMtgDatabaseService _mtgDatabase;

        public SystemPageViewModel(
            IMtgSetService setService,
            IMtgCardService cardService,
            IBinderDomainConfigurationProvider configurationProvider,
            IMtgDatabaseService mtgDatabase)
        {
            _setService = setService;
            _cardService = cardService;
            _configurationProvider = configurationProvider;
            _mtgDatabase = mtgDatabase;

            _setService.InitializeDone += (sender, e) => FireSetServiceChanges();
            _cardService.InitializeDone += (sender, e) => FireCardServiceChanges();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string AppDataFolder => _configurationProvider.AppDataFolder;

        public int NumberOfSets => _setService.SetRepository.NumberOfSets;

        public int NumberOfCards => _cardService.NumberOfCards;

        public string SetLastUpdateDate => _setService.LastUpdatedCacheAt?.ToString();

        public void UpdateDatabaseFromScryfall()
        {
            Task.Factory.StartNew(() =>
            {
                _mtgDatabase.UpdateDatabase(false);
                FireSetServiceChanges();
            });
        }

        private void FireSetServiceChanges()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NumberOfSets)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SetLastUpdateDate)));
        }

        private void FireCardServiceChanges()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NumberOfCards)));
        }
    }
}