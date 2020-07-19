using System;
using System.Linq;
using System.Reactive;
using MtgBinder.Domain.Database;
using ReactiveUI;
using ScryfallApi.Client.Models;

namespace MtgBinder.Avalonia.ViewModels.Lookup
{
    public class CardLookupViewModel : ReactiveObject
    {
        private readonly ICardDatabase _database;

        private CardViewModel[] _lookupResults;

        public CardLookupViewModel(ICardDatabase database)
        {
            _database = database;
            LookupCards = ReactiveCommand.Create(RunLookupCards);
        }

        public ReactiveCommand<Unit, Unit> LookupCards { get; }

        public string SearchPattern { get; set; }

        public CardViewModel[] LookupResult
        {
            get => _lookupResults;
            set => this.RaiseAndSetIfChanged(ref _lookupResults, value);
        }

        public CardViewModel SelectedCard { get; set; }

        public SearchOptions.RollupMode[] AvailableRollupModes =>
            new[] { SearchOptions.RollupMode.Cards, SearchOptions.RollupMode.Prints };

        public SearchOptions.RollupMode CardRollupMode { get; set; } = SearchOptions.RollupMode.Cards;

        public Uri SelectedCardUri => SelectedCard?.Card.ImageUrls.FirstOrDefault(i => i.Key == "normal")?.Url ?? SelectedCard?.Card.ImageUrls.FirstOrDefault()?.Url;

        public void RunLookupCards()
        {
            if (string.IsNullOrEmpty(SearchPattern))
            {
                return;
            }

            SelectedCard = null;
            LookupResult = _database.LookupCards(SearchPattern, CardRollupMode).Select(c => new CardViewModel(c, _database)).ToArray();
        }
    }
}