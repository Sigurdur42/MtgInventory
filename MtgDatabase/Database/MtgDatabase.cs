﻿using System.IO;
using LiteDB;
using MtgDatabase.Models;

namespace MtgDatabase.Database
{
    public interface IQueryableCardsProvider
    {
        ILiteCollection<QueryableMagicCard>? Cards { get; }
    }

    public class MtgDatabase : IQueryableCardsProvider
    {
        private LiteDatabase? _database;

        public bool IsInitialized { get; private set; }

        public ILiteCollection<QueryableMagicCard>? Cards { get; private set; }

        public void Configure(DirectoryInfo folder)
        {
            if (!folder.Exists)
            {
                folder.Create();
            }

            _database = new LiteDatabase(Path.Combine(folder.FullName, "MtgDatabase.db"));
            Cards = _database.GetCollection<QueryableMagicCard>();

            var mapper = BsonMapper.Global;

            mapper.Entity<QueryableMagicCard>()
                .Id(x => x.UniqueId);

            IsInitialized = true;
        }

        internal void EnsureIndex()
        {
            Cards?.EnsureIndex(c => c.Name);
            Cards?.EnsureIndex(c => c.LocalName);
            Cards?.EnsureIndex(c => c.IsBasicLand);
            Cards?.EnsureIndex(c => c.IsToken);
            Cards?.EnsureIndex(c => c.IsCreature);

            Cards?.EnsureIndex(c => c.SetCode);
            Cards?.EnsureIndex(c => c.Language);
            Cards?.EnsureIndex(c => c.UpdateDateUtc);
        }

        public void ShutDown()
        {
            _database?.Dispose();
            _database = null;
        }
    }
}