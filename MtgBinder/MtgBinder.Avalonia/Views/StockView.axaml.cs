using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MtgBinder.Avalonia.Views
{
    public class StockView : UserControl
    {
        public StockView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
