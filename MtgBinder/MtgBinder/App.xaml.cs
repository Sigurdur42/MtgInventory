using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MtgBinder.Configuration;
using MtgBinder.Domain.Scryfall;
using ScryfallApi.Client;
using Serilog;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using MtgBinder.Database;
using MtgBinder.Decks;
using MtgBinder.Domain.Database;
using MtgBinder.Domain.Service;
using MtgBinder.Domain.Tools;
using MtgBinder.Inventory;
using MtgBinder.LogProgress;
using MtgBinder.Lookup;

namespace MtgBinder
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            //
            //  DI for core WPF:
            // https://itbackyard.com/how-to-net-core-3-0-wpf-application-use-dependency-injection/
            //
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var configurationFolderProvider = new UserDataFolderProvider();
            var logProgressViewModel = new LogProgressViewModel();
            
            var progressNotifier = new AsyncProgressNotifier();
            services.AddSingleton<IAsyncProgressNotifier>(progressNotifier);
            services.AddSingleton<IAsyncProgress>(progressNotifier);

            services.AddHttpClient<ScryfallApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.scryfall.com/");
            });

            services.AddSingleton<IScryfallService, ScryfallService>();

            services.AddSingleton(configurationFolderProvider);
            services.AddSingleton<IBinderConfigurationRepository, BinderConfigurationRepository>();
            services.AddSingleton<ICardDatabase, CardDatabase>();
            services.AddSingleton<ICardService, CardService>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<InventoryViewModel>();
            services.AddSingleton<CardDatabaseViewModel>();
            services.AddSingleton<LookupViewModel>();
            services.AddSingleton(logProgressViewModel);
            services.AddSingleton<LoadDeckViewModel>();

            //     services.AddSingleton<ILogBase>(new LogBase(new FileInfo($@"C:\temp\log.txt")));
            services.AddSingleton<MainWindow>();

            // Configure Logging
            services.AddLogging((logBuilder) =>
            {
                if (Debugger.IsAttached)
                {
                    logBuilder.AddDebug();
                }
            });

            var now = DateTime.Now;
            var targetFileName = Path.Combine(
                configurationFolderProvider.ConfigurationFolder.FullName,
                "Logs",
                $"MtgBinder_{now.ToString("yyyy_MM_dd__HH_mm_ss_ffff", CultureInfo.InvariantCulture)}.log");

            var targetFileInfo = new FileInfo(targetFileName);
            if (!targetFileInfo.Directory.Exists)
            {
                targetFileInfo.Directory.Create();
            }

            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Sink(logProgressViewModel)
                .WriteTo.File(targetFileName);

            if (Debugger.IsAttached)
            {
                logConfig = logConfig.WriteTo.Debug();
            }

            Log.Logger = logConfig.CreateLogger();

            Log.Information($"Initialising MtgBinder...");
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            
        }
    }
}