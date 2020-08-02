using System;
using System.Collections.Generic;
using System.Text;
using MtgDomain;

namespace MtgBinder.Domain.Scryfall
{
    public class ScryfallCardData
    {
        public CardInfo Card { get; set; }
        public CardPrice Price { get; set; }
    }
}
