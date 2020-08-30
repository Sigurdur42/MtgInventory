﻿using LiteDB;
using MkmApi;
using MkmApi.Entities;
using MtgInventory.Service.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MtgInventory.Service.Database
{
    public sealed class CardDatabase : IDisposable
    {
        private LiteDatabase _cardDatabase;

        public bool IsInitialized { get; private set; }

        public ILiteCollection<MkmProductInfo> MkmProductInfo { get; private set; }
        public ILiteCollection<Expansion> MkmExpansion { get; private set; }

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

            MkmProductInfo = _cardDatabase.GetCollection<MkmProductInfo>();
            MkmExpansion = _cardDatabase.GetCollection<Expansion>();

            IsInitialized = true;
        }

        public void ShutDown()
        {
            IsInitialized = false;
            _cardDatabase?.Dispose();
            _cardDatabase = null;
        }

        public void InsertProductInfo(
            IEnumerable<ProductInfo> products,
            IEnumerable<Expansion> expansions)
        {
            MkmProductInfo.DeleteAll();

            var orderedExpansions = expansions.ToDictionary(_ => _.IdExpansion.ToString(CultureInfo.InvariantCulture) ?? "");

            var temp = new List<MkmProductInfo>();

            foreach (var p in products)
            {
                var product = new MkmProductInfo(p);
                if (orderedExpansions.TryGetValue(p.ExpansionId?.ToString(CultureInfo.InvariantCulture) ?? "", out var foundExpansion))
                {
                    product.ExpansionName = foundExpansion.EnName;
                    product.ExpansionCode = foundExpansion.Abbreviation;
                }

                temp.Add(product);

                if (temp.Count >= 1000)
                {
                    BulkInsertProductInfo(temp);
                    temp.Clear();
                }
            }

            BulkInsertProductInfo(temp);
        }

        private void BulkInsertProductInfo(IList<MkmProductInfo> products)
        {
            MkmProductInfo.InsertBulk(products);
            MkmProductInfo.EnsureIndex(p => p.Name);
            MkmProductInfo.EnsureIndex(p => p.CategoryId);
        }

        internal void InsertExpansions(IEnumerable<Expansion> expansions)
        {
            MkmExpansion.DeleteAll();
            MkmExpansion.InsertBulk(expansions);

            MkmExpansion.EnsureIndex(e => e.EnName);
            MkmExpansion.EnsureIndex(e => e.IdExpansion);
            MkmExpansion.EnsureIndex(e => e.IdGame);
        }
    }
}