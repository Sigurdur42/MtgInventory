using System;
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
        private readonly IMtgJsonService _mtgJsonService;
        private readonly ILiteDbService _mtgJsonLiteDbService;
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
            var total = 0;
            _mtgJsonService.DownloadPriceDataAsync(
                // new FileInfo(@"C:\pCloudSync\MtgInventory\AllPrices.json"),
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
                })
                .GetAwaiter()
                .GetResult();

            return -1;
        }
    }
}