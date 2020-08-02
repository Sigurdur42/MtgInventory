using System.Collections.Generic;
using MtgDomain;
using ScryfallApi.Client.Models;

namespace MtgBinder.Domain.Scryfall
{
    public interface IScryfallService
    {
        IEnumerable<SetInfo> RetrieveSets();

        ScryfallCardData[] RetrieveCardsForSetCode(string setCode);

        ScryfallCardData[] RetrieveCardsByCardName(string cardName, SearchOptions.RollupMode rollupMode);
    }
}