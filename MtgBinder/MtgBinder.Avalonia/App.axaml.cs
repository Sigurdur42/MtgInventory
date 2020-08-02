using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MtgBinder.Avalonia.Configuration;
using MtgBinder.Avalonia.ViewModels;
using MtgBinder.Avalonia.ViewModels.Lookup;
using MtgBinder.Avalonia.ViewModels.Stock;
using MtgBinder.Avalonia.Views;
using MtgBinder.Domain.Configuration;
using MtgBinder.Domain.Database;
using MtgBinder.Domain.Scryfall;
using MtgBinder.Domain.Service;
using MtgBinder.Domain.Tools;
using ScryfallApi.Client;

namespace MtgBinder.Avalonia
{
    public class App : Application
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

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = _serviceProvider.GetService<MainWindow>();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // var logProgressViewModel = new LogProgressViewModel();

            var progressNotifier = new AsyncProgressNotifier();
            services.AddSingleton<IAsyncProgressNotifier>(progressNotifier);
            services.AddSingleton<IAsyncProgress>(progressNotifier);

            services.AddHttpClient<ScryfallApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.scryfall.com/");
            });

            services.AddSingleton<IScryfallService, ScryfallService>();

            services.AddSingleton<IBinderConfigurationRepository, BinderConfigurationRepository>();
            services.AddSingleton<IUserDataFolderProvider, UserDataFolderProvider>();
            services.AddSingleton<ICardDatabase, CardDatabase>();
            services.AddSingleton<ICardService, CardService>();
            // services.AddSingleton<MainViewModel>();
            ////services.AddSingleton<InventoryViewModel>();
            ////services.AddSingleton<CardDatabaseViewModel>();
            services.AddSingleton<CardLookupViewModel>();
            services.AddSingleton<StockViewModel>();

            ////services.AddSingleton(logProgressViewModel);
            ////services.AddSingleton<LoadDeckViewModel>();

            //     services.AddSingleton<ILogBase>(new LogBase(new FileInfo($@"C:\temp\log.txt")));

            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<CardLookupView>();
            services.AddSingleton<StockView>();
            services.AddSingleton<MainWindow>();

            // Configure Logging
            services.AddLogging((logBuilder) =>
            {
                if (Debugger.IsAttached)
                {
                    logBuilder.AddDebug();
                }
            });

            ////var now = DateTime.Now;
            ////var targetFileName = Path.Combine(
            ////    configurationFolderProvider.ConfigurationFolder.FullName,
            ////    "Logs",
            ////    $"MtgBinder_{now.ToString("yyyy_MM_dd__HH_mm_ss_ffff", CultureInfo.InvariantCulture)}.log");

            ////var targetFileInfo = new FileInfo(targetFileName);
            ////if (!targetFileInfo.Directory.Exists)
            ////{
            ////    targetFileInfo.Directory.Create();
            ////}

            ////var logConfig = new LoggerConfiguration()
            ////    .MinimumLevel.Debug()
            ////    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            ////    .Enrich.FromLogContext()
            ////    .WriteTo.Sink(logProgressViewModel)
            ////    .WriteTo.File(targetFileName);

            ////if (Debugger.IsAttached)
            ////{
            ////    logConfig = logConfig.WriteTo.Debug();
            ////}

            ////Log.Logger = logConfig.CreateLogger();

            ////Log.Information($"Initialising MtgBinder...");
        }
    }
}