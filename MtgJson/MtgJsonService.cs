using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Ionic.Zip;
using Microsoft.Extensions.Logging;
using MoreLinq;
using MtgJson.CsvModels;
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

        public void DownloadAllPrintingsZip(
            FileInfo localFile,
            Func<CsvMeta, bool> headerLoaded,
            Func<CsvSet[], bool> setsLoaded,
            Func<IEnumerable<CsvCard>, bool> cardsLoaded,
            Func<IEnumerable<CsvForeignData>, bool> foreignDataLoaded,
            Func<IEnumerable<CsvLegalities>, bool> legalitiesLoaded)
        {
            using var zip = ZipFile.Read(localFile.FullName);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // NewLine = Environment.NewLine,
                HasHeaderRecord = true,
                Delimiter = ",",
                LineBreakInQuotedFieldIsBadData = false,
            };

            var headerRecord = ReadCsvMeta(zip, config);
            var shallContinue = headerLoaded?.Invoke(headerRecord) ?? true;
            if (!shallContinue)
            {
                return;
            }

            var sets = ReadCsvSet(zip, config);
            shallContinue = setsLoaded(sets);
            if (!shallContinue)
            {
                return;
            }

            if (!ReadCsvCards(zip, config, cardsLoaded))
            {
                return;
            }
            if (!ReadCsvForeign(zip, config, foreignDataLoaded))
            {
                return;
            }
            if (!ReadCsvLegalities(zip, config, legalitiesLoaded))
            {
                return;
            }
        }

        public async Task DownloadPriceDataAsync(
            FileInfo localFile,
            Func<JsonMeta, bool> headerLoaded,
            Action<IEnumerable<JsonCardPrice>> loadedBatch,
            MtgJsonPriceFilter priceFilter)
        {
            var stopwatch = Stopwatch.StartNew();
            await using var sr = File.OpenRead(localFile.FullName);
            ReadFromStreamByText(sr, 5000, headerLoaded, loadedBatch, priceFilter);

            stopwatch.Stop();
            _logger.LogInformation("DownloadPriceData took " + stopwatch.Elapsed);
        }

        public async Task DownloadPriceDataAsync(
            Func<JsonMeta, bool> headerLoaded,
            Action<IEnumerable<JsonCardPrice>> loadedBatch,
            MtgJsonPriceFilter priceFilter)
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
                        ReadFromStreamByText(stream, 5000, headerLoaded, loadedBatch, priceFilter);
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

        private JsonCardPrice ReadCardLevel(
            dynamic cardLevel,
            MtgJsonPriceFilter priceFilter)
        {
            var result = new JsonCardPrice
            {
                Id = Guid.Parse(cardLevel.Key),
            };

            foreach (var paperLevel in cardLevel.Value)
            {
                foreach (var sellerLevel in paperLevel.Value)
                {
                    var currency = sellerLevel.Value.currency;

                    switch (sellerLevel.Key)
                    {
                        case "cardmarket":
                            if (priceFilter.HideCardMarket)
                            {
                                continue;
                            }

                            break;

                        case "tcgplayer":
                            if (priceFilter.HideTcgPlayer)
                            {
                                continue;
                            }
                            break;

                        case "cardkingdom":
                            if (priceFilter.HideCardKingdom)
                            {
                                continue;
                            }
                            break;

                        case "cardhoarder":
                            if (priceFilter.HideCardHoarder)
                            {
                                continue;
                            }
                            break;

                        default:
                            _logger.LogWarning($"Unknown seller key {sellerLevel.Key}");
                            break;
                    }

                    foreach (var buyListOrRetail in sellerLevel.Value)
                    {
                        switch (buyListOrRetail.Key)
                        {
                            case "currency": continue;
                            case "buylist":
                                if (priceFilter.HideBuyList)
                                {
                                    continue;
                                }
                                break;
                        }

                        foreach (var foilOrNormal in buyListOrRetail.Value)
                        {
                            var priceRows = new List<JsonCardPriceItem>();
                            foreach (var priceRow in foilOrNormal.Value)
                            {
                                var row = new JsonCardPriceItem
                                {
                                    Type = paperLevel.Key,
                                    Currency = currency,
                                    Seller = sellerLevel.Key,
                                    BuylistOrRetail = buyListOrRetail.Key,
                                    IsFoil = foilOrNormal.Key,
                                    PaperOrOnline = paperLevel.Key,
                                    Date = priceRow.Key,
                                    Price = priceRow.Value,
                                };

                                priceRows.Add(row);
                            }

                            var filtered = priceRows
                                .OrderByDescending(p => p.Date)
                                .Take(priceFilter.HistoryDays)
                                .ToArray();
                            result.Items.AddRange(filtered);
                        }
                    }
                }
            }

            return result;
        }

        private bool ReadCsvCards(
            ZipFile zipFile,
            CsvConfiguration csvConfig,
            Func<IEnumerable<CsvCard>, bool> cardsLoaded)
        {
            var metaEntry = zipFile["cards.csv"];
            using var metaStream = new MemoryStream(new byte[metaEntry.UncompressedSize]);
            metaEntry.Extract(metaStream);
            metaStream.Seek(0, SeekOrigin.Begin);

            using var reader = new StreamReader(metaStream);
            using var csv = new CsvReader(reader, csvConfig);
            return cardsLoaded?.Invoke(csv.GetRecords<CsvCard>()) ?? false;
        }

        private bool ReadCsvForeign(
            ZipFile zipFile,
            CsvConfiguration csvConfig,
            Func<IEnumerable<CsvForeignData>, bool> foreignDataLoaded)
        {
            var metaEntry = zipFile["foreign_data.csv"];
            using var metaStream = new MemoryStream(new byte[metaEntry.UncompressedSize]);
            metaEntry.Extract(metaStream);
            metaStream.Seek(0, SeekOrigin.Begin);

            using var reader = new StreamReader(metaStream);
            using var csv = new CsvReader(reader, csvConfig);
            return foreignDataLoaded?.Invoke(csv.GetRecords<CsvForeignData>()) ?? false;
        }

        private bool ReadCsvLegalities(
            ZipFile zipFile,
            CsvConfiguration csvConfig,
            Func<IEnumerable<CsvLegalities>, bool> legalitiesLoaded)
        {
            var metaEntry = zipFile["legalities.csv"];
            using var metaStream = new MemoryStream(new byte[metaEntry.UncompressedSize]);
            metaEntry.Extract(metaStream);
            metaStream.Seek(0, SeekOrigin.Begin);

            using var reader = new StreamReader(metaStream);
            using var csv = new CsvReader(reader, csvConfig);
            return legalitiesLoaded?.Invoke(csv.GetRecords<CsvLegalities>()) ?? false;
        }

        private CsvMeta ReadCsvMeta(ZipFile zipFile, CsvConfiguration csvConfig)
        {
            var metaEntry = zipFile["meta.csv"];
            using var metaStream = new MemoryStream(new byte[metaEntry.UncompressedSize]);
            metaEntry.Extract(metaStream);
            metaStream.Seek(0, SeekOrigin.Begin);

            using var reader = new StreamReader(metaStream);
            using var csv = new CsvReader(reader, csvConfig);
            return csv.GetRecords<CsvMeta>().FirstOrDefault() ?? new CsvMeta();
        }

        private CsvSet[] ReadCsvSet(ZipFile zipFile, CsvConfiguration csvConfig)
        {
            var metaEntry = zipFile["sets.csv"];
            using var metaStream = new MemoryStream(new byte[metaEntry.UncompressedSize]);
            metaEntry.Extract(metaStream);
            metaStream.Seek(0, SeekOrigin.Begin);

            using var reader = new StreamReader(metaStream);
            using var csv = new CsvReader(reader, csvConfig);
            return csv.GetRecords<CsvSet>().ToArray();
        }

        /// <summary>
        /// Reads the block starting with "data": {
        /// </summary>
        private void ReadDataBlock(StreamReader reader,
            int batchSize,
            Action<IEnumerable<JsonCardPrice>> loadedBatch,
            MtgJsonPriceFilter priceFilter)
        {
            SkipUntilNextStart(reader);

            var readBlocks = new List<string>();

            // Read all and then analyze in threads
            var end = false;
            do
            {
                var completeBlock = ReadUntilMatchingEnd(reader, true).Trim(new[] { ',' });
                if (string.IsNullOrWhiteSpace(completeBlock))
                {
                    continue;
                }

                readBlocks.Add(completeBlock);

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

            // Now analyse in multiple threads:
            var analyzeAction = new Action<string>((completeBlock) =>
            {
                var deserializationInput = "{" + completeBlock + "}";
                dynamic? analyzedBlock = JsonConvert.DeserializeObject<ExpandoObject>(
                    deserializationInput,
                    new ExpandoObjectConverter());

                var readPrice = new List<JsonCardPrice>();

                if (analyzedBlock == null)
                {
                    return;
                }

                foreach (var level1 in analyzedBlock)
                {
                    readPrice.Add(ReadCardLevel(level1, priceFilter));
                }

                loadedBatch?.Invoke(readPrice);
            });

            readBlocks.AsParallel().ForAll(analyzeAction);
        }

        private void ReadFromStreamByText(
            Stream inputStream,
            int batchSize,
            Func<JsonMeta, bool> headerLoaded,
            Action<IEnumerable<JsonCardPrice>> loadedBatch,
            MtgJsonPriceFilter priceFilter)
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
                ReadDataBlock(reader, batchSize, loadedBatch, priceFilter);
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