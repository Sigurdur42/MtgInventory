using Microsoft.Extensions.Logging;
using MtgBinder.Wpf.Logging;
using MtgBinders.Domain.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MtgBinder.Wpf.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private readonly IMtgDatabaseService _mtgDatabaseService;
        private readonly IMtgInventoryService _inventoryService;

        public MainViewModel(
            SystemPageViewModel systemPageViewModel,
            MainCardSearchViewModel mainCardSearchViewModel,
            IMtgDatabaseService mtgDatabaseService,
            SetListViewModel setListViewModel,
            IMtgInventoryService inventoryService,
            CardInventoryCollectionViewModel cardInventoryCollectionViewModel)
        {
            _mtgDatabaseService = mtgDatabaseService;
            _inventoryService = inventoryService;
            SystemPageViewModel = systemPageViewModel;
            MainCardSearchViewModel = mainCardSearchViewModel;
            SetListViewModel = setListViewModel;
            CardInventoryCollectionViewModel = cardInventoryCollectionViewModel;
            UiLogger.UiCallbacks.Add(SetLatestLog);

            _mtgDatabaseService.Initialized += (sender, e) =>
            {
                IsInitialized = true;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsInitialized)));
            };

            // Launch the initialization in a separate task:
            Task.Factory.StartNew(() =>
            {
                _mtgDatabaseService.Initialize();
                _inventoryService.Initialize();
            });

            SetListViewModel.RequestShowSetCards += (sender, e) =>
            {
                var cards = _mtgDatabaseService.CardData.Where(c => c.SetCode == e.SetCode);
                MainCardSearchViewModel.SetCardsToDisplay(cards);

                ActivateTabRequested?.Invoke(this, new ActivateTabEventArgs(0));
            };
        }

        public event EventHandler<ActivateTabEventArgs> ActivateTabRequested;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsInitialized { get; private set; }
        public SetListViewModel SetListViewModel { get; }
        public string LatestLogMessage { get; private set; }

        public SystemPageViewModel SystemPageViewModel { get; }

        public MainCardSearchViewModel MainCardSearchViewModel { get; }
        public CardInventoryCollectionViewModel CardInventoryCollectionViewModel { get; }

        public void OnShutdown()
        {
            MainCardSearchViewModel.WriteCardSearchSettings();
        }

        private void SetLatestLog(DateTime timestamp, LogLevel logLevel, string category, string message, Exception error)
        {
            LatestLogMessage = $"[{logLevel.ToString()[0]}] {category}: {message}";
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LatestLogMessage)));
        }
    }
}