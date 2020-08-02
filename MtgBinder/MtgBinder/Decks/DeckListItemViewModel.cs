using System;
using System.Collections.Generic;
using System.Text;
using MtgBinder.Domain.Database;
using MtgBinder.Domain.Decks;
using MtgBinder.Domain.Service;
using MtgDomain;

namespace MtgBinder.Decks
{
    public class DeckListItemViewModel
    {
        private readonly DeckItem _item;

        public DeckListItemViewModel(DeckItem item, ICardService cardService)
        {
            _item = item;
            Card = cardService.LookupCard(item.Name);
        }

        public CardInfo Card { get; }

        public string Name => Card.Name;

        public int Count
        {
            get => _item.Count;
            set => _item.Count = value;
        }
    }
}
