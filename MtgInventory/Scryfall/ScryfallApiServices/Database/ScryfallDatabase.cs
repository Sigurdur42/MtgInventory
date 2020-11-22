using System;
using System.Collections.Generic;
using System.IO;
using LiteDB;
using Microsoft.Extensions.Logging;
using ScryfallApiServices.Models;

namespace ScryfallApiServices.Database
{
    public interface IScryfallDatabase
    {
        ILiteCollection<ScryfallCard>? ScryfallCards { get; }
        ILiteCollection<ScryfallSet>? ScryfallSets { get; }
        void Configure(DirectoryInfo folder);
        void ShutDown();
        void ClearScryfallCards();
        void InsertScryfallCards(IEnumerable<ScryfallCard> cards);
        void InsertScryfallSets(IEnumerable<ScryfallSet> sets);
    }

    public class ScryfallDatabase : IScryfallDatabase
    {
        private readonly ILogger<ScryfallDatabase> _logger;
        private LiteDatabase? _scryfallDatabase;

        public ScryfallDatabase(ILogger<ScryfallDatabase> logger)
        {
            _logger = logger;
            
        }
        public ILiteCollection<ScryfallCard>? ScryfallCards { get; private set; }
        public ILiteCollection<ScryfallSet>? ScryfallSets { get; private set; }
        public bool IsInitialized { get; private set; }
        
        public void Configure(DirectoryInfo folder)
        {
            if (!folder.Exists)
            {
                folder.Create();
            }
            _scryfallDatabase = new LiteDatabase(Path.Combine(folder.FullName, "ScryfallDatabase.db"));
            ScryfallCards = _scryfallDatabase.GetCollection<ScryfallCard>();
            ScryfallSets = _scryfallDatabase.GetCollection<ScryfallSet>();

            IsInitialized = true;
        }

        public void ShutDown()
        {
            _scryfallDatabase?.Dispose();
            _scryfallDatabase = null;
        }

        public void ClearScryfallCards()
        {
            _logger.LogDebug($"Cleaning existing card info...");
            ScryfallCards?.DeleteAll();
        }
        
        public void InsertScryfallCards(IEnumerable<ScryfallCard> cards)
        {
            // _// Logger.Information($"Inserting {cards.Count()} new scryfall cards...");
            ScryfallCards?.InsertBulk(cards);

            ScryfallCards?.EnsureIndex(e => e.Set);
            ScryfallCards?.EnsureIndex(e => e.Name);
        }

        public void InsertScryfallSets(IEnumerable<ScryfallSet> sets)
        {
            // _// Logger.Information($"{nameof(InsertScryfallSets)}: Cleaning existing set info...");
            ScryfallSets?.DeleteAll();
            ScryfallSets?.InsertBulk(sets);

            ScryfallSets?.EnsureIndex(e => e.Code);
            ScryfallSets?.EnsureIndex(e => e.Name);
        }
        
        internal void VerifyConfigured()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Scryfall service not initialized - Call .Configure() in your startup");
            }
        }
    }
}