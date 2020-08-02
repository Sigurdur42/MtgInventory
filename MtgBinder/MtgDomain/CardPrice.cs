using System;

namespace MtgDomain
{
    public class CardPrice
    {
        public Guid Id { get; set; }
        public Guid ScryfallId { get; set; }

        public DateTime DateUtc { get; set; }

        public string DateTimeLookup { get; set; }

        public decimal? Usd { get; set; }

        public decimal? UsdFoil { get; set; }

        public decimal? Eur { get; set; }

        public decimal? EurFoil { get; set; }

        public decimal? Tix { get; set; }

        public bool HasAny() => Usd.HasValue || UsdFoil.HasValue || Eur.HasValue || EurFoil.HasValue || Tix.HasValue;
    }
}