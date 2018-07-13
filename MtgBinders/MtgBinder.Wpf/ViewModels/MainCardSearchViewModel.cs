using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MtgBinders.Domain.Configuration;
using MtgBinders.Domain.Services;
using MtgBinders.Domain.Services.Images;
using MtgBinders.Domain.ValueObjects;

namespace MtgBinder.Wpf.ViewModels
{
    internal class MainCardSearchViewModel : INotifyPropertyChanged
    {
        private readonly IMtgDatabaseService _mtgDatabase;
        private readonly ICardSearchService _cardSearchService;
        private readonly IJsonConfigurationSerializer _configurationSerializer;
        private readonly string _cardSearchSettingsCache;
        private readonly IMtgImageCache _imageCache;

        private string _searchPattern;

        private MtgFullCardViewModel _selectedCard;

        public MainCardSearchViewModel(
           IMtgDatabaseService mtgDatabase,
           ICardSearchService cardSearchService,
           IJsonConfigurationSerializer configurationSerializer,
           IBinderDomainConfigurationProvider configurationProvider,
            IMtgImageCache imageCache)
        {
            _imageCache = imageCache;
            _mtgDatabase = mtgDatabase;
            _cardSearchService = cardSearchService;

            _configurationSerializer = configurationSerializer;
            _cardSearchSettingsCache = Path.Combine(configurationProvider.AppDataFolder, "MainCardSearchSettings.json");

            // TODO: Serialize
            CardSearchSettings = _configurationSerializer.Deserialize<CardSearchSettings>(_cardSearchSettingsCache, null) ?? new CardSearchSettings();
            CardSearchSettings.ShowUniquePrints = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string SearchPattern
        {
            get => _searchPattern;

            set
            {
                if (_searchPattern != value)
                {
                    _searchPattern = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SearchPattern)));
                }
            }
        }

        public MtgFullCardViewModel SelectedCard
        {
            get => _selectedCard;

            set
            {
                if (_selectedCard != value)
                {
                    _selectedCard = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCard)));

                    // TODO: Reactivate later? Task.Factory.StartNew(() => _selectedCard?.UpdateCardFromScryfall(_mtgDatabase));
                }
            }
        }

        public CardSearchSettings CardSearchSettings { get; private set; }

        public MtgFullCardViewModel[] FoundCards { get; private set; }

        public void StartCardSearch()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_searchPattern))
                {
                    FoundCards = new MtgFullCardViewModel[0];
                    return;
                }

                FoundCards = _cardSearchService
                    .Search(_searchPattern, CardSearchSettings)
                    .Select(c => new MtgFullCardViewModel(c))
                    .ToArray();

                if (FoundCards.Any())
                {
                    var lookup = FoundCards.Select(c => c.FullCard).ToArray();
                    Task.Factory.StartNew(() =>
                    {
                        _imageCache?.DownloadMissingImages(lookup);
                    });
                }
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