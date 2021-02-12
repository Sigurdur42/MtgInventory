using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MtgJson.JsonModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MtgJson
{
    public class MtgJsonService : IMtgJsonService
    {
        private readonly ILogger<MtgJsonService> _logger;
        private readonly Regex _regexToken = new Regex("\"(?<token>[^\"]+)\"", RegexOptions.IgnorePatternWhitespace);

        public MtgJsonService(
            ILogger<MtgJsonService> logger)
        {
            _logger = logger;
        }

        public void DownloadPriceData(
            FileInfo localFile,
            Func<JsonMeta, bool> headerLoaded,
            Action<IEnumerable<JsonCardPrice>> loadedBatch)
        {
            var stopwatch = Stopwatch.StartNew();
            using var sr = File.OpenRead(localFile.FullName);
            ReadFromStreamByText(sr, 5000, headerLoaded, loadedBatch);

            stopwatch.Stop();
            _logger.LogInformation("DownloadPriceData took " + stopwatch.Elapsed);
        }

        public async Task DownloadPriceDataAsync(
            Func<JsonMeta, bool> headerLoaded,
            Action<IEnumerable<JsonCardPrice>> loadedBatch)
        {
            var stopwatch = Stopwatch.StartNew();

            const string remotePriceFile = @"https://mtgjson.com/api/v5/AllPrices.json";

            using (var client = new HttpClient())
            {
                using (var result = await client.GetAsync(remotePriceFile))
                {
                    if (result.IsSuccessStatusCode)
                    {
                        var stream = await result.Content.ReadAsStreamAsync();
                        ReadFromStreamByText(stream, 5000, headerLoaded, loadedBatch);
                    }
                }
            }

            stopwatch.Stop();
            _logger.LogInformation($"{nameof(DownloadPriceDataAsync)} took " + stopwatch.Elapsed);
        }

        private string[] ExtractToken(string input)
        {
            var result = new List<string>();
            var matches = _regexToken.Matches(input);
            foreach (Match match in matches)
            {
                result.Add(match.Groups["token"].Value);
            }

            return result.ToArray();
        }

        private JsonCardPrice ReadCardLevel(dynamic cardLevel)
        {
            var result = new JsonCardPrice
            {
                Id = cardLevel.Key,
            };

            foreach (var paperLevel in cardLevel.Value)
            {
                foreach (var sellerLevel in paperLevel.Value)
                {
                    var currency = sellerLevel.Value.currency;

                    foreach (var buylistOrRetail in sellerLevel.Value)
                    {
                        if (buylistOrRetail.Key == "currency")
                        {
                            continue;
                        }
                        foreach (var foilOrNormal in buylistOrRetail.Value)
                        {
                            foreach (var priceRow in foilOrNormal.Value)
                            {
                                var row = new JsonCardPriceItem
                                {
                                    Type = paperLevel.Key,
                                    Currency = currency,
                                    Seller = sellerLevel.Key,
                                };

                                row.BuylistOrRetail = buylistOrRetail.Key;
                                row.FoilOrNormal = foilOrNormal.Key;
                                row.PaperOrOnline = paperLevel.Key;
                                row.Date = priceRow.Key;
                                row.Price = priceRow.Value;

                                result.Items.Add(row);
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Reads the block starting with "data": {
        /// </summary>
        private void ReadDataBlock(StreamReader reader,
            int batchSize,
            Action<IEnumerable<JsonCardPrice>> loadedBatch)
        {
            var dataHeader = SkipUntilNextStart(reader);

            var readCardPrices = new List<JsonCardPrice>();

            var end = false;
            do
            {
                var completeBlock = ReadUntilMatchingEnd(reader, true).Trim(new[] { ',' });
                if (string.IsNullOrWhiteSpace(completeBlock))
                {
                    continue;
                }

                var deserializationInput = "{" + completeBlock + "}";
                dynamic? analyzedBlock = JsonConvert.DeserializeObject<ExpandoObject>(
                    deserializationInput,
                    new ExpandoObjectConverter());

                if (analyzedBlock == null)
                {
                    continue;
                }

                foreach (var level1 in analyzedBlock)
                {
                    readCardPrices.Add(ReadCardLevel(level1));
                }

                if (readCardPrices.Count >= batchSize)
                {
                    loadedBatch?.Invoke(readCardPrices);
                    readCardPrices.Clear();
                }

                var next = (char)reader.Peek();
                switch (next)
                {
                    case ',':
                    case '\0':
                        end = false;
                        break;

                    default:
                        end = true;
                        break;
                }
            }
            while (!end);

            if (readCardPrices.Any())
            {
                loadedBatch?.Invoke(readCardPrices);
            }
        }

        private void ReadFromStreamByText(
            Stream inputStream,
            int batchSize,
            Func<JsonMeta, bool> headerLoaded,
            Action<IEnumerable<JsonCardPrice>> loadedBatch)
        {
            var currentLine = new StringBuilder();

            using var reader = new StreamReader(inputStream);

            // skip root {
            reader.Read();

            // read meta
            var metaHeader = SkipUntilNextStart(reader);

            var metaLine = ReadUntilMatchingEnd(reader, false);
            var jsonMeta = JsonConvert.DeserializeObject<JsonMeta>("{" + metaLine + "}");

            var shallContinue = headerLoaded?.Invoke(jsonMeta) ?? true;
            if (shallContinue)
            {
                ReadDataBlock(reader, batchSize, loadedBatch);
            }
        }

        private string ReadUntilMatchingEnd(StreamReader reader, bool includesStart)
        {
            var currentLine = new StringBuilder();
            var done = false;

            var endCount = includesStart ? 0 : 1;
            do
            {
                if (reader.EndOfStream)
                {
                    done = true;
                    continue;
                }

                var buffer = (char)reader.Read();

                switch (buffer)
                {
                    case '{':
                        currentLine.Append(buffer);
                        endCount++;
                        break;

                    case '}':
                        endCount--;
                        done = endCount <= 0;
                        if (!done || includesStart)
                        {
                            currentLine.Append(buffer);
                        }
                        break;

                    default:
                        currentLine.Append(buffer);
                        break;
                }
            } while (!done);

            return currentLine.ToString();
        }

        private string SkipUntilNextComma(StreamReader reader)
        {
            return SkipUntilNextToken(reader, ',');
        }

        private string SkipUntilNextStart(StreamReader reader)
        {
            return SkipUntilNextToken(reader, '}');
        }

        private string SkipUntilNextToken(StreamReader reader, char token)
        {
            var currentLine = new StringBuilder();
            var done = false;

            do
            {
                if (reader.EndOfStream)
                {
                    done = true;
                    continue;
                }
                var buffer = (char)reader.Read();

                switch (buffer)
                {
                    case '{':
                        done = true;
                        break;

                    default:
                        currentLine.Append(buffer);
                        break;
                }
            } while (!done);

            return currentLine.ToString();
        }
    }
}