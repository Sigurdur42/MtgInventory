using LiteDB;
using MkmApi;
using System;
using System.IO;

namespace MtgInventory.Service.Database
{
    public sealed class CardDatabase : IDisposable
    {
        private LiteDatabase _cardDatabase;

        public bool IsInitialized { get; private set; }

        public ILiteCollection<ProductInfo> MkmProductInfo { get; private set; }

        public void Dispose()
        {
            ShutDown();
        }

        public void Initialize(
            DirectoryInfo folder)
        {
            folder.EnsureExists();
            var databaseFile = Path.Combine(folder.FullName, "CardDatabase.db");
            _cardDatabase = new LiteDatabase(databaseFile);

            MkmProductInfo = _cardDatabase.GetCollection<ProductInfo>();

            IsInitialized = true;
        }

        public void ShutDown()
        {
            IsInitialized = false;
            _cardDatabase?.Dispose();
            _cardDatabase = null;
        }
    }
}