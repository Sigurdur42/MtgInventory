namespace MtgInventoryWpf
{
    public class MainViewModel
    {
        public MainViewModel(
            DatabaseInfoViewModel databaseInfoViewModel,
            CardSearchViewModel cardSearchViewModel,
            InventoryViewModel inventoryViewModel)
        {
            DatabaseInfoViewModel = databaseInfoViewModel;
            CardSearchViewModel = cardSearchViewModel;
            InventoryViewModel = inventoryViewModel;
        }

        public DatabaseInfoViewModel DatabaseInfoViewModel { get; }
        public CardSearchViewModel CardSearchViewModel { get; }
        public InventoryViewModel InventoryViewModel { get; }
    }
}
