using Avalonia;
using Avalonia.Logging.Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MtgBinders.Domain.Configuration;
using MtgBinders.Domain.DependencyInjection;
using MtgBinders.ViewModels;
using MtgBinders.Views;
using System;
using System.IO;

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
            serviceCollection
                .AddSingleton((serviceProvider) => loggerFactory)
                .AddSingleton<MainWindowViewModel>()
                .AddSingleton<SystemPageViewModel>();

            // Configure DI
            BindMicrosoftDi.BindProductiveEnvironment(initLogger, serviceCollection);

            var provider = serviceCollection.BuildServiceProvider();
            var domainConfiguration = provider.GetService<IBinderDomainConfigurationProvider>();
            domainConfiguration.Initialize(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MtgBinder"));

            var mainViewModel = provider.GetService<MainWindowViewModel>();

            initLogger.LogDebug("Launching UI...");
            BuildAvaloniaApp().Start<MainWindow>(() => mainViewModel);
        }
    }
}