using MtgBinders.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MtgBinders.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {
        private readonly IMtgDatabaseService _mtgDatabaseService;

        public MainWindowViewModel(
            SystemPageViewModel systemPageViewModel,
            IMtgDatabaseService mtgDatabaseService)
        {
            _mtgDatabaseService = mtgDatabaseService;
            SystemPageViewModel = systemPageViewModel;

            // Launch the initialization in a separate task:
            Task.Factory.StartNew(() =>
            {
                _mtgDatabaseService.Initialize();
            });
        }

        public SystemPageViewModel SystemPageViewModel { get; }
    }
}