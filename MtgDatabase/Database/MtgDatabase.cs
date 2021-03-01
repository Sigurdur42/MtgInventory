using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LiteDB;
using Microsoft.Extensions.Logging;
using MtgDatabase.Models;
using MtgJson.Sqlite.Repository;

namespace MtgDatabase.Database
{
    public interface IMtgRepositoryProvider
    {
        IMtgDataRepository CreateRepository();
    }

    public class MtgDatabase : IMtgRepositoryProvider
    {
        private readonly ILogger<MtgDatabase> _logger;
        private LiteDatabase? _database;

        public MtgDatabase(ILogger<MtgDatabase> logger)
        {
            _logger = logger;
        }

        public ILiteCollection<QueryableMagicCard>? Cards { get; private set; }
        public FileInfo? DatabaseFileName { get; private set; }
        public bool IsInitialized { get; private set; }

        public void Configure(DirectoryInfo folder)
        {
            if (!folder.Exists)
            {
                folder.Create();
            }

            DatabaseFileName = new FileInfo(Path.Combine(folder.FullName, "MtgDatabase.sqlite"));

            _database = new LiteDatabase(Path.Combine(folder.FullName, "MtgDatabase.d"));
            Cards = _database.GetCollection<QueryableMagicCard>();

            var mapper = BsonMapper.Global;

            mapper.Entity<QueryableMagicCard>()
                .Id(x => x.Id);

            IsInitialized = true;
        }

        public IMtgDataRepository CreateRepository()
        {
            if (DatabaseFileName == null)
            {
                return new EmptyMtgDataRepository();
            }

            return new SqliteDatabaseContext(DatabaseFileName);
        }

        public void ShutDown()
        {
            _database?.Dispose();
            _database = null;
        }

        internal void EnsureIndex()
        {
            Cards?.EnsureIndex(c => c.Name);
            Cards?.EnsureIndex(c => c.IsBasicLand);
            Cards?.EnsureIndex(c => c.IsToken);
            Cards?.EnsureIndex(c => c.IsCreature);

            Cards?.EnsureIndex(c => c.SetCode);
            Cards?.EnsureIndex(c => c.Language);
            Cards?.EnsureIndex(c => c.UpdateDateUtc);
        }

        internal void InsertOrUpdate(IList<QueryableMagicCard> allCards)
        {
            var stopwatch = Stopwatch.StartNew();
            var cardsToInsert = new List<QueryableMagicCard>();
            var cardsToUpdate = new List<QueryableMagicCard>();
            foreach (var card in allCards)
            {
                var found = Cards?.Query()
                    ?.Where(c => c.Id == card.Id)
                    ?.FirstOrDefault();

                if (found == null)
                {
                    cardsToInsert.Add(card);
                }
                else
                {
                    cardsToUpdate.Add(card);
                }
            }

            if (cardsToInsert.Any())
            {
                Cards?.InsertBulk(cardsToInsert);
            }

            if (cardsToUpdate.Any())
            {
                Cards?.Update(cardsToUpdate);
            }

            if (cardsToInsert.Any() || cardsToUpdate.Any())
            {
                EnsureIndex();
            }

            stopwatch.Stop();

            _logger.LogTrace(
                $"Inserted: {cardsToInsert.Count}, Updated: {cardsToUpdate.Count} in {stopwatch.Elapsed}");
        }
    }
}