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

        ////     public IReadOnlyDictionary<string, IEnumerable<MtgFullCard>> CardsByName { get; private set; }

        public void SetCardData(IEnumerable<MtgFullCard> cardData)
        {
            CardData = cardData.ToArray();
            NumberOfCards = CardData.Length;

            ////// Create cache by name
            ////var dict = new Dictionary<string, IEnumerable<MtgFullCard>>();
            ////foreach (var t in CardData.GroupBy(g => g.Name))
            ////{
            ////    dict.Add(t.Key, t);
            ////}

            ////CardsByName = dict;
        }

        public void ReplaceCardsForSet(IEnumerable<MtgFullCard> cards, string setCode)
        {
            var cardData = CardData.Where(c => c.SetCode != setCode).ToList();
            cardData.AddRange(cards);
            SetCardData(cardData);
        }
    }
}