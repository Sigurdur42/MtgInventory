using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        void ClearDatabase();
        void InsertScryfallCards(ScryfallCard[] cards);
        void InsertScryfallSets(ScryfallSet[] sets);
    }

    public class ScryfallDatabase : IScryfallDatabase
    {
        private readonly ILogger<ScryfallDatabase> _logger;
        private LiteDatabase? _scryfallDatabase;

        public ScryfallDatabase(ILogger<ScryfallDatabase> logger)
        {
            _logger = logger;
        }

        public bool IsInitialized { get; private set; }
        public ILiteCollection<ScryfallCard>? ScryfallCards { get; private set; }
        public ILiteCollection<ScryfallSet>? ScryfallSets { get; private set; }

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

        public void ClearDatabase()
        {
            VerifyConfigured();

            _logger.LogDebug("Cleaning existing card info...");
            ScryfallCards?.DeleteAll();
        }

        public void InsertScryfallCards(ScryfallCard[] cards)
        {
            VerifyConfigured();

            _logger.LogTrace($"Inserting {cards.Count()} new scryfall cards...");
            var cardsToInsert = new List<ScryfallCard>();
            var cardsToUpdate = new List<ScryfallCard>();
            foreach (var card in cards)
            {
                var found = ScryfallCards?.Query()?.Where(c => c.Id == card.Id)?.FirstOrDefault();
                if (found == null)
                {
                    cardsToInsert.Add(card);
                    continue;
                }
                
                cardsToUpdate.Add(card);
            }


            if (cardsToInsert.Any())
            {
                ScryfallCards?.InsertBulk(cardsToInsert);
            }
            
            if (cardsToUpdate.Any())
            {
                ScryfallCards?.Update(cardsToUpdate);
            }

            ScryfallCards?.EnsureIndex(e => e.Set);
            ScryfallCards?.EnsureIndex(e => e.Name);
            ScryfallCards?.EnsureIndex(e => e.UpdateDateUtc);
        }

        public void InsertScryfallSets(ScryfallSet[] sets)
        {
            VerifyConfigured();
            _logger.LogInformation("Cleaning existing set info...");
            ScryfallSets?.DeleteAll();

            _logger.LogInformation($"Inserting {sets.Count()} sets...");
            ScryfallSets?.InsertBulk(sets);

            ScryfallSets?.EnsureIndex(e => e.Code);
            ScryfallSets?.EnsureIndex(e => e.Name);
            ScryfallSets?.EnsureIndex(e => e.UpdateDateUtc);
        }

        private void VerifyConfigured()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Scryfall service not initialized - Call .Configure() in your startup");
            }
        }
    }
}