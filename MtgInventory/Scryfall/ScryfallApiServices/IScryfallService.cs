using System.Collections.Generic;
using System.IO;
using ScryfallApi.Client.Models;

namespace ScryfallApiServices
{
    public interface IScryfallService
    {
        IEnumerable<Set> RetrieveSets();

        Card[] RetrieveCardsForSetCode(string setCode);

        Card[] RetrieveCardsByCardName(string cardName, SearchOptions.RollupMode rollupMode);

        Card[] RetrieveCardsByCardNameAndSet(string cardName, string setCode, SearchOptions.RollupMode rollupMode);

        void Configure(DirectoryInfo folder);
        void ShutDown();
    }
}