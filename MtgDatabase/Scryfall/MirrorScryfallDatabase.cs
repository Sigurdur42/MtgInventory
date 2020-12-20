using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MtgDatabase.Scryfall
{
    public interface IMirrorScryfallDatabase
    {
        Task DownloadDatabase(int batchSize);
        event EventHandler<DownloadedCardsEventArgs> CardBatchDownloaded;
    }

    public class MirrorScryfallDatabase : IMirrorScryfallDatabase
    {
        private readonly ILogger<MirrorScryfallDatabase> _logger;

        public MirrorScryfallDatabase(ILogger<MirrorScryfallDatabase> logger)
        {
            _logger = logger;
        }

        public event EventHandler<DownloadedCardsEventArgs> CardBatchDownloaded = (sender, args) => { };

        public async Task DownloadDatabase(int batchSize)
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
                ReadFromStream(downloadStream, batchSize);
            }

            stopwatch.Stop();
            _logger.LogTrace($"Downloading all data took {stopwatch.Elapsed} ...");
        }

        private void ReadFromStream(Stream inputStream, int batchSize)
        {
            var result = new List<ScryfallJsonCard>();
            var serializer = new JsonSerializer();

            using var sr = new StreamReader(inputStream);
            using var reader = new JsonTextReader(sr);

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

                CardBatchDownloaded?.Invoke(this, new DownloadedCardsEventArgs(result));
                result.Clear();
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