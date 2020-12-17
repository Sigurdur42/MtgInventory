using System;
using System.IO;
using System.Threading.Tasks;
using Blazored.Toast;
using LocalSettings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MtgDatabase;
using MtgInventoryBlazor.Data;
using MudBlazor;
using MudBlazor.Services;
using Radzen;
using ScryfallApiServices;
using FileInfo = System.IO.FileInfo;

namespace MtgInventoryBlazor
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
            services.AddSingleton<MtgInventoryService>();
            services.AddSingleton<ILocalSettingService, LocalSettingService>();
            services.AddMtgDatabase();
            services.AddBlazoredToast();
            
            services.AddMudBlazorDialog();
            services.AddMudBlazorSnackbar();
            services.AddMudBlazorResizeListener();            

            services.AddAntDesign();

            services.AddScoped<DialogService>();
            services.AddScoped<NotificationService>();
            services.AddScoped<TooltipService>();
            services.AddScoped<ContextMenuService>();

            services.AddLogging(cfg =>
            {
                cfg.AddConfiguration(Configuration.GetSection("Logging"));
                cfg.AddConsole();
                cfg.AddDebug();
                //// cfg.AddProvider(new PanelLogSinkProvider());
            });
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            // Initialize configuration
            var baseFolder = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MtgDatabase"));
            var configuration = app.ApplicationServices.GetService<ILocalSettingService>();
            configuration?.Initialize(new FileInfo(Path.Combine(baseFolder.FullName, "settings.yaml")), SettingWriteMode.OnChange);
            
            // Initialize mtg app service
            var service = app.ApplicationServices.GetService<IMtgDatabaseService>();
            service?.Configure(baseFolder, new ScryfallConfiguration());
            
            configuration?.Set("dummy", "bla");
            configuration?.Set("dummy", "bla2");
            configuration?.Set("dummy2323", "test");
            configuration?.Set("IntTest", 42);
            
            var dum = configuration?.Get("dummy");
            var dum2 = configuration?.GetInt("IntTest");
            
            var mtgService = app.ApplicationServices.GetService<MtgInventoryService>();
            Task.Factory.StartNew(() => mtgService?.CreateDatabase());
        }
    }
}