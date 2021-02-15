using System.Collections.Generic;
using System.Threading.Tasks;
using MtgDatabase.Models;

namespace MtgDatabase.MtgJson
{
    public interface IMirrorMtgJson
    {
        bool AreCardsOutdated { get; }
        bool IsPriceOutdated { get; }
        Task<IList<QueryableMagicCard>> DownloadDatabase(bool force);

        Task<IList<QueryableMagicCard>> UpdatePriceData(IList<QueryableMagicCard> allCards, bool force);

    }
}