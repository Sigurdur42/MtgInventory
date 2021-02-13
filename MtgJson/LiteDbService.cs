using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Logging;
using MtgJson.Database;
using MtgJson.JsonModels;

namespace MtgJson
{
    public class LiteDbService : ILiteDbService
    {
        private static object _sync = new object();
        private readonly ILogger<LiteDbService> _logger;
        private LiteDatabase? _database;

        private ILiteCollection<DbPriceItem>? _priceItems;
        private ILiteCollection<DbPriceMeta>? _priceMeta;

        public LiteDbService(
            ILogger<LiteDbService> logger)
        {
            _logger = logger;
        }

        public IList<Task> InsertTasks { get; } = new List<Task>();

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

        public void DeleteExistingDatabase()
        {
            _priceMeta?.DeleteAll();
            _priceItems?.DeleteAll();
        }

        public void Dispose() => _database?.Dispose();

        public void OnPriceDataBatchLoaded(IEnumerable<JsonCardPrice> loadedBatch)
        {
            if (_priceItems == null)
            {
                _logger.LogWarning($"Missing {nameof(Configure)} call - aborting.");
                return;
            }

            var filteredBatch = loadedBatch
                .Where(i => i.Items.Any())
                .ToArray();
            if (!filteredBatch.Any())
            {
                return;
            }

            var insertTask = Task.Factory.StartNew(() =>
            {
                var items = filteredBatch
                    .AsParallel()
                    .SelectMany(i =>
                    {
                        return i.Items.ToArray().Select(b => new DbPriceItem()
                        {
                            CardId = i.Id,
                            Date = b.Date,
                            BuylistOrRetail = b.BuylistOrRetail,
                            Currency = b.Currency,
                            IsFoil = b.IsFoil == "foil",
                            PaperOrOnline = b.PaperOrOnline,
                            Price = b.Price,
                            Seller = b.Seller,
                            Type = b.Type
                        }).ToArray();
                    })
                    .ToArray();

                _logger.LogInformation($"Inserting {items.Length} price rows...");
                lock (_sync)
                {
                    _priceItems.InsertBulk(items);

                    _priceItems.EnsureIndex(i => i.CardId);
                    _priceItems.EnsureIndex(i => i.Seller);
                    _priceItems.EnsureIndex(i => i.Date);
                }
            });

            InsertTasks.Add(insertTask);
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

        public void WaitOnInsertTasksAndClear()
        {
            while (InsertTasks.Any())
            {
                var task = InsertTasks.FirstOrDefault();
                if (task != null)
                {
                    _logger.LogInformation($"{InsertTasks.Count} insert tasks still in queue");
                    task.Wait();
                    InsertTasks.Remove(task);
                }
            }
        }
    }
}