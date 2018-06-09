using System.Collections.Generic;
using System.Linq;
using MtgBinders.Domain.ValueObjects;

namespace MtgBinders.Domain.Entities
{
    internal class MtgCardRepository : IMtgCardRepository
    {
        public MtgCardRepository()
        {
            CardData = new MtgFullCard[0];
        }

        public int NumberOfCards { get; private set; }
        public MtgFullCard[] CardData { get; private set; }

        public void SetCardData(IEnumerable<MtgFullCard> cardData)
        {
            CardData = cardData.ToArray();
            NumberOfCards = CardData.Length;
        }

        public void ReplaceCardsForSet(IEnumerable<MtgFullCard> cards, string setCode)
        {
            var cardData = CardData.Where(c => c.SetCode != setCode).ToList();
            cardData.AddRange(cards);
            SetCardData(cardData);
        }
    }
}