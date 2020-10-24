using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using MtgInventory.Logging;
using MtgInventory.Models;
using MtgInventory.Service;
using MtgInventory.Service.Decks;
using MtgInventory.Service.Models;
using MtgInventory.Service.Settings;
using MtgInventory.Views;
using ReactiveUI;

namespace MtgInventory.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private const string _allSetsName = "All Sets";
        private IEnumerable<DetailedSetInfo> _allSets;
        private DeckListItemViewModel[] _currentDeckList;
        private IEnumerable<MkmStockItemViewModel> _currentStock;
        private IEnumerable<DetailedSetInfo> _filteredSets = new DetailedSetInfo[0];
        private MkmApiCallStatistics _mkmApiCallStatistics;
        private IEnumerable<DetailedCardViewModel> _mkmProductsFound;
        private string _mkmProductsSummary = "";
        private QueryCardOptions _queryCardOptions = new QueryCardOptions();
        private IEnumerable<string> _setFilter;
        private MtgInventorySettings _settings = new MtgInventorySettings();

        public MainWindowViewModel()
        {
            MainTitle = $"MtgInventory V" + Assembly.GetEntryAssembly()?.GetName()?.Version;

            Task.Factory.StartNew(() =>
            {
                MkmApiCallStatistics = new MkmApiCallStatistics();
                MainService.Initialize(MkmApiCallStatistics, MkmApiCallStatistics);

                Settings = MainService.Settings;

                UpdateProductSummary();

                AssignSets();
                MainService.SetsUpdated += (sender, e) => AssignSets();
            });

            LogSink = PanelLogSink.Instance;
        }

        public IEnumerable<DetailedSetInfo> AllSets
        {
            get => _allSets;
            set
            {
                this.RaiseAndSetIfChanged(ref _allSets, value);
                RebuildSetFilter();
            }
        }

        public DeckListItemViewModel[] CurrentDeckList
        {
            get => _currentDeckList;
            set => this.RaiseAndSetIfChanged(ref _currentDeckList, value);
        }

        public IEnumerable<MkmStockItemViewModel> CurrentStock
        {
            get => _currentStock;
            set => this.RaiseAndSetIfChanged(ref _currentStock, value);
        }

        public QueryCardOptions DetailedCardQueryOptions
        {
            get => _queryCardOptions;
            set => this.RaiseAndSetIfChanged(ref _queryCardOptions, value);
        }

        public IEnumerable<DetailedSetInfo> FilteredSets
        {
            get => _filteredSets;
            set
            {
                this.RaiseAndSetIfChanged(ref _filteredSets, value);
                RebuildSetFilter();
            }
        }

        public PanelLogSink LogSink { get; private set; }

        public MtgInventoryService MainService { get; } = new MtgInventoryService();

        public string MainTitle { get; private set; }

        public MkmApiCallStatistics MkmApiCallStatistics
        {
            get => _mkmApiCallStatistics;
            set => this.RaiseAndSetIfChanged(ref _mkmApiCallStatistics, value);
        }

        public IEnumerable<DetailedCardViewModel> MkmProductsFound
        {
            get => _mkmProductsFound;
            set => this.RaiseAndSetIfChanged(ref _mkmProductsFound, value);
        }

        public string MkmProductsSummary
        {
            get => _mkmProductsSummary;
            set => this.RaiseAndSetIfChanged(ref _mkmProductsSummary, value);
        }

        public QuerySetFilter QuerySetFilter { get; set; } = new QuerySetFilter();

        public IEnumerable<string> SetFilter
        {
            get => _setFilter;
            set => this.RaiseAndSetIfChanged(ref _setFilter, value);
        }

        public MtgInventorySettings Settings
        {
            get => _settings;
            set
            {
                this.RaiseAndSetIfChanged(ref _settings, value);
                RebuildSetFilter();
            }
        }

        public string SystemBaseFolder => MainService?.SystemFolders.BaseFolder.FullName ?? "";

        public void OnDownloadAndRebuildAll()
        {
            Task.Factory.StartNew(() =>
            {
                MainService?.DownloadAllProducts();
                UpdateProductSummary();
                AllSets = MainService.AllSets.ToArray();
                FilteredSets = AllSets;
            });
        }

        public void OnDownloadCardDetails(DetailedSetInfo info)
        {
            Task.Factory.StartNew(() =>
            {
                MainService?.AutoDownloadCardDetailsForSet(info);
                UpdateProductSummary();
            });
        }

        public void OnDownloadMkmSetsAndCards()
        {
            Task.Factory.StartNew(() =>
            {
                MainService?.DownloadMkmSetsAndProducts();
                UpdateProductSummary();
                AllSets = MainService.AllSets.ToArray();
            });
        }

        public void OnDownloadScryfallCards()
        {
            Task.Factory.StartNew(() =>
            {
                MainService.DownloadScryfallCardData();
                UpdateProductSummary();
                AllSets = MainService.AllSets.ToArray();
                FilteredSets = AllSets;
            });
        }

        public void OnDownloadScryfallSets()
        {
            Task.Factory.StartNew(() =>
            {
                MainService.DownloadScryfallSetsData();
                UpdateProductSummary();
                AllSets = MainService.AllSets.ToArray();
                FilteredSets = AllSets;
            });
        }

        public void OnFilterSets()
        {
            FilteredSets = MainService?.FilterSets(QuerySetFilter) ?? new DetailedSetInfo[0];
        }

        public async Task OnLoadDeckFile()
        {
            var openFile = new OpenFileDialog()
            {
                Title = "Select deck file",
                AllowMultiple = false,
            };

            var result = await openFile.ShowAsync(MainWindow.Instance);
            if (result != null && result.Any())
            {
                var file = result.First();

                var content = File.ReadAllText(file);
                var reader = new TextDeckReader();
                var loaded = reader.ReadDeck(content, new FileInfo(file).Name);

                if (loaded.UnreadLines?.Any() ?? false)
                {
                    var display = $"These lines could not be read:{Environment.NewLine}{string.Join(Environment.NewLine, loaded.UnreadLines)}";
                    MessageBoxDialog.DisplayWarning("Reading deck", display);
                }

                MainService.EnrichDeckListWithDetails(loaded.Deck);
                CurrentDeckList = loaded.Deck.Mainboard.Select(c => new DeckListItemViewModel(c)).ToArray();
            }
        }

        public void OnLoadMkmStock()
        {
            Task.Factory.StartNew(() =>
            {
                var stock = MainService
                    .DownloadMkmStock()
                    .Select(i => new MkmStockItemViewModel(i))
                    .ToArray();

                CurrentStock = stock;
                foreach (var item in stock)
                {
                    item.CardPrice = MainService.AutoScryfallService.AutoDownloadPrice(
                        item.EnglishName,
                        item.SetCode,
                        item.ScryfallId);
                }

                MainService.UpdateCallStatistics();
            });
        }

        public void OnOpenMkmProductPage(DetailedCardViewModel info)
        {
            Task.Factory.StartNew(() =>
            {
                MainService?.OpenMkmProductPage(info?.Card?.MkmId ?? "");
                UpdateProductSummary();
            });
        }

        public void OnOpenStockItemInMkmProductPage(MkmStockItemViewModel stockItem)
        {
            Task.Factory.StartNew(() =>
            {
                MainService?.OpenMkmProductPage(stockItem?.IdProduct ?? "");
            });
        }

        public void OnRebuildInternalDatabase()
        {
            Task.Factory.StartNew(() =>
            {
                var task = MainService?.RebuildInternalDatabase();
                task?.ContinueWith((task) =>
                {
                    UpdateProductSummary();
                    AllSets = MainService.AllSets.ToArray();
                    FilteredSets = AllSets;
                });
            });
        }

        public void OnRebuildSetCards(DetailedSetInfo info)
        {
            Task.Factory.StartNew(() =>
            {
                MainService?.RebuildCardsForSet(info);
                UpdateProductSummary();
            });
        }

        public void OnRebuildSetData()
        {
            MainService?.RebuildSetData();
        }

        public void OnSaveSettings()
        {
            Task.Factory.StartNew(() =>
            {
                MainService?.SaveSettings();
            });
        }

        public void OnSearchMkmProduct()
        {
            Task.Factory.StartNew(() =>
            {
                var cards = MainService
                    .FindDetailedCardsByName(_queryCardOptions)
                    .Select(c => new DetailedCardViewModel(c))
                    .ToArray();

                MkmProductsFound = cards;

                foreach (var item in cards)
                {
                    item.CardPrice = MainService.AutoScryfallService.AutoDownloadPrice(
                        item.Card.NameEn,
                        item.Card.SetCode,
                        item.Card.ScryfallId);
                }

                MainService.UpdateCallStatistics();
            });
        }

        public void ShutDown()
        {
            MainService?.ShutDown();
        }

        internal void OnGenerateMissingSetData() => MainService?.GenerateMissingSetData();

        internal void OnGenerateReferenceCardData() => MainService?.GenerateReferenceCardData();

        internal void OnGenerateReferenceSetData()
        {
            MainService?.GenerateReferenceSetData();
        }

        private void AssignSets()
        {
            AllSets = MainService.AllSets.ToArray();
            FilteredSets = AllSets;

            UpdateProductSummary();
        }

        private void RebuildSetFilter()
        {
            var setsToFilter = _allSets?.Select(s => s.SetName)?.OrderBy(s => s)?.ToList() ?? new List<string>();
            setsToFilter.Insert(0, "All Sets");
            SetFilter = setsToFilter;

            _queryCardOptions.SetName = _allSetsName;
        }

        private void UpdateProductSummary()
                                                                                                                                                                    => MkmProductsSummary = "MKM: " + MainService?.MkmProductsSummary + Environment.NewLine + "Scryfall: " + MainService?.ScryfallProductsSummary + Environment.NewLine + "Internal: " + MainService?.InternalProductsSummary;
    }
}