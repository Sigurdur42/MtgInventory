using MtgBinders.Domain.ValueObjects;

namespace MtgBinders.Domain.Scryfall
{
    public interface IScryfallService
    {
        MtgSetInfo[] LoadAllSets();

        MtgFullCard[] LoadAllCards();

        MtgFullCard[] LoadCardsOfSet(string setCode);
    }
}