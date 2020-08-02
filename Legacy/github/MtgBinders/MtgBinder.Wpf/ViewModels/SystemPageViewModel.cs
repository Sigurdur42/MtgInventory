using MtgBinders.Domain.Configuration;
using MtgBinders.Domain.Services;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using MtgBinders.Domain.Services.Images;

namespace MtgBinder.Wpf.ViewModels
{
    internal class SystemPageViewModel : INotifyPropertyChanged
    {
        private readonly IBinderDomainConfigurationProvider _configurationProvider;
        private readonly IMtgDatabaseService _mtgDatabase;
        private readonly IMtgImageCache _imageCache;

        public SystemPageViewModel(
            IBinderDomainConfigurationProvider configurationProvider,
            IMtgDatabaseService mtgDatabase,
            IMtgImageCache imageCache)
        {
            _configurationProvider = configurationProvider;
            _mtgDatabase = mtgDatabase;
            _imageCache = imageCache;

            _mtgDatabase.DatabaseUpdated += (sender, e) =>
            {
                FireCardDatabaseeChanges();
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string AppDataFolder => _configurationProvider.AppDataFolder;

        public int NumberOfSets => _mtgDatabase.NumberOfSets;

        public int NumberOfCards => _mtgDatabase.NumberOfCards;

        public string SetLastUpdateDate => _mtgDatabase.LastUpdated?.ToString();

        public bool AreCardsMissing => _mtgDatabase.IsCardsMissing;

        public void UpdateDatabaseFromScryfall(bool force)
        {
            Task.Factory.StartNew(() =>
            {
                _mtgDatabase.UpdateDatabase(force);
                FireCardDatabaseeChanges();
            });
        }

        private void FireCardDatabaseeChanges()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NumberOfSets)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SetLastUpdateDate)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NumberOfCards)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AreCardsMissing)));
        }

        public void DownloadMissingImages(bool b)
        {
             Task.Factory.StartNew(() =>
             {
                 _imageCache.DownloadMissingImages(_mtgDatabase.CardData);
             });
           
        }
    }
}