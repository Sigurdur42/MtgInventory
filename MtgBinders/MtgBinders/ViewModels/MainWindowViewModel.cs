using MtgBinders.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MtgBinders.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {
        private readonly IMtgSetService _mtgSetService;

        public MainWindowViewModel(
            SystemPageViewModel systemPageViewModel,
            IMtgSetService mtgSetService)
        {
            _mtgSetService = mtgSetService;
            SystemPageViewModel = systemPageViewModel;

            // Launch the initialization in a separate task:
            Task.Factory.StartNew(() =>
            {
                _mtgSetService.Initialize();
                
            });
        }

        public SystemPageViewModel SystemPageViewModel { get; }
    }
}