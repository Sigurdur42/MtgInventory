using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using Microsoft.Extensions.DependencyInjection;
using MtgBinders.Domain.DependencyInjection;
using MtgBinders.Domain.Scryfall;
using MtgBinders.ViewModels;
using MtgBinders.Views;
using MtgScryfall;

namespace MtgBinders
{
    class Program
    {
        static void Main(string[] args)
        {
            // Configure DI
            var services = new ServiceCollection();
            BindMicrosoftDi.BindProductiveEnvironment(services);

            var provider = services.BuildServiceProvider();
            var test = provider.GetService<IScryfallService>();


            BuildAvaloniaApp().Start<MainWindow>(() => new MainWindowViewModel());



        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .LogToDebug();
    }
}
