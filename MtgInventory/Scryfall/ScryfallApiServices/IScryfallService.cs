using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ScryfallApi.Client.Models;
using ScryfallApiServices.Database;
using ScryfallApiServices.Models;

namespace ScryfallApiServices
{
    public interface IScryfallService : IScryfallData
    {
        IEnumerable<ScryfallSet> RetrieveSets();

        ScryfallCard[] RetrieveCardsForSetCode(string setCode);

        ScryfallCard[] RetrieveCardsByCardName(string cardName, SearchOptions.RollupMode rollupMode);

        ScryfallCard[] RetrieveCardsByCardNameAndSet(string cardName, string setCode, SearchOptions.RollupMode rollupMode);

        void Configure(DirectoryInfo folder, ScryfallConfiguration configuration);
        
        void ShutDown();
        void RefreshLocalMirror(bool cleanDatabase);
    }
}