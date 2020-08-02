using MtgBinders.Domain.ValueObjects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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

        public IReadOnlyDictionary<string, MtgFullCard> CardsByUniqueId { get; private set; }

        public void SetCardData(IEnumerable<MtgFullCard> cardData)
        {
            CardData = cardData.ToArray();
            NumberOfCards = CardData.Length;

            // Create cache by name
            CardsByUniqueId = CardData
                .Where(c => !string.IsNullOrWhiteSpace(c.UniqueId))
                .ToDictionary(g => g.UniqueId);
        }

        public void ReplaceCardsForSet(IEnumerable<MtgFullCard> cards, string setCode)
        {
            var cardData = CardData.Where(c => c.SetCode != setCode).ToList();
            cardData.AddRange(cards);
            SetCardData(cardData);
        }
    }
}