using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Logging;
using MtgDatabase.Database;
using MtgDatabase.Models;
using ScryfallApiServices;
using ScryfallApiServices.Models;

namespace MtgDatabase
{
    public interface IMtgDatabaseService : IQueryableCardsProvider, IDisposable
    {
        void Configure(DirectoryInfo folder, ScryfallConfiguration configuration);
        void CreateDatabase(bool clearScryfallMirror, bool clearMtgDatabase);

        Task<QueryableMagicCard[]> SearchCardsAsync(MtgDatabaseQueryData queryData);
    }

    public class MtgDatabaseService : IMtgDatabaseService
    {
        private readonly Database.MtgDatabase _database;
        private readonly ILogger<MtgDatabaseService> _logger;
        private readonly IScryfallService _scryfallService;

        public MtgDatabaseService(
            ILogger<MtgDatabaseService> logger,
            Database.MtgDatabase database,
            IScryfallService scryfallService)
        {
            _logger = logger;
            _database = database;
            _scryfallService = scryfallService;
        }

        public void CreateDatabase(bool clearScryfallMirror, bool clearMtgDatabase)
        {
            _scryfallService.RefreshLocalMirror(clearScryfallMirror);

            RebuildInternalDatabase(clearMtgDatabase);
        }

        private ScryfallConfiguration? _scryfallConfiguration;
        public void Configure(DirectoryInfo folder, ScryfallConfiguration configuration)
        {
            _scryfallConfiguration = configuration;
            _logger.LogInformation($"Configuring {nameof(Database.MtgDatabase)} in {folder.FullName} with {Environment.NewLine}{configuration.DumpSettings()}");
            _scryfallService.Configure(folder, configuration);
            _database.Configure(folder);
        }

        public void Dispose() => _database?.ShutDown();

        public ILiteCollection<QueryableMagicCard>? Cards => _database?.Cards;

        public Task<QueryableMagicCard[]> SearchCardsAsync(MtgDatabaseQueryData queryData)
        {
            return Task.Run(() =>
            {
                if (!(queryData?.ContainsValidSearch() ?? false))
                {
                    return Array.Empty<QueryableMagicCard>();
                }

                var query = Cards?.Query();
                if (!string.IsNullOrWhiteSpace(queryData.Name))
                {
                    if (queryData.MatchExactName)
                    {
                        query = query?.Where(c => c.Name.Equals(queryData.Name, StringComparison.InvariantCultureIgnoreCase));
                    }
                    else
                    {
                        query = query?.Where(c => c.Name.Contains(queryData.Name, StringComparison.InvariantCultureIgnoreCase));
                    }
                }

                if (queryData.IsToken)
                {
                    query = query?.Where(c => c.IsToken);
                }

                return query?.ToArray() ?? Array.Empty<QueryableMagicCard>();
            });
        }

        private void RebuildInternalDatabase(bool clearDatabase)
        {
            if (clearDatabase)
            {
                _database.Cards?.DeleteAll();
            }

            var oldestCard = _scryfallService.ScryfallCards?.Query().OrderBy(c => c.UpdateDateUtc).FirstOrDefault()?.UpdateDateUtc ?? DateTime.MinValue;
            var oldestDate = DateTime.Now.AddDays(-1 * _scryfallConfiguration?.UpdateSetCacheInDays ?? 28);
            if (!clearDatabase && oldestDate <= oldestCard)
            {
                // Cards are up to date - skip this
                _logger.LogTrace($"All cards are up to date - skip rebuild");
                return;
            }
            
            var stopwatch = Stopwatch.StartNew();
            var allCards = _scryfallService.ScryfallCards?.FindAll().ToArray() ?? Array.Empty<ScryfallCard>();
            _logger.LogTrace($"Retrieving all cards from cache took {stopwatch.Elapsed} for {allCards.Length} cards");

            stopwatch.Restart();
            var groupedByName = allCards.GroupBy(c => c.Name).ToArray();
            _logger.LogTrace($"Found {groupedByName.Length} distinct cards in {stopwatch.Elapsed}");
            stopwatch.Restart();

            // TEST ONLY
            // var groupedTypelines = allCards
            //     .GroupBy(c => c.TypeLine)
            //     .OrderBy(c=>c.Key)
            //     .Select(c=>c.Key)
            //     .ToArray();
            // var dummy = string.Join(Environment.NewLine, groupedTypelines);
            // File.WriteAllText(@"C:\temp\typelines.txt", dummy);

            var cardFactory = new QueryableMagicCardFactory();
            var cardsToInsert = new List<QueryableMagicCard>();
            var cardsToUpdate = new List<QueryableMagicCard>();
            foreach (var group in groupedByName)
            {
                var card = cardFactory.Create(group);
                var found = _database.Cards?.Query()?.Where(c => c.Name == card.Name)?.FirstOrDefault();
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
                _database.Cards?.InsertBulk(cardsToInsert);
            }

            if (cardsToUpdate.Any())
            {
                _database.Cards?.Update(cardsToUpdate);
            }

            if (cardsToInsert.Any() || cardsToUpdate.Any())
            {
                _database.EnsureIndex();
            }

            stopwatch.Stop();

            _logger.LogTrace($"Inserted: {cardsToInsert.Count()}, Updated: {cardsToUpdate.Count()} in {stopwatch.Elapsed}");
        }
    }
}