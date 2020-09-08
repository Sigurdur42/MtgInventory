using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using LiteDB;
using MkmApi;
using MkmApi.Entities;
using MtgInventory.Service.Converter;
using MtgInventory.Service.Models;
using Serilog;

namespace MtgInventory.Service.Database
{
    public sealed class CardDatabase : IDisposable
    {
        private readonly ILogger _logger = Log.ForContext<CardDatabase>();
        private LiteDatabase _cardDatabase;

        public bool IsInitialized { get; private set; }

        public ILiteCollection<MkmProductInfo> MkmProductInfo { get; private set; }

        public ILiteCollection<Expansion> MkmExpansion { get; private set; }
        public ILiteCollection<ApiCallStatistics> ApiCallStatistics { get; private set; }

        public void Dispose()
        {
            ShutDown();
        }

        public void Initialize(
            DirectoryInfo folder)
        {
            _logger.Information($"{nameof(Initialize)}: Initializing database service...");

            folder.EnsureExists();
            var databaseFile = Path.Combine(folder.FullName, "CardDatabase.db");
            _cardDatabase = new LiteDatabase(databaseFile);

            MkmProductInfo = _cardDatabase.GetCollection<MkmProductInfo>();
            MkmExpansion = _cardDatabase.GetCollection<Expansion>();
            ApiCallStatistics = _cardDatabase.GetCollection<ApiCallStatistics>();

            IsInitialized = true;
        }

        public void ShutDown()
        {
            _logger.Information($"{nameof(ShutDown)}: Shutting down database service...");

            IsInitialized = false;
            _cardDatabase?.Dispose();
            _cardDatabase = null;
        }

        public void InsertProductInfo(
            IEnumerable<ProductInfo> products,
            IEnumerable<Expansion> expansions)
        {
            _logger.Information($"{nameof(InsertProductInfo)}: Cleaning existing product info...");
            MkmProductInfo.DeleteAll();

            var orderedExpansions = expansions.ToDictionary(_ => _.IdExpansion.ToString(CultureInfo.InvariantCulture) ?? "");

            var temp = new List<MkmProductInfo>();
            var total = 0;

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
                    total += temp.Count;
                    _logger.Information($"{nameof(InsertProductInfo)}: Inserting {temp.Count} products (total: {total}...");

                    BulkInsertProductInfo(temp);
                    temp.Clear();
                }
            }

            total += temp.Count;
            _logger.Information($"{nameof(InsertProductInfo)}: Inserting {temp.Count} products (total: {total}...");
            BulkInsertProductInfo(temp);
        }

        public void UpdateMkmStatistics(IApiCallStatistic mkmStatistic)
        {
            var found = ApiCallStatistics.Query().FirstOrDefault();
            if (found == null)
            {
                var newRecord = new ApiCallStatistics
                {
                    CountToday = mkmStatistic.CountToday,
                    CountTotal = mkmStatistic.CountTotal,
                    Today = mkmStatistic.Today,
                };
                ApiCallStatistics.Insert(newRecord);
            }
            else
            {
                found.CountToday = mkmStatistic.CountToday;
                found.CountTotal = mkmStatistic.CountTotal;
                found.Today = mkmStatistic.Today;
                ApiCallStatistics.Update(found);
            }
        }

        public IApiCallStatistic GetMkmCallStatistic()
        {
            var found = ApiCallStatistics.Query().FirstOrDefault();
            if (found == null)
            {
                found = new ApiCallStatistics
                {
                    CountToday = 0,
                    CountTotal = 0,
                    Today = DateTime.Now.Date,
                };
                ApiCallStatistics.Insert(found);
            }

            return found;
        }

        internal void InsertExpansions(IEnumerable<Expansion> expansions)
        {
            foreach (var exp in expansions)
            {
                exp.ReleaseDateParsed = exp.ReleaseDate.ParseDateTime();
            }

            MkmExpansion.DeleteAll();
            MkmExpansion.InsertBulk(expansions);

            MkmExpansion.EnsureIndex(e => e.EnName);
            MkmExpansion.EnsureIndex(e => e.IdExpansion);
            MkmExpansion.EnsureIndex(e => e.IdGame);
        }

        private void BulkInsertProductInfo(IList<MkmProductInfo> products)
        {
            MkmProductInfo.InsertBulk(products);
            MkmProductInfo.EnsureIndex(p => p.Name);
            MkmProductInfo.EnsureIndex(p => p.CategoryId);
        }
    }
}