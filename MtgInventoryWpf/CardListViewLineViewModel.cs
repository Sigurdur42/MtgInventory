using System;
using System.Globalization;
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
            QueryableMagicCard? card,
            int quantity)
        {
            _mtgDatabaseService = mtgDatabaseService;
            Card = card;
            Quantity = quantity;

            Reprints = Card != null ? new[] { Card } : Array.Empty<QueryableMagicCard>();
            // UpdateReprints();
            UpdatePrice();
        }

        public QueryableMagicCard? Card { get; set; }
        public string Category { get; set; } = "";
        public string Price { get; set; } = "---";
        public decimal? PriceValue { get; set; }
        public int Quantity { get; set; }
        public QueryableMagicCard[] Reprints { get; set; } = Array.Empty<QueryableMagicCard>();

        public void ChangeSetOfCard(QueryableMagicCard newSelection)
        {
            Card = newSelection;
            UpdatePrice();
        }

        public void UpdateReprints()
        {
            if (Reprints.Length > 1)
            {
                return;
            }

            if (Card == null)
            {
                Reprints = Array.Empty<QueryableMagicCard>();
                return;
            }

            // TODO: Move to shared function
            ////var id = Card.UniqueId ?? "";
            ////var prints = _mtgDatabaseService.Cards?
            ////                           .Query()
            ////                           .Where(c=> c.Language == "en")
            ////                           .Where(c => c.Name.Equals(Card.Name, StringComparison.InvariantCultureIgnoreCase))
            ////                           .Where(c=>c.UniqueId != id)
            ////                           .ToList() ?? new List<QueryableMagicCard>();

            ////    prints.Add(Card);

            ////Reprints = prints.ToArray();
        }

        private void UpdatePrice()
        {
            if (Card?.Eur.HasValue ?? false)
            {
                PriceValue = Quantity * Card.Eur;
                Price = PriceValue?.ToString("F2", CultureInfo.CurrentUICulture) ?? "---";
                return;
            }

            Price = "---";
            PriceValue = null;
        }
    }
}