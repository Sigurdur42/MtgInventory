using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MkmApi;
using MtgInventory.Logging;
using MtgInventory.Models;
using MtgInventory.Service;
using MtgInventory.ViewModels;
using MtgInventory.Views;
using ScryfallApiServices;

namespace MtgInventory
{
    internal class Program
    {
        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug()
                .UseReactiveUI();

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            ILogger? logger = null;
            try
            {
                var serviceCollection = new ServiceCollection()
                    .AddLogging(cfg =>
                    {
                        cfg.AddConsole();
                        cfg.AddDebug();
                        cfg.AddProvider(new PanelLogSinkProvider());
                    }).Configure<LoggerFilterOptions>(cfg => cfg.MinLevel = LogLevel.Debug);

                // Add other DI

                var callStats = new MkmApiCallStatistics();
                serviceCollection
                    .AddMtgInventoryService()
                    .AddSingleton<MainWindowViewModel>()
                    .AddSingleton<MainWindow>()
                    .AddSingleton<IScryfallApiCallStatistic>(callStats)
                    .AddSingleton<IApiCallStatistic>(callStats)
                    .AddSingleton<MkmApiCallStatistics>(callStats)
                    .AddSingleton<MkmRequest>()
                    ;

                ServiceProvider = serviceCollection.BuildServiceProvider();

                // TODO: Add other DI configs
                var loggerFactory = ServiceProvider.GetService<ILoggerFactory>();

                 logger = ServiceProvider.GetService<ILogger<Program>>();

                logger.LogInformation("This is log message.");

                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception error)
            {
                // TODO: Actual log
                logger.LogError($"Unhandled exception caught: {error}");
            }
            finally
            {
                ServiceProvider?.Dispose();
            }
        }

        internal static ServiceProvider? ServiceProvider { get; set; }
    }
}