namespace MtgInventoryWpf
{
    public class MainViewModel
    {
        public MainViewModel(DatabaseInfoViewModel databaseInfoViewModel)
        {
            DatabaseInfoViewModel = databaseInfoViewModel;
        }

        public DatabaseInfoViewModel DatabaseInfoViewModel { get; }
    }
}
