using Avalonia;
using Avalonia.Markup.Xaml;

namespace MtgBinders
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
