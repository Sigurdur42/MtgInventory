using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using MtgInventory.Service;
using MtgInventory.ViewModels;
using MtgInventory.Views;
using Serilog;
using System;

namespace MtgInventory
{
    public class App : Application
    {
        public override void Initialize()
        {
            var systemFolders = new SystemFolders();
            var folder = System.IO.Path.Combine(systemFolders.BaseFolder.FullName, "Logs");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                // .WriteTo.Console()
                .WriteTo.File(System.IO.Path.Combine(folder, "MtgInventory.log"), rollingInterval: RollingInterval.Minute)
                .CreateLogger();

            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}