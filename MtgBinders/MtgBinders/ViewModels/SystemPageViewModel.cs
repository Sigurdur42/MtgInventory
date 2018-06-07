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

        public SystemPageViewModel(
            IMtgSetService setService,
            IBinderDomainConfigurationProvider configurationProvider)
        {
            _setService = setService;
            _configurationProvider = configurationProvider;

            _setService.InitializeDone += (sender, e) => FireSetServiceChanges();
        }

        public string AppDataFolder => _configurationProvider.AppDataFolder;

        public int NumberOfSets => _setService.SetRepository.NumberOfSets;

        public string SetLastUpdateDate => _setService.LastUpdatedCacheAt?.ToString();

        public void UpdateSetsFromScryfall()
        {
            Task.Factory.StartNew(() =>
            {
                _setService.UpdateSetsFromScryfall();
                FireSetServiceChanges();
            });
        }

        private void FireSetServiceChanges()
        {
            this.RaisePropertyChanged(nameof(NumberOfSets));
            this.RaisePropertyChanged(nameof(SetLastUpdateDate));
        }
    }
}