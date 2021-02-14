using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using Microsoft.Extensions.Logging;
using MtgJson.CsvModels;
using MtgJson.Database;
using MtgJson.JsonModels;

namespace MtgJson
{
    public class LiteDbService : ILiteDbService
    {
        private static readonly object _sync = new object();
        private readonly ILogger<LiteDbService> _logger;
        private ILiteCollection<CsvCard>? _csvCards;
        private ILiteCollection<CsvForeignData>? _csvForeignData;
        private ILiteCollection<CsvLegalities>? _csvLegalities;
        private ILiteCollection<CsvMeta>? _csvMeta;
        private ILiteCollection<CsvSet>? _csvSets;
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

            _csvMeta = _database.GetCollection<CsvMeta>();
            _csvCards = _database.GetCollection<CsvCard>();
            _csvLegalities = _database.GetCollection<CsvLegalities>();
            _csvForeignData = _database.GetCollection<CsvForeignData>();
            _csvSets = _database.GetCollection<CsvSet>();
        }

        public void DeleteExistingDatabase()
        {
            _priceMeta?.DeleteAll();
            _priceItems?.DeleteAll();
            _csvMeta?.DeleteAll();
            _csvCards?.DeleteAll();
            _csvLegalities?.DeleteAll();
            _csvForeignData?.DeleteAll();
            _csvSets?.DeleteAll();
        }

        public void Dispose() => _database?.Dispose();

        public void OnCardDataBatchLoaded(IEnumerable<CsvCard> loadedBatch)
        {
            if (_csvCards == null)
            {
                _logger.LogWarning($"Missing {nameof(Configure)} call - aborting.");
                return;
            }

            var filteredBatch = loadedBatch.ToArray();
            if (!filteredBatch.Any())
            {
                return;
            }

            var insertTask = Task.Factory.StartNew(() =>
            {
                _logger.LogInformation($"Inserting {filteredBatch.Length} card rows...");
                lock (_sync)
                {
                    _csvCards.InsertBulk(filteredBatch);
                    _csvCards.EnsureIndex(i => i.Name);
                }
            });

            InsertTasks.Add(insertTask);
        }

        public void OnSetDataBatchLoaded(IEnumerable<CsvSet> loadedBatch)
        {
            if (_csvSets == null)
            {
                _logger.LogWarning($"Missing {nameof(Configure)} call - aborting.");
                return;
            }

            var filteredBatch = loadedBatch.ToArray();
            if (!filteredBatch.Any())
            {
                return;
            }

            var insertTask = Task.Factory.StartNew(() =>
            {
                _logger.LogInformation($"Inserting {filteredBatch.Length} set rows...");
                lock (_sync)
                {
                    _csvSets.InsertBulk(filteredBatch);
                    _csvSets.EnsureIndex(i => i.Name);
                }
            });

            InsertTasks.Add(insertTask);
        }

        public void OnLegalitiyBatchLoaded(IEnumerable<CsvLegalities> loadedBatch)
        {
            if (_csvLegalities == null)
            {
                _logger.LogWarning($"Missing {nameof(Configure)} call - aborting.");
                return;
            }

            var filteredBatch = loadedBatch.ToArray();
            if (!filteredBatch.Any())
            {
                return;
            }

            var insertTask = Task.Factory.StartNew(() =>
            {
                _logger.LogInformation($"Inserting {filteredBatch.Length} legality rows...");
                lock (_sync)
                {
                    _csvLegalities.InsertBulk(filteredBatch);
             //       _csvLegalities.EnsureIndex(i => i.Name);
                }
            });

            InsertTasks.Add(insertTask);
        }

        public void OnForeignDataBatchLoaded(IEnumerable<CsvForeignData> loadedBatch)
        {
            if (_csvForeignData == null)
            {
                _logger.LogWarning($"Missing {nameof(Configure)} call - aborting.");
                return;
            }

            var filteredBatch = loadedBatch.ToArray();
            if (!filteredBatch.Any())
            {
                return;
            }

            var insertTask = Task.Factory.StartNew(() =>
            {
                _logger.LogInformation($"Inserting {filteredBatch.Length} foreign data rows...");
                lock (_sync)
                {
                    _csvForeignData.InsertBulk(filteredBatch);
                    // _csvForeignData.EnsureIndex(i => i.Name);
                }
            });

            InsertTasks.Add(insertTask);
        }

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