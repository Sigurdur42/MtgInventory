using System.Collections.Generic;
using MtgBinders.Domain.ValueObjects;

namespace MtgBinders.Domain.Entities
{
    public interface IMtgCardRepository
    {
        MtgFullCard[] CardData { get; }
        int NumberOfCards { get; }

        void ReplaceCardsForSet(IEnumerable<MtgFullCard> cards, string setCode);

        void SetCardData(IEnumerable<MtgFullCard> cardData);
    }
}