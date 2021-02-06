using System;
using System.Collections.Generic;
using System.Text;
using MtgDatabase;
using MtgDatabase.Models;
using PropertyChanged;

namespace MtgInventoryWpf
{
    [AddINotifyPropertyChangedInterface]
    public class CardListViewLineViewModel
    {
        private readonly IMtgDatabaseService _mtgDatabaseService;

        public CardListViewLineViewModel(
            IMtgDatabaseService mtgDatabaseService,
            QueryableMagicCard? card)
        {
            _mtgDatabaseService = mtgDatabaseService;
            Card = card;
            Reprints = card != null ? new[] { card } : Array.Empty<QueryableMagicCard>();
        }

        public int Quantity { get; set; }

        public string Category { get; set; } = "";

        public QueryableMagicCard? Card { get; set; }

        public QueryableMagicCard[] Reprints { get; set; } = Array.Empty<QueryableMagicCard>();

        public void UpdateReprints()
        {
            if (Card == null)
            {
                Reprints = Array.Empty<QueryableMagicCard>();
                return;
            }

            // TODO: Move to shared function
            Reprints = _mtgDatabaseService.Cards?
                                       .Query()
                                       .Where(c => c.Name.Equals(Card.Name, StringComparison.InvariantCultureIgnoreCase))
                                       .ToArray() ?? Array.Empty<QueryableMagicCard>();
        }



        public void ChangeSetOfCard(QueryableMagicCard newSelection)
        {
            Card = newSelection;
        }
    }
}
