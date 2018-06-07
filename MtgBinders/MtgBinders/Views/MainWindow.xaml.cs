using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MtgBinders.ViewModels;

namespace MtgBinders.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private MainWindowViewModel MainWindowViewModel => DataContext as MainWindowViewModel;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnUpdateSets(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel?.SystemPageViewModel?.UpdateSetsFromScryfall();
        }
    }
}