using System.Reactive;
using MtgBinder.Avalonia.ViewModels.Lookup;
using ReactiveUI;

namespace MtgBinder.Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(
            CardLookupViewModel cardLookupViewModel)
        {
            CardLookup = cardLookupViewModel;
            LookupCards = ReactiveCommand.Create(RunLookupCards);

        }
        public ReactiveCommand<Unit, Unit> LookupCards { get; }


        public CardLookupViewModel CardLookup { get; }

        public void RunLookupCards() => CardLookup.RunLookupCards();
    }
}