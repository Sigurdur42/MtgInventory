using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MtgBinder.Blazr.Data;
using MtgBinder.Blazr.Data.CardLookup;
using MtgBinder.Domain.Configuration;
using MtgBinder.Domain.Database;
using MtgBinder.Domain.Scryfall;
using MtgBinder.Domain.Service;
using MtgBinder.Domain.Tools;
using ScryfallApi.Client;
using Serilog;
using Serilog.Events;

namespace MtgBinder.Blazr
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();


            var progressNotifier = new AsyncProgressNotifier();
            services.AddSingleton<IAsyncProgressNotifier>(progressNotifier);
            services.AddSingleton<IAsyncProgress>(progressNotifier);

            services.AddHttpClient<ScryfallApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://api.scryfall.com/");
            });

            services.AddSingleton<IScryfallService, ScryfallService>();

            var configurationFolderProvider = new UserDataFolderProvider();
            services.AddSingleton<IUserDataFolderProvider>(configurationFolderProvider);
            services.AddSingleton<ICardDatabase, CardDatabase>();
            services.AddSingleton<ICardService, CardService>();

            // Blazr services
            services.AddSingleton<CardLookupService>();

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
                .WriteTo.File(targetFileName);

            if (Debugger.IsAttached)
            {
                logConfig = logConfig.WriteTo.Debug();
            }

            Log.Logger = logConfig.CreateLogger();

            Log.Information($"Initialising MtgBinder.Blazr...");

            // For MatBlazor:
            if (services.All(x => x.ServiceType != typeof(HttpClient)))
            {
                services.AddScoped(
                    s =>
                    {
                        var navigationManager = s.GetRequiredService<NavigationManager>();
                        return new HttpClient
                        {
                            BaseAddress = new Uri(navigationManager.BaseUri)
                        };
                    });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}