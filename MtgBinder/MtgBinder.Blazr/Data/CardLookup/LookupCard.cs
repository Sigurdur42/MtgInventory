using System;
using MtgDomain;

namespace MtgBinder.Blazr.Data.CardLookup
{
    public class LookupCard
    {
        public LookupCard(CardInfo card)
        {
            Name = card.Name;
            SetCode = card.SetCode;
            Image = card.GetNormalImage();
        }

        public string Name { get; set; }
        
        public string SetCode { get; set; }

        public Uri Image { get; set; }
    }
}