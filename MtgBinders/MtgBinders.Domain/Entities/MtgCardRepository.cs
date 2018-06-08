using System.Collections.Generic;
using System.Linq;
using MtgBinders.Domain.ValueObjects;

namespace MtgBinders.Domain.Entities
{
    internal class MtgCardRepository
    {
        public int NumberOfCards { get; private set; }
        public MtgFullCard[] CardData { get; private set; }

        internal void SetCardData(IEnumerable<MtgFullCard> cardData)
        {
            CardData = cardData.ToArray();
            NumberOfCards = CardData.Length;
        }
    }
}