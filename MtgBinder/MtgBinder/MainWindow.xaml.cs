using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using MtgBinder.Configuration;
using MtgBinder.Database;
using MtgBinder.Domain.Configuration;
using MtgBinder.Domain.Database;
using MtgBinder.Domain.Tools;

namespace MtgBinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly ICardDatabase _cardDatabase;
        private readonly IUserDataFolderProvider _userDataFolderProvider;

        public MainWindow(
            MainViewModel viewModel,
            ICardDatabase cardDatabase,
            IUserDataFolderProvider userDataFolderProvider,
            IAsyncProgress asyncProgress)
        {
            _viewModel = viewModel;
            _cardDatabase = cardDatabase;
            _userDataFolderProvider = userDataFolderProvider;
            InitializeComponent();

            DataContext = _viewModel;

            asyncProgress.Starting += (sender, args) => viewModel.LogProgressViewModel.UpdateProgressBar(args);
            asyncProgress.Finishing += (sender, args) => viewModel.LogProgressViewModel.UpdateProgressBar(args);
            asyncProgress.Progress += (sender, args) => viewModel.LogProgressViewModel.UpdateProgressBar(args);
        }

        private void OnUpdateSetDatabase(object sender, RoutedEventArgs e)
        {
            RunAsyncAction(() => _viewModel.CardDatabaseViewModel.CardDatabase.UpdateSetDataFromSryfall());
        }

        private void OnUpdateMissingCardDatabase(object sender, RoutedEventArgs e)
        {
            RunAsyncAction(() => _viewModel.CardDatabaseViewModel.CardDatabase.UpdateMissingCardDataFromSryfall());
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            _cardDatabase?.Close();
        }

        private void OnInitialized(object sender, EventArgs e)
        {
            _cardDatabase.Initialize(_userDataFolderProvider.ConfigurationFolder);
        }

        private void OnUpdateCardsForSet(object sender, RoutedEventArgs e)
        {
            if (((Button)sender)?.DataContext is SetStaticData setStats)
            {
                RunAsyncAction(() => _cardDatabase.LoadCardsForSet(setStats.SetCode));
            }
        }

        private void OnCardLookup(object sender, RoutedEventArgs e)
        {
            RunAsyncAction(() => _viewModel.LookupViewModel.Lookup());
        }

        private void OnLoadDeckListFromFile(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Title = "Open deck list from file",
                Multiselect = false
            };

            if (openDialog.ShowDialog(this) == true)
            {
                RunAsyncAction(() => _viewModel.LoadDeckViewModel.LoadDeckFromFile(new FileInfo(openDialog.FileName)));
            }
        }

        private void RunAsyncAction(Action actionToRun)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                rootTab.IsEnabled = false;
                Task.Factory.StartNew(actionToRun)
                    .ContinueWith((task) =>
                    {
                        rootTab.IsEnabled = true;
                        Mouse.OverrideCursor = null;

                        if (task.IsFaulted)
                        {
                            var exception = task.Exception?.InnerException ??
                                            new InvalidOperationException("Unknown bad things happened");
                            MessageBox.Show(this, exception.ToString());
                        }
                    }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception error)
            {
                Mouse.OverrideCursor = null;
                rootTab.IsEnabled = true;
                MessageBox.Show(this, error.Message);
            }
        }
    }
}