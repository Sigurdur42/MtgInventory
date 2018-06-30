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

        public MainViewModel(
            SystemPageViewModel systemPageViewModel,
            MainCardSearchViewModel mainCardSearchViewModel,
            IMtgDatabaseService mtgDatabaseService)
        {
            _mtgDatabaseService = mtgDatabaseService;
            SystemPageViewModel = systemPageViewModel;
            MainCardSearchViewModel = mainCardSearchViewModel;

            UiLogger.UiCallbacks.Add(SetLatestLog);

            // Launch the initialization in a separate task:
            Task.Factory.StartNew(() =>
            {
                _mtgDatabaseService.Initialize();
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string LatestLogMessage { get; private set; }

        public SystemPageViewModel SystemPageViewModel { get; }

        public MainCardSearchViewModel MainCardSearchViewModel { get; }

        private void SetLatestLog(DateTime timestamp, LogLevel logLevel, string category, string message, Exception error)
        {
            LatestLogMessage = $"[{logLevel.ToString()[0]}] {category}: {message}";
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LatestLogMessage)));
        }
    }
}