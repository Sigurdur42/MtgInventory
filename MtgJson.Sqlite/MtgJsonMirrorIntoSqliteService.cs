using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MtgJson.Sqlite.Models;
using MtgJson.Sqlite.Repository;

namespace MtgJson.Sqlite
{
    public class MtgJsonMirrorIntoSqliteService : IMtgJsonMirrorIntoSqliteService
    {
        private readonly ILogger<MtgJsonMirrorIntoSqliteService> _logger;
        private readonly IMtgJsonService _mtgJsonService;

        public MtgJsonMirrorIntoSqliteService(
            ILogger<MtgJsonMirrorIntoSqliteService> logger,
            IMtgJsonService mtgJsonService)
        {
            _logger = logger;
            _mtgJsonService = mtgJsonService;
        }

        public async Task CreateLocalSqliteMirror(
            FileInfo targetFile,
            bool updatePriceDataOnly)
        {
            IList<DbCard> cards;
            SqliteDatabaseContext context = null;
            try
            {
                if (!updatePriceDataOnly)
                {
                    _logger.LogInformation($"Copying empty reference database...");
                    CopyReferenceDatabase(targetFile);

                    _logger.LogInformation($"Downloading card data...");
                    cards = await DownloadDatabase();
                    context = new SqliteDatabaseContext(targetFile);
                }
                else
                {
                    _logger.LogInformation($"Loading all cards from local database...");
                    context = new SqliteDatabaseContext(targetFile);
                    cards = context.Cards?.ToList() ?? new List<DbCard>();
                }

                _logger.LogInformation($"Updating price data...");
                UpdatePriceData(cards);

                if (!updatePriceDataOnly)
                {
                    _logger.LogInformation($"Bulk insert card data...");
                    await context.BulkInsertAsync(cards);
                }
                else
                {
                    _logger.LogInformation($"Bulk update card data...");
                    await context.BulkUpdateAsync(cards);
                }
            }
            finally
            {
                context?.Dispose();
            }
        }

        private void CopyReferenceDatabase(FileInfo targetFile)
        {
            var source = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "SqliteMirror.sqlite");

            if (!targetFile.Directory.Exists)
            {
                targetFile.Directory.Create();
            }

            File.Copy(source, targetFile.FullName, true);
        }

        private async Task<IList<DbCard>> DownloadDatabase()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                _logger.LogInformation("Downloading AllPrintings now");
                using var httpClient = new HttpClient();
                using var response = await httpClient.GetAsync("https://mtgjson.com/api/v5/AllPrintingsCSVFiles.zip");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to download file: {response.StatusCode}");
                    return new List<DbCard>();
                }

                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    await using var fileStream = File.Create(tempFile);
                    await stream.CopyToAsync(fileStream);
                }

                _logger.LogInformation($"Downloaded AllPrintingsCSVFiles to temp folder - starting analysis");

                var cardFactory = new DbCardFactory();

                _mtgJsonService.DownloadAllPrintingsZip(
                    new FileInfo(tempFile),
                    (header) =>
                    {
                        // TODO
                        // cardDate = DateTime.Parse(header.Date, );
                        return true;
                        // return _mtgJsonLiteDbService.OnPriceDataHeaderLoaded(header);
                    },
                    (sets) =>
                    {
                        cardFactory.LoadedSets = sets.ToArray();
                        return true;
                    },
                    (cards) =>
                    {
                        cardFactory.AllCards = cards.ToDictionary(c => c.Id);
                        return true;
                    },
                    (foreignData) =>
                    {
                        cardFactory.ForeignByCard = foreignData.GroupBy(f => f.CardId).ToArray();
                        return true;
                    },
                    (legalities) =>
                    {
                        cardFactory.LegalitiesByCard = legalities.GroupBy(f => f.CardId).ToArray();
                        return true;
                    });

                return cardFactory.CreateCards();
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        private void UpdatePriceData(IList<DbCard> allCards)
        {
            var insertTasks = new List<Task>();
            var byCardId = allCards.ToDictionary(c => c.Uuid);

            _mtgJsonService.DownloadPriceDataAsync(
                    new FileInfo(@"C:\pCloudSync\MtgInventory\AllPrices.json"),
                    (header) =>
                    {
                        // Console.WriteLine($"Header: Header: {header.Date} - Version: {header.Version}");
                        return true;
                    },
                    (filteredBatch) =>
                    {
                        var filteredArray = filteredBatch.ToArray();
                        var insertTask = Task.Factory.StartNew(() =>
                        {
                            foreach (var jsonCardPrice in filteredArray)
                            {
                                if (!byCardId.TryGetValue(jsonCardPrice.Id.ToString(), out var card))
                                {
                                    continue;
                                }

                                // We can do MKM only at this time
                                foreach (var jsonCardPriceItem in jsonCardPrice.Items)
                                {
                                    if (jsonCardPriceItem.IsFoil.Equals("foil", StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        card.MkmFoil = (decimal)jsonCardPriceItem.Price;
                                    }
                                    else
                                    {
                                        card.MkmNormal = (decimal)jsonCardPriceItem.Price;
                                    }
                                }
                            }
                        });

                        insertTasks.Add(insertTask);
                    },
                    new MtgJsonPriceFilter())
                .GetAwaiter()
                .GetResult();

            while (insertTasks.Any())
            {
                var task = insertTasks.FirstOrDefault();
                if (task != null)
                {
                    _logger.LogInformation($"{insertTasks.Count} insert tasks still in queue");
                    task.Wait();
                    insertTasks.Remove(task);
                }
            }
        }
    }
}