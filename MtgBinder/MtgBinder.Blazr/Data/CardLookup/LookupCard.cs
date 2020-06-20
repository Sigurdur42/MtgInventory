using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MtgDomain;

namespace MtgBinder.Blazr.Data.CardLookup
{
    public class LookupCard
    {
        public LookupCard(CardInfo card)
        {
            Name = card.Name;
            SetCode = card.SetCode;
        }

        public string Name { get; set; }
        public string SetCode { get; set; }
    }
}
