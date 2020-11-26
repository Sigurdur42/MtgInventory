using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using MtgDatabase.Models;
using ScryfallApiServices;
using ScryfallApiServices.Models;

namespace MtgDatabase
{
    public interface IMtgDatabaseService : IDisposable
    {
        void Configure(DirectoryInfo folder);
        void CreateDatabase(bool clearScryfallMirror);
    }

    public class MtgDatabaseService : IMtgDatabaseService
    {
        private readonly ILogger<MtgDatabaseService> _logger;
        private readonly Database.MtgDatabase _database;
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

        public void CreateDatabase(bool clearScryfallMirror)
        {
            _scryfallService.RefreshLocalMirror(clearScryfallMirror);
            
            RebuildInternalDatabase();
        }

        private void RebuildInternalDatabase()
        {
            var stopwatch = Stopwatch.StartNew();
            
            var allCards = _scryfallService.ScryfallCards?.FindAll().ToArray() ?? Array.Empty<ScryfallCard>();
            _logger.LogTrace($"Retrieving all cards from cache took {stopwatch.Elapsed} for {allCards.Length} cards");
            
            stopwatch.Restart();
            var groupedByName = allCards.GroupBy(c => c.Name).ToArray();
            _logger.LogTrace($"Found {groupedByName.Length} distinct cards in {stopwatch.Elapsed}");
            stopwatch.Restart();

            // TEST ONLY
            var groupedTypelines = allCards
                .GroupBy(c => c.TypeLine)
                .OrderBy(c=>c.Key)
                .Select(c=>c.Key)
                .ToArray();
            var dummy = string.Join(Environment.NewLine, groupedTypelines);
            File.WriteAllText(@"C:\temp\typelines.txt", dummy);
            var cardFactory = new QueryableMagicCardFactory();
            var cardsToInsert = new List<QueryableMagicCard>();
            foreach (var group in groupedByName)
            {
                var card = cardFactory.Create(group);
            }
            
            stopwatch.Stop();
        } 

        public void Configure(DirectoryInfo folder)
        {
            _scryfallService.Configure(folder);
            _database.Configure(folder);
        }
        public void Dispose()
        {
            _database?.ShutDown();
        }
    }
}