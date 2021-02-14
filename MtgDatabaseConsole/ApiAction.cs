using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using MtgDatabase;
using MtgJson;
using ScryfallApiConsole;
using ScryfallApiServices;

namespace MtgDatabaseConsole
{
    public class ApiAction : IProgress<int>
    {
        private readonly ILogger<ApiAction> _logger;
        private readonly IMtgDatabaseService _mtgDatabaseService;
        private readonly ILiteDbService _mtgJsonLiteDbService;
        private readonly IMtgJsonService _mtgJsonService;
        private readonly IScryfallService _scryfallService;

        public ApiAction(
            ILogger<ApiAction> logger,
            IScryfallService scryfallService,
            IMtgDatabaseService mtgDatabaseService,
            IMtgJsonService mtgJsonService,
            ILiteDbService mtgJsonLiteDbService)
        {
            _logger = logger;
            _scryfallService = scryfallService;
            _mtgDatabaseService = mtgDatabaseService;
            _mtgJsonService = mtgJsonService;
            _mtgJsonLiteDbService = mtgJsonLiteDbService;
        }

        public void Report(int value) => Console.WriteLine($"Database progress: {value}%");

        public int RunAction(ApiOptions options)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Deleting mirror database...");
            _mtgJsonLiteDbService.DeleteExistingDatabase();

            _logger.LogInformation("Starting download...");

            DownloadCardDataFromLocalFile();
            DownloadPriceDataFromLocalFile();

            stopwatch.Stop();
            _logger.LogInformation($"All done in {stopwatch.Elapsed}");

            return -1;
        }

        private void DownloadCardDataFromLocalFile()
        {
            var localFile = new FileInfo(@"C:\Users\Micha\Downloads\AllPrintingsCsv.zip");

            _mtgJsonService.DownloadAllPrintingsZip(
                localFile,
                (header) =>
            {
                Console.WriteLine($"Printing Header: Header: {header.Date} - Version: {header.Version}");

                return true;
                // return _mtgJsonLiteDbService.OnPriceDataHeaderLoaded(header);
            },
                (sets) =>
                {
                    _mtgJsonLiteDbService.OnSetDataBatchLoaded(sets);
                    return true;
                },
                (cards) =>
                {
                    _mtgJsonLiteDbService.OnCardDataBatchLoaded(cards);
                    return true;
                },
                (foreignData) =>
                {
                    _mtgJsonLiteDbService.OnForeignDataBatchLoaded(foreignData);

                    return true;
                },
                (legalities) =>
                {
                    _mtgJsonLiteDbService.OnLegalitiyBatchLoaded(legalities);

                    return true;
                }
                );
        }

        private void DownloadPriceDataDirect()
        {
            var total = 0;

            _mtgJsonService.DownloadPriceDataAsync(
                //new FileInfo(@"C:\pCloudSync\MtgInventory\AllPrices.json"),
                (header) =>
                {
                    Console.WriteLine($"Header: Header: {header.Date} - Version: {header.Version}");
                    return _mtgJsonLiteDbService.OnPriceDataHeaderLoaded(header);
                },
                (loaded) =>
                {
                    _mtgJsonLiteDbService.OnPriceDataBatchLoaded(loaded);
                    var step = loaded.Count();
                    total += step;
                    Console.WriteLine($"Loaded {total} cards");
                },
                new MtgJsonPriceFilter())
                .GetAwaiter()
                .GetResult();

            _logger.LogInformation("Waiting for insert tasks ...");
            _mtgJsonLiteDbService.WaitOnInsertTasksAndClear();
        }

        private void DownloadPriceDataFromLocalFile()
        {
            var total = 0;

            _mtgJsonService.DownloadPriceData(
                new FileInfo(@"C:\Users\Micha\Downloads\AllPrices.json"),
                (header) =>
                {
                    Console.WriteLine($"Header: Header: {header.Date} - Version: {header.Version}");
                    return _mtgJsonLiteDbService.OnPriceDataHeaderLoaded(header);
                },
                (loaded) =>
                {
                    _mtgJsonLiteDbService.OnPriceDataBatchLoaded(loaded);
                    var step = loaded.Count();
                    total += step;
                    Console.WriteLine($"Loaded {total} cards");
                },
                new MtgJsonPriceFilter());
            _logger.LogInformation("Waiting for insert tasks ...");
            _mtgJsonLiteDbService.WaitOnInsertTasksAndClear();
        }
    }
}