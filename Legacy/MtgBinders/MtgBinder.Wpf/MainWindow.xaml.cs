using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MtgBinder.Wpf.Dropbox;
using MtgBinder.Wpf.Logging;
using MtgBinder.Wpf.ViewModels;
using MtgBinders.Domain.Configuration;
using MtgBinders.Domain.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace MtgBinder.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _mainViewModel;

        private InventoryViewModel _inventoryViewModel;

        public MainWindow()
        {
            Bootstrap();
            InitializeComponent();
        }

        private void Bootstrap()
        {
            var serviceCollection = new ServiceCollection();

            // Bootstrap logger
            var loggerFactory = new LoggerFactory()
                // .AddConsole(LogLevel.Debug)
                .AddDebug(LogLevel.Debug);
            loggerFactory.AddProvider(new UiLoggingProvider());

            var initLogger = loggerFactory.CreateLogger("Bootstrap");

            initLogger.LogDebug("Configuring DI for MtgBinder...");
            serviceCollection
                .AddSingleton((serviceProvider) => loggerFactory)
                .AddSingleton<MainViewModel>()
                .AddSingleton<MainCardSearchViewModel>()
                .AddSingleton<SetListViewModel>()
                .AddSingleton<InventoryViewModel>()
                .AddSingleton<SystemPageViewModel>();

            // Configure DI
            BindMicrosoftDi.BindProductiveEnvironment(initLogger, serviceCollection);

            ApplicationSingeltons.ServiceProvider = serviceCollection.BuildServiceProvider();
            var domainConfiguration = ApplicationSingeltons.ServiceProvider.GetService<IBinderDomainConfigurationProvider>();
            var serializer = ApplicationSingeltons.ServiceProvider.GetService<IJsonConfigurationSerializer>();

            var dropbox = new LocalDropbox();
            var path = dropbox.FindDropboxFolder(serializer);

            path = path ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            domainConfiguration.Initialize(
                Path.Combine(path, "MtgBinder", "Data"));

            _inventoryViewModel = ApplicationSingeltons.ServiceProvider.GetService<InventoryViewModel>();

            _mainViewModel = ApplicationSingeltons.ServiceProvider.GetService<MainViewModel>();
            DataContext = _mainViewModel;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            _mainViewModel.OnShutdown();
            _inventoryViewModel?.SaveInventory();
        }
    }
}