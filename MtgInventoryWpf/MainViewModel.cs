using System.Windows;
using MtgDatabase;
using PropertyChanged;

namespace MtgInventoryWpf
{
    [AddINotifyPropertyChangedInterface]
    public class MainViewModel
    {

        public MainViewModel(
            DatabaseInfoViewModel databaseInfoViewModel,
            CardSearchViewModel cardSearchViewModel,
            InventoryViewModel inventoryViewModel,
            IAutoAupdateMtgDatabaseService autoAupdateMtgDatabaseService)
        {
            DatabaseInfoViewModel = databaseInfoViewModel;
            CardSearchViewModel = cardSearchViewModel;
            InventoryViewModel = inventoryViewModel;

            autoAupdateMtgDatabaseService.UpdateStarted += (sender, e) =>
            {
                DatabaseStatusSymbol = ":hourglass:";
                StatusLineMessage = "Database update started...";
                DatabaseUpdateProgress = Visibility.Visible;
            };

            autoAupdateMtgDatabaseService.UpdateProgress += (sender, e) =>
            {
                DatabaseUpdateProgressValue = e;
            };

            autoAupdateMtgDatabaseService.UpdateFinished += (sender, e) =>
            {
                DatabaseStatusSymbol = ":database:";
                StatusLineMessage = "Database update finished.";
                DatabaseUpdateProgress = Visibility.Collapsed;
            };
        }

        public DatabaseInfoViewModel DatabaseInfoViewModel { get; }
        public CardSearchViewModel CardSearchViewModel { get; }
        public InventoryViewModel InventoryViewModel { get; }
        public string StatusLineMessage { get; set; } = "";

        public Visibility DatabaseUpdateProgress { get; set; } = Visibility.Collapsed;
        public int DatabaseUpdateProgressValue { get; set; } = 0;

        public string DatabaseStatusSymbol { get; set; } = ":database:";
    }
}
