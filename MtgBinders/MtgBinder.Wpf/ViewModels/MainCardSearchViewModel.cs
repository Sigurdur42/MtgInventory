using MtgBinders.Domain.Configuration;
using MtgBinders.Domain.Services;
using MtgBinders.Domain.ValueObjects;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace MtgBinder.Wpf.ViewModels
{
    internal class MainCardSearchViewModel : INotifyPropertyChanged
    {
        private readonly IMtgDatabaseService _mtgDatabase;
        private readonly ICardSearchService _cardSearchService;
        private readonly IJsonConfigurationSerializer _configurationSerializer;
        private readonly string _cardSearchSettingsCache;

        private string _searchPattern;

        private MtgFullCard _selectedCard;

        public MainCardSearchViewModel(
                       IMtgDatabaseService mtgDatabase,
               ICardSearchService cardSearchService,
               IJsonConfigurationSerializer configurationSerializer,
               IBinderDomainConfigurationProvider configurationProvider)
        {
            _mtgDatabase = mtgDatabase;
            _cardSearchService = cardSearchService;

            _configurationSerializer = configurationSerializer;
            _cardSearchSettingsCache = Path.Combine(configurationProvider.AppDataFolder, "MainCardSearchSettings.json");

            // TODO: Serialize
            CardSearchSettings = _configurationSerializer.Deserialize<CardSearchSettings>(_cardSearchSettingsCache) ?? new CardSearchSettings();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string SearchPattern
        {
            get
            {
                return _searchPattern;
            }

            set
            {
                if (_searchPattern != value)
                {
                    _searchPattern = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchPattern)));
                }
            }
        }

        public MtgFullCard SelectedCard
        {
            get
            {
                return _selectedCard;
            }

            set
            {
                if (_selectedCard != value)
                {
                    _selectedCard = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCard)));
                }
            }
        }

        public CardSearchSettings CardSearchSettings { get; private set; }

        public MtgFullCard[] FoundCards { get; private set; }

        public void StartCardSearch()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_searchPattern))
                {
                    FoundCards = new MtgFullCard[0];
                    return;
                }

                FoundCards = _cardSearchService.Search(_searchPattern, CardSearchSettings);
            }
            finally
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FoundCards)));

                // TODO: Write only when changed
                WriteCardSearchSettings();
            }
        }

        private void WriteCardSearchSettings()
        {
            _configurationSerializer.Serialize(_cardSearchSettingsCache, CardSearchSettings);
        }
    }
}