using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MtgJson.JsonModels;
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
            bool updatePriceDataOnly,
            bool optionsDebugMode)
        {
            IList<DbCard> cards;
            IList<DbSet> sets;
            SqliteDatabaseContext context = null;
            try
            {
                if (!updatePriceDataOnly)
                {
                    _logger.LogInformation($"Copying empty reference database...");
                    CopyReferenceDatabase(targetFile);

                    _logger.LogInformation($"Downloading card data...");

                    var result = await DownloadDatabase(optionsDebugMode);

                    cards = result.cards;
                    sets = result.sets;
                    context = new SqliteDatabaseContext(targetFile);
                }
                else
                {
                    _logger.LogInformation($"Loading all cards from local database...");
                    context = new SqliteDatabaseContext(targetFile);
                    cards = context.Cards?.ToList() ?? new List<DbCard>();
                    sets = new List<DbSet>();
                }

                _logger.LogInformation($"Updating price data...");
                UpdatePriceData(cards, optionsDebugMode);

                if (!updatePriceDataOnly)
                {
                    _logger.LogInformation($"Bulk insert card data...");
                    await context.BulkInsertAsync(sets);
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

        private async Task<(IList<DbCard> cards, IList<DbSet> sets)> DownloadDatabase(bool useAlreadyDownloadedFile)
        {
            var tempFile = useAlreadyDownloadedFile
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AllPrintingsCSVFiles.zip")
            : Path.GetTempFileName();

            try
            {
                if (!useAlreadyDownloadedFile)
                {
                    _logger.LogInformation("Downloading AllPrintings now");
                    using var httpClient = new HttpClient();
                    using var response = await httpClient.GetAsync("https://mtgjson.com/api/v5/AllPrintingsCSVFiles.zip");
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError($"Failed to download file: {response.StatusCode}");
                        return (new List<DbCard>(), new List<DbSet>());
                    }

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        await using var fileStream = File.Create(tempFile);
                        await stream.CopyToAsync(fileStream);
                    }
                    _logger.LogInformation($"Downloaded AllPrintingsCSVFiles to temp folder - starting analysis");
                }
                else
                {
                    _logger.LogInformation($"Using local file {tempFile} - starting analysis");
                }

                var cardFactory = new DbCardFactory();

                _mtgJsonService.DownloadAllPrintingsZip(
                    new FileInfo(tempFile),
                    (header) => true,
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

                return (cardFactory.CreateCards(), cardFactory.CreateSets());
            }
            finally
            {
                if (!useAlreadyDownloadedFile && File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        private void UpdatePriceData(IList<DbCard> allCards, bool useLocalFile)
        {
            var insertTasks = new List<Task>();
            var byCardId = allCards.ToDictionary(c => c.Uuid);

            var priceBatchLoaded = new Action<IEnumerable<JsonCardPrice>>((filteredBatch) =>
            {
                var filteredArray = filteredBatch.ToArray();
                foreach (var jsonCardPrice in filteredArray)
                {
                    if (!byCardId.TryGetValue(jsonCardPrice.Id.ToString(), out var card))
                    {
                        continue;
                    }

                    // We can do MKM only at this time
                    foreach (var jsonCardPriceItem in jsonCardPrice.Items)
                    {
                        switch (jsonCardPriceItem.Seller)
                        {
                            case "cardmarket":
                                if (jsonCardPriceItem.IsFoil.Equals("foil", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    card.MkmFoil = (decimal)jsonCardPriceItem.Price;
                                }
                                else
                                {
                                    card.MkmNormal = (decimal)jsonCardPriceItem.Price;
                                }
                                break;

                            case "tcgplayer":
                                if (jsonCardPriceItem.IsFoil.Equals("foil", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    card.TcgPlayerFoil = (decimal)jsonCardPriceItem.Price;
                                }
                                else
                                {
                                    card.TcgPlayerNormal = (decimal)jsonCardPriceItem.Price;
                                }
                                break;
                        }


                    }
                }
            });

            var stopwatch = Stopwatch.StartNew();

            if (useLocalFile)
            {
                var localFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "AllPrices.json");
                _logger.LogInformation($"Using local file {localFile} for price download");
                _mtgJsonService.DownloadPriceDataAsync(
                        new FileInfo(localFile),
                        (header) => true,
                        priceBatchLoaded,
                        new MtgJsonPriceFilter())
                    .GetAwaiter()
                    .GetResult();
            }
            else
            {
                _logger.LogInformation($"Downloading price data from MtgJson.com");
                _mtgJsonService.DownloadPriceDataAsync(
                        (header) => true,
                        priceBatchLoaded,
                        new MtgJsonPriceFilter())
                    .GetAwaiter()
                    .GetResult();
            }

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

            stopwatch.Stop();
            _logger.LogInformation($"Price download took {stopwatch.Elapsed}");
        }
    }
}