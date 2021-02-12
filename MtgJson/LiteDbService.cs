using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteDB;
using Microsoft.Extensions.Logging;
using MtgJson.Database;
using MtgJson.JsonModels;

namespace MtgJson
{
    public class LiteDbService : ILiteDbService
    {
        private readonly ILogger<LiteDbService> _logger;
        private LiteDatabase? _database;

        private ILiteCollection<DbPriceMeta>? _priceMeta;
        private ILiteCollection<DbPriceItem>? _priceItems;

        public LiteDbService(
            ILogger<LiteDbService> logger)
        {
            _logger = logger;
        }

        public void Configure(DirectoryInfo folder)
        {
            if (!folder.Exists)
            {
                folder.Create();
            }

            _database = new LiteDatabase(Path.Combine(folder.FullName, "MtgJsonMirror.litedb"));

            _priceMeta = _database.GetCollection<DbPriceMeta>();
            _priceItems = _database.GetCollection<DbPriceItem>();
        }

        public bool OnPriceDataHeaderLoaded(JsonMeta metaData)
        {
            if (_priceMeta == null)
            {
                _logger.LogWarning($"Missing {nameof(Configure)} call - aborting.");
                return false;
            }
            var priceMeta = _priceMeta.FindAll().FirstOrDefault();
            if (priceMeta == null)
            {
                priceMeta = new DbPriceMeta();
                _priceMeta.Insert(priceMeta);
            }

            if (priceMeta.Date == metaData.Date && priceMeta.Version == metaData.Version)
            {
                return false;
            }

            priceMeta.Date = metaData.Date;
            priceMeta.Version = metaData.Version;

            _priceMeta.Update(priceMeta);
            return true;
        }

        public void OnPriceDataBatchLoaded(IEnumerable<JsonCardPrice> loadedBatch)
        {
            if (_priceItems == null)
            {
                _logger.LogWarning($"Missing {nameof(Configure)} call - aborting.");
                return;
            }

            _priceItems.InsertBulk(loadedBatch.SelectMany(i =>
            {
                return i.Items.Select(b => new DbPriceItem()
                {
                    CardId = i.Id,
                    Date = b.Date,
                    BuylistOrRetail = b.BuylistOrRetail,
                    Currency = b.Currency,
                    FoilOrNormal = b.FoilOrNormal,
                    PaperOrOnline = b.PaperOrOnline,
                    Price = b.Price,
                    Seller = b.Seller,
                    Type = b.Type
                });
            }));

            _priceItems.EnsureIndex(i => i.CardId);
            _priceItems.EnsureIndex(i => i.Seller);
            _priceItems.EnsureIndex(i => i.Date);
        }

        public void Dispose() => _database?.Dispose();
    }
}