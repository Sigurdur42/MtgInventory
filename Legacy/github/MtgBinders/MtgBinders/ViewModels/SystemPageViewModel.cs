using MtgBinders.Domain.Configuration;
using MtgBinders.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Threading.Tasks;

namespace MtgBinders.ViewModels
{
    internal class SystemPageViewModel : ViewModelBase
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

        public string AppDataFolder => _configurationProvider.AppDataFolder;

        public int NumberOfSets => _setService.SetRepository.NumberOfSets;

        public int NumberOfCards => _cardService.NumberOfCards;

        public string SetLastUpdateDate => _setService.LastUpdatedCacheAt?.ToString();

        public void UpdateSetsFromScryfall()
        {
            Task.Factory.StartNew(() =>
            {
                _mtgDatabase.UpdateDatabase(false);
                FireSetServiceChanges();
            });
        }

        private void FireSetServiceChanges()
        {
            this.RaisePropertyChanged(nameof(NumberOfSets));
            this.RaisePropertyChanged(nameof(SetLastUpdateDate));
        }

        private void FireCardServiceChanges()
        {
            this.RaisePropertyChanged(nameof(NumberOfCards));
        }
    }
}