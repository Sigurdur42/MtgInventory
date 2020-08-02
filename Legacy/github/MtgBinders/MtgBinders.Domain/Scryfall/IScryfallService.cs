using MtgBinders.Domain.ValueObjects;

namespace MtgBinders.Domain.Scryfall
{
    public interface IScryfallService
    {
        MtgFullCard LoadCardByScryfallId(string scryfallId);

        MtgSetInfo[] LoadAllSets();

        MtgFullCard[] LoadAllCards();

        MtgFullCard[] LoadCardsOfSet(string setCode);
    }
}