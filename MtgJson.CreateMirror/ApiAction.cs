using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MtgJson.Sqlite;

namespace MtgJson.CreateMirror
{
    public class ApiAction : IProgress<int>
    {
        private readonly ILogger<ApiAction> _logger;
        private readonly IMtgJsonMirrorIntoSqliteService _mirrorService;

        public ApiAction(
            ILogger<ApiAction> logger,
            IMtgJsonMirrorIntoSqliteService mirrorService)
        {
            _logger = logger;
            _mirrorService = mirrorService;
        }

        public void Report(int value) => Console.WriteLine($"Database progress: {value}%");

        public int RunAction(ApiOptions options)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Creating local Sqlite copy...");

            _mirrorService.CreateLocalSqliteMirror(options.TargetFile)
                .GetAwaiter()
                .GetResult();

            stopwatch.Stop();
            _logger.LogInformation($"All done in {stopwatch.Elapsed}");

            return -1;
        }
    }
}