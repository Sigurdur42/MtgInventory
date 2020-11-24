using System.IO;
using LiteDB;
using MtgDatabase.Models;

namespace MtgDatabase.Database
{
    public class MtgDatabase
    {
        private LiteDatabase? _database;

        public ILiteCollection<QueryableMagicCard>? Cards { get; private set; }

        public bool IsInitialized { get; private set; }

        public void Configure(DirectoryInfo folder)
        {
            if (!folder.Exists)
            {
                folder.Create();
            }

            _database = new LiteDatabase(Path.Combine(folder.FullName, "MtgDatabase.db"));
            Cards = _database.GetCollection<QueryableMagicCard>();


            IsInitialized = true;
        }

        public void ShutDown()
        {
            _database?.Dispose();
            _database = null;
        }
    }
}