using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MtgInventory.ViewModels;

namespace MtgInventory.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public static MainWindow? Instance { get; private set; }

        private void InitializeComponent()
        {
            Instance = this;

            AvaloniaXamlLoader.Load(this);

            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var viewModel = DataContext as MainWindowViewModel;
            viewModel?.ShutDown();
        }
    }
}