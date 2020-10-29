using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

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
            ServiceProvider? serviceProvider = null;
            try
            {
                serviceProvider = new ServiceCollection()
                    .AddLogging(cfg =>
                    {
                        cfg.AddConsole();
                        cfg.AddDebug();
                    }).Configure<LoggerFilterOptions>(cfg => cfg.MinLevel = LogLevel.Debug)
                    .BuildServiceProvider();

                // TODO: Add other DI configs

                var logger = serviceProvider.GetService<ILogger<Program>>();

                logger.LogInformation("This is log message.");

                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception error)
            {
                // TODO: Actual log
                Console.WriteLine($"Unhandled exception caught: {error}");
            }
            finally
            {
                serviceProvider?.Dispose();
            }
        }

    }
}