using System;
using System.IO;
using System.Threading.Tasks;
using MtgDatabase.Database;
using MtgDatabase.DatabaseDecks;
using MtgDatabase.Models;

namespace MtgDatabase
{
    public interface IMtgDatabaseService : IQueryableCardsProvider, IDisposable
    {
        event EventHandler<DatabaseRebuildingEventArgs> OnRebuilding;

        bool IsRebuilding { get; }

        void Configure(DirectoryInfo folder, int downloadCardBatchSize);

        Task<DatabaseDeckReaderResult> ReadDeck(string name, string deckContent);

        Task RefreshLocalDatabaseAsync(IProgress<int>? progress, bool force);

        Task<QueryableMagicCard[]> SearchCardsAsync(MtgDatabaseQueryData queryData);
    }
}