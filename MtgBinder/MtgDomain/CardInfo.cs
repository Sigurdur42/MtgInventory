using System;
using System.Diagnostics;

namespace MtgDomain
{
    [DebuggerDisplay("{SetCode} - {Name} - {CollectorNumber} - {ScryfallId}")]
    public class CardInfo
    {
        public string Id { get; set; }

        public Guid ScryfallId { get; set; }

        public string Name { get; set; }
        public string SetCode { get; set; }
        public decimal Cmc { get; set; }
        public string TypeLine { get; set; }
        public string OracleText { get; set; }

        public ImageUrl[] ImageUrls { get; set; }
        public string ManaCost { get; set; }
        public string[] ColorIdentity { get; set; }

        // TODO: Mache enum
        public string Rarity { get; set; }

        public CardLegality[] Legalities { get; set; }

        public DateTime Updated { get; set; }
        public bool IsReserverd { get; set; }

        public string CollectorNumber { get; set; }
    }
}