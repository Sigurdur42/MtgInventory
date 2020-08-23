using LiteDB;
using MkmApi;
using System;
using System.Collections.Generic;
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
            // define mappings
            ////var mapper = BsonMapper.Global;
            ////mapper.Entity<ProductInfo>()
            ////    .Id(p => p.Id);


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

        public void InsertProductInfo(IEnumerable<ProductInfo> products)
        {
            MkmProductInfo.DeleteAll();

            var temp = new List<ProductInfo>();

            foreach (var p in products)
            {
                temp.Add(p);

                if (temp.Count >= 1000)
                {
                    BulkInsertProductInfo(temp);
                    temp.Clear();
                }
            }

            BulkInsertProductInfo(temp);
        }

        private void BulkInsertProductInfo(IList<ProductInfo> products)
        {
            MkmProductInfo.InsertBulk(products);
            MkmProductInfo.EnsureIndex(p => p.Name);
        }
    }
}