using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MtgBinder.Domain.Decks;
using MtgBinder.Domain.Service;
using MtgBinder.Domain.Tools;
using PropertyChanged;

namespace MtgBinder.Decks
{
    [AddINotifyPropertyChangedInterface]
    public class DeckListViewModel
    {
        private readonly DeckList _deckList;

        public DeckListViewModel(DeckList deckList, ICardService cardService, IAsyncProgressNotifier progress)
        {
            var action = $"Deck lookup for {deckList.Name}";
            progress.Start(action, deckList.TotalCards);
            _deckList = deckList;
            Cards = _deckList.Mainboard.Select(c =>
            {
                progress.NextStep(action);
                return new DeckListItemViewModel(c, cardService);
            }).ToArray();

            progress.Finish(action);
        }

        public string Name
        {
            get => _deckList.Name;
            set => _deckList.Name = value;
        }

        public DeckListItemViewModel[] Cards { get; }

        public int NumberOfCards => _deckList.TotalCards;
    }
}
