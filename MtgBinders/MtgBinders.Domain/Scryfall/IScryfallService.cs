using MtgBinders.Domain.ValueObjects;

namespace MtgBinders.Domain.Scryfall
{
    public interface IScryfallService
    {
        MtgSetInfo[] LoadAllSets();
    }
}