using System;
using System.Linq;
using MtgBinder.Domain.Database;
using PropertyChanged;
using ScryfallApi.Client.Models;

namespace MtgBinder.Lookup
{
    [AddINotifyPropertyChangedInterface]
    public class LookupViewModel
    {
        private readonly ICardDatabase _database;

        public LookupViewModel(ICardDatabase database)
        {
            _database = database;
        }

        public string SearchPattern { get; set; }

        public CardViewModel[] LookupResult { get; set; }

        [AlsoNotifyFor(nameof(SelectedCard))]
        public CardViewModel SelectedCard { get; set; }

        public SearchOptions.RollupMode[] AvailableRollupModes =>
            new[] { SearchOptions.RollupMode.Cards, SearchOptions.RollupMode.Prints };

        public SearchOptions.RollupMode CardRollupMode { get; set; } = SearchOptions.RollupMode.Cards;

        public Uri SelectedCardUri => SelectedCard?.Card.ImageUrls.FirstOrDefault(i => i.Key == "normal")?.Url ?? SelectedCard?.Card.ImageUrls.FirstOrDefault()?.Url;

        public void Lookup()
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