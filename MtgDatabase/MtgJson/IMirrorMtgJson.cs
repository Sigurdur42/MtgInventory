using System.Collections.Generic;
using System.Threading.Tasks;
using MtgDatabase.Models;

namespace MtgDatabase.MtgJson
{
    public interface IMirrorMtgJson
    {
        Task<IList<QueryableMagicCard>> DownloadDatabase(bool force);
    }
}