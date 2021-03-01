using System;
using System.IO;
using CommandLine;
using LocalSettings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MtgDatabase;
using MtgDatabaseConsole;
using MtgJson;

namespace ScryfallApiConsole
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var exitCode = -1;
            ILogger? logger = null;
            try
            {
                var serviceProvider = InitializeDI();
                logger = serviceProvider?.GetService<ILoggerFactory>()?.CreateLogger<Program>();
                new Parser(with =>
                    {
                        with.CaseInsensitiveEnumValues = true;
                        with.CaseSensitive = false;
                    })
                    .ParseArguments<ApiOptions>(args)
                    .ThrowOnParseError()
                    .WithParsed(options =>
                    {
                        var action = serviceProvider?.GetService<ApiAction>();
                        exitCode = action?.RunAction(options) ?? -1;
                    });
            }
            catch (Exception error)
            {
                logger?.LogCritical($"Caught unhandled exception: {error}");
                exitCode = -1;
            }

            if (exitCode != 0)
            {
                // console.PrintError($"Wanted task has failed with code {exitCode} - please see logs above");
            }

            return exitCode;
        }

        private static ServiceProvider? InitializeDI()
        {
            ServiceProvider? serviceProvider = null;
            ILogger? logger = null;
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("AppSettings.json");

                var configuration = builder.Build();

                var serviceCollection = new ServiceCollection()
                        .AddLogging(cfg =>
                        {
                            cfg.AddConfiguration(configuration.GetSection("Logging"));
                            cfg.AddConsole();
                            cfg.AddDebug();
                            //// cfg.AddProvider(new PanelLogSinkProvider());
                        })
                    // .Configure<LoggerFilterOptions>(cfg => cfg.MinLevel = LogLevel.Trace)
                    ;

                // Add other DI

                serviceCollection
                    .AddMtgDatabase()
                    .AddSingleton<ILocalSettingService, LocalSettingService>()
                    .AddSingleton<ApiAction>();

                serviceProvider = serviceCollection.BuildServiceProvider();

                var localSettings = serviceProvider.GetService<ILocalSettingService>();
                var localPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MtgInventory");
                var configFile = new FileInfo(Path.Combine(localPath, "MtgInventorySettings.yaml"));
                localSettings?.Initialize(configFile, SettingWriteMode.OnChange);


                // TODO: Add other DI configs
                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

                logger = serviceProvider.GetService<ILogger<Program>>();

                logger.LogInformation("This is log message.");

                // configure service
                var baseFolder = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MtgDatabase"));
                var service = serviceProvider.GetService<IMtgDatabaseService>();
                service?.Configure(baseFolder);

                var mtgJsonDatabaseService = serviceProvider.GetService<ILiteDbService>();
                mtgJsonDatabaseService?.Configure(baseFolder);
            }
            catch (Exception error)
            {
                // TODO: Actual log
                logger.LogError($"Unhandled exception caught: {error}");
            }

            return serviceProvider;
        }
    }
}