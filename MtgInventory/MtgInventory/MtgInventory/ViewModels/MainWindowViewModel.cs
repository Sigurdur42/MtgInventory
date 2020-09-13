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
using MtgInventory.Views;
using ReactiveUI;

namespace MtgInventory.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _mkmProductsSummary;

        private IEnumerable<DetailedMagicCard> _mkmProductsFound;

        private DeckList _currentDeckList;

        private MkmApiCallStatistics _mkmApiCallStatistics;

        private IEnumerable<MkmStockItemExtended> _currentStock;

        private IEnumerable<DetailedSetInfo> _allSets;

        private QueryCardOptions _queryCardOptions = new QueryCardOptions();

        public MainWindowViewModel()
        {
            MainTitle = $"MtgInventory V" + Assembly.GetEntryAssembly().GetName().Version;

            Task.Factory.StartNew(() =>
            {
                MkmApiCallStatistics = new MkmApiCallStatistics();
                MainService.Initialize(MkmApiCallStatistics);

                UpdateProductSummary();

                AllSets = MainService.AllSets.ToArray();
            });

            LogSink = PanelLogSink.Instance;
        }

        public string SystemBaseFolder => MainService?.SystemFolders.BaseFolder.FullName;

        public string MainTitle { get; private set; }

        public MtgInventoryService MainService { get; } = new MtgInventoryService();

        public PanelLogSink LogSink { get; private set; }

        public DeckList CurrentDeckList
        {
            get => _currentDeckList;
            set => this.RaiseAndSetIfChanged(ref _currentDeckList, value);
        }

        public IEnumerable<DetailedSetInfo> AllSets
        {
            get => _allSets;
            set => this.RaiseAndSetIfChanged(ref _allSets, value);
        }

        public string MkmProductsSummary
        {
            get => _mkmProductsSummary;
            set => this.RaiseAndSetIfChanged(ref _mkmProductsSummary, value);
        }

        public QueryCardOptions DetailedCardQueryOptions
        {
            get => _queryCardOptions;
            set => this.RaiseAndSetIfChanged(ref _queryCardOptions, value);
        }

        public MkmApiCallStatistics MkmApiCallStatistics
        {
            get => _mkmApiCallStatistics;
            set => this.RaiseAndSetIfChanged(ref _mkmApiCallStatistics, value);
        }

        public IEnumerable<MkmStockItemExtended> CurrentStock
        {
            get => _currentStock;
            set => this.RaiseAndSetIfChanged(ref _currentStock, value);
        }

        public IEnumerable<DetailedMagicCard> MkmProductsFound
        {
            get => _mkmProductsFound;
            set => this.RaiseAndSetIfChanged(ref _mkmProductsFound, value);
        }

        public void ShutDown()
        {
            MainService?.ShutDown();
        }

        public void OnDownloadMkmProducts()
        {
            Task.Factory.StartNew(() =>
            {
                MainService?.DownloadMkmProducts();
                UpdateProductSummary();
                AllSets = MainService.AllSets.ToArray();
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
                });
            });
        }

        public void OnSearchMkmProduct()
        {
            Task.Factory.StartNew(() =>
            {
                MkmProductsFound = MainService?.FindDetailedCardsByName(_queryCardOptions);
            });
        }

        public void OnLoadMkmStock()
        {
            Task.Factory.StartNew(() =>
            {
                CurrentStock = MainService?.DownloadMkmStock();
            });
        }

        public void OnOpenMkmProductPage(DetailedMagicCard info)
        {
            Task.Factory.StartNew(() =>
            {
                MainService?.OpenMkmProductPage(info?.MkmId);
                UpdateProductSummary();
            });
        }

        public void OnOpenStockItemInMkmProductPage(MkmStockItemExtended stockItem)
        {
            Task.Factory.StartNew(() =>
            {
                MainService?.OpenMkmProductPage(stockItem?.StockItem.IdProduct);
            });
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
                CurrentDeckList = loaded.Deck;
            }
        }

        private void UpdateProductSummary()
                                                                                                                                                                    => MkmProductsSummary = "MKM: " + MainService?.MkmProductsSummary + Environment.NewLine + "Scryfall: " + MainService?.ScryfallProductsSummary + Environment.NewLine + "Internal: " + MainService?.InternalProductsSummary;
    }
}