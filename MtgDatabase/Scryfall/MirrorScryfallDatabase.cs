using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LocalSettings;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MtgDatabase.Scryfall
{
    public interface IMirrorScryfallDatabase
    {
        Task DownloadDatabase(int batchSize, IProgress<int>? progress);
        event EventHandler<DownloadedCardsEventArgs> CardBatchDownloaded;
    }

    public class MirrorScryfallDatabase : IMirrorScryfallDatabase
    {
        private readonly ILogger<MirrorScryfallDatabase> _logger;
        private readonly ILocalSettingService _settingService;
        private const int _knownMaxCardCount = 305000;
        private int _totalCardCount;
        private const string _totalCountKey = "ScryfallLastDatabaseCardCount";

        public MirrorScryfallDatabase(
            ILogger<MirrorScryfallDatabase> logger,
            ILocalSettingService settingService)
        {
            _logger = logger;
            _settingService = settingService;
            _totalCardCount = settingService.GetInt(_totalCountKey);

            if (_totalCardCount < _knownMaxCardCount)
            {
                _totalCardCount = _knownMaxCardCount;
                settingService.Set(_totalCountKey, _totalCardCount);
            }
        }

        public event EventHandler<DownloadedCardsEventArgs> CardBatchDownloaded = (sender, args) => { };

        public async Task DownloadDatabase(int batchSize, IProgress<int>? progress)
        {
            using var httpClient = new HttpClient();

            _logger.LogTrace("Getting bulk info for all cards...");
            using var response = await httpClient.GetAsync(" https://api.scryfall.com/bulk-data/all_cards");

            string apiResponse = await response.Content.ReadAsStringAsync();
            var definition = new
            {
                updated_at = DateTime.MinValue,
                download_uri = "",
                content_encoding = ""
            };

            var responseRead = JsonConvert.DeserializeAnonymousType(apiResponse, definition);

            _logger.LogTrace($"Bulk data last updated at {responseRead.updated_at}...");

            _logger.LogTrace("Downloading all data now ...");
            var stopwatch = Stopwatch.StartNew();

            await using (var downloadStream = await httpClient.GetStreamAsync(responseRead.download_uri))
            {
                ReadFromStream(downloadStream, batchSize, progress);
            }

            stopwatch.Stop();
            _logger.LogTrace($"Downloading all data took {stopwatch.Elapsed} ...");
        }

        private void ReadFromStream(Stream inputStream, int batchSize, IProgress<int>? progress)
        {
            var result = new List<ScryfallJsonCard>();
            var serializer = new JsonSerializer();

            using var sr = new StreamReader(inputStream);
            using var reader = new JsonTextReader(sr);

            var alreadyRead = 0;
            progress?.Report(alreadyRead);
            var lastProgress = 0;

            while (reader.Read())
            {
                // deserialize only when there's "{" character in the stream
                if (reader.TokenType == JsonToken.StartObject)
                {
                    var singleCard = serializer.Deserialize<ScryfallJsonCard>(reader);
                    if (singleCard != null)
                    {
                        result.Add(singleCard);
                    }
                }

                if (result.Count < batchSize)
                {
                    continue;
                }

                alreadyRead += result.Count;
                var progressStep = (int)Math.Round((alreadyRead / (decimal)_totalCardCount) * 100, 0, MidpointRounding.AwayFromZero);
                if (lastProgress != progressStep)
                {
                    lastProgress = progressStep;
                    progress?.Report(progressStep);
                }
                CardBatchDownloaded?.Invoke(this, new DownloadedCardsEventArgs(result));
                result.Clear();
            }

            if (_totalCardCount < alreadyRead)
            {
                _settingService.Set(_totalCountKey, alreadyRead);
                _totalCardCount = alreadyRead;
            }

            if (!result.Any())
            {
                return;
            }

            CardBatchDownloaded?.Invoke(this, new DownloadedCardsEventArgs(result));
            result.Clear();
        }
    }
}