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
        private readonly IScryfallService _scryfallService;

        public ApiAction(
            ILogger<ApiAction> logger,
            IScryfallService scryfallService,
            IMtgDatabaseService mtgDatabaseService)
        {
            _logger = logger;
            _scryfallService = scryfallService;
            _mtgDatabaseService = mtgDatabaseService;
        }

        public void Report(int value) => Console.WriteLine($"Database progress: {value}%");

        public int RunAction(ApiOptions options)
        {
            var total = 0;
            var service = new MtgJsonService();
            service.DownloadPriceData(
                new FileInfo(@"C:\pCloudSync\MtgInventory\AllPrices.json"),
                (header) =>
                {
                    Console.WriteLine($"Header: Header: {header.Date} - Version: {header.Version}");
                    return true;
                },
                (loaded) =>
                {
                    var step = loaded.Count();
                    total += step;
                    Console.WriteLine($"Loaded {total} cards");
                });

            return -1;
        }
    }
}