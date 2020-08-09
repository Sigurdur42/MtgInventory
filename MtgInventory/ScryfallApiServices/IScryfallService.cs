using System.Collections.Generic;
using ScryfallApi.Client.Models;

namespace MtgBinder.Domain.Scryfall
{
    public interface IScryfallService
    {
        IEnumerable<Set> RetrieveSets();

        Card[] RetrieveCardsForSetCode(string setCode);

        Card[] RetrieveCardsByCardName(string cardName, SearchOptions.RollupMode rollupMode);
    }
}