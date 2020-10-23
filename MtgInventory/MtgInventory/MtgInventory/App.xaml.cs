using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using MtgInventory.Logging;
using MtgInventory.Service;
using MtgInventory.ViewModels;
using MtgInventory.Views;
using Serilog;
using System;
using System.Globalization;

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
                .WriteTo.Console()
                .WriteTo.PanelLogSink(CultureInfo.InvariantCulture)
                .WriteTo.File(System.IO.Path.Combine(folder, "MtgInventory.log"), rollingInterval: RollingInterval.Day)
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