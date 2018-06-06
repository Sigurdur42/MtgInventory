using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MtgBinders.Domain.DependencyInjection;
using MtgBinders.Domain.Scryfall;
using MtgBinders.ViewModels;
using MtgBinders.Views;
using MtgScryfall;

namespace MtgBinders
{
    internal class Program
    {
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .LogToDebug();

        private static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();

            // Bootstrap logger
            ILoggerFactory loggerFactory = new LoggerFactory()
                .AddConsole(LogLevel.Debug)
                .AddDebug(LogLevel.Debug);

            var initLogger = loggerFactory.CreateLogger("Bootstrap");

            initLogger.LogDebug("Configuring DI for MtgBinder...");
            serviceCollection.AddSingleton((serviceProvider) => loggerFactory);
            serviceCollection.AddSingleton<MainWindowViewModel>();

            // Configure DI
            BindMicrosoftDi.BindProductiveEnvironment(initLogger, serviceCollection);

            var provider = serviceCollection.BuildServiceProvider();
            // var test = provider.GetService<IScryfallService>();

            initLogger.LogDebug("Launching UI...");
            BuildAvaloniaApp().Start<MainWindow>(() => provider.GetService<MainWindowViewModel>());
        }
    }
}