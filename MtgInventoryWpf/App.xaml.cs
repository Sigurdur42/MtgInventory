using System;
using System.IO;
using System.Windows;
using LocalSettings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MtgDatabase;
using MtgDatabase.Cache;

namespace MtgInventoryWpf
{
    public partial class App : Application
    {
        private readonly IHost host;

        public App()
        {
            // The DI configuration here has been build based on
            // https://marcominerva.wordpress.com/2019/11/07/update-on-using-hostbuilder-dependency-injection-and-service-provider-with-net-core-3-0-wpf-applications/
            host = Host.CreateDefaultBuilder()
                   .ConfigureServices((context, services) =>
                   {
                       ConfigureServices(context.Configuration, services);
                   })

                   .Build();

            var localSettings = host.Services.GetService<ILocalSettingService>();

            var localPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MtgInventory");
            var configFile = new FileInfo(Path.Combine(localPath, "MtgInventorySettings.yaml"));
            localSettings?.Initialize(configFile, SettingWriteMode.OnChange);

            var imageCache = host.Services.GetService<IImageCache>();
            imageCache?.Initialize(new DirectoryInfo(localPath));

            var mtgService = host.Services.GetService<IMtgDatabaseService>();
            mtgService?.Configure(
                new DirectoryInfo(localPath));
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (host)
            {
                await host.StopAsync(TimeSpan.FromSeconds(5));
            }

            base.OnExit(e);
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await host.StartAsync();

            var mainWindow = host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        private void ConfigureServices(
                            IConfiguration configuration,
            IServiceCollection services)
        {
            services.AddLogging(cfg =>
            {
                cfg.AddConfiguration(configuration.GetSection("Logging"));
                cfg.AddConsole();
                cfg.AddDebug();
            });

            services.AddSingleton<ILocalSettingService, LocalSettingService>();
            services.AddMtgDatabase();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<CardSearchViewModel>();
            services.AddSingleton<CardListViewModel>();
            services.AddSingleton<InventoryViewModel>();
            services.AddSingleton<DatabaseInfoViewModel>();
            services.AddSingleton<CardListViewModel>();

            services.AddSingleton<MainWindow>();
        }
    }
}