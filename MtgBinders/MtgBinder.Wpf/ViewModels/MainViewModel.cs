﻿using MtgBinders.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtgBinder.Wpf.ViewModels
{
    internal class MainViewModel
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

            // Launch the initialization in a separate task:
            Task.Factory.StartNew(() =>
            {
                _mtgDatabaseService.Initialize();
            });
        }

        public SystemPageViewModel SystemPageViewModel { get; }

        public MainCardSearchViewModel MainCardSearchViewModel { get; }
    }
}