using MtgDatabase;

namespace MtgInventoryWpf
{
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
                DatabaseStatusSymbol = ":hourglass-half:";
            };

            autoAupdateMtgDatabaseService.UpdateFinished += (sender, e) =>
            {
                DatabaseStatusSymbol = ":database:";
            };
        }

        public DatabaseInfoViewModel DatabaseInfoViewModel { get; }
        public CardSearchViewModel CardSearchViewModel { get; }
        public InventoryViewModel InventoryViewModel { get; }

        public string DatabaseStatusSymbol { get; set; } = ":database:";
    }
}
