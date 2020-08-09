using ReactiveUI;

namespace MkmApi.TestUI.ViewModels
{
    public class TestViewModel : ReactiveObject
    {
        private string _test;
        public string Test
        {
            get => _test;
            set => this.RaiseAndSetIfChanged(ref _test, value);
        }
    }
}