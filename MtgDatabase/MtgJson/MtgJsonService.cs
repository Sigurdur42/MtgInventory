using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MtgDatabase.MtgJson.JsonModels;
using MtgDatabase.Scryfall;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MtgDatabase.MtgJson
{
    public class MtgJsonService
    {

        public void DownloadPriceData(FileInfo localFile, Action<IEnumerable<JsonCardPrice>> loadedBatch)
        {
            var stopwatch = Stopwatch.StartNew();
            using var sr = File.OpenRead(localFile.FullName);
            ReadFromStreamByText(sr, 1000, loadedBatch);


            stopwatch.Stop();
            Console.WriteLine("Took " + stopwatch.Elapsed);
        }

        private void ReadFromStreamByText(
            Stream inputStream,
            int batchSize,
            Action<IEnumerable<JsonCardPrice>> loadedBatch)
        {
            var currentLine = new StringBuilder();

            using var reader = new StreamReader(inputStream);

            var buffer = new Span<char>(new[] { ' ' });

            // skip root {

            reader.Read(buffer);

            // read meta
            var metaHeader = SkipUntilNextStart(reader);

            var metaLine = ReadUntilMatchingEnd(reader, false);
            var jsonMeta = JsonConvert.DeserializeObject<JsonMeta>("{" + metaLine + "}");
            Console.WriteLine(metaLine);

            ReadDataBlock(reader, batchSize, loadedBatch);

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

                dynamic analysedBlock = JsonConvert.DeserializeObject<ExpandoObject>(
                    "{" + completeBlock + "}",
                    new ExpandoObjectConverter());


                foreach (var level1 in analysedBlock)
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





        private Regex _regexToken = new Regex("\"(?<token>[^\"]+)\"", RegexOptions.IgnorePatternWhitespace);
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

        private string SkipUntilNextStart(StreamReader reader)
        {
            return SkipUntilNextToken(reader, '}');
        }

        private string SkipUntilNextComma(StreamReader reader)
        {
            return SkipUntilNextToken(reader, ',');
        }


        private string SkipUntilNextToken(StreamReader reader, char token)
        {
            var currentLine = new StringBuilder();
            var done = false;
            var buffer = new Span<char>(new[] { token });

            do
            {
                var count = reader.Read(buffer);
                if (count == 0)
                {
                    done = true;
                    continue;

                }

                switch (buffer[0])
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

        private string ReadUntilMatchingEnd(StreamReader reader, bool includesStart)
        {
            var currentLine = new StringBuilder();
            var done = false;
            var buffer = new Span<char>(new[] { ' ' });

            var endCount = includesStart ? 0 : 1;
            do
            {
                var count = reader.Read(buffer);
                if (count == 0)
                {
                    done = true;
                    continue;

                }

                switch (buffer[0])
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


                    var s = reader.ToString();
                    var s2 = reader.Value;
                    var dummy = 0;

                    ////var singleCard = serializer.Deserialize<ScryfallJsonCard>(reader);
                    ////if (singleCard != null)
                    ////{
                    ////    result.Add(singleCard);
                    ////}
                }

                if (result.Count < batchSize)
                {
                    continue;
                }

                //alreadyRead += result.Count;
                //var progressStep = (int)Math.Round((alreadyRead / (decimal)_totalCardCount) * 100, 0, MidpointRounding.AwayFromZero);
                //if (lastProgress != progressStep)
                //{
                //    lastProgress = progressStep;
                //    progress?.Report(progressStep);
                //}
                // CardBatchDownloaded?.Invoke(this, new DownloadedCardsEventArgs(result));
                result.Clear();
            }

            //if (_totalCardCount < alreadyRead)
            //{
            //    _settingService.Set(_totalCountKey, alreadyRead);
            //    _totalCardCount = alreadyRead;
            //}

            if (!result.Any())
            {
                return;
            }

            //  CardBatchDownloaded?.Invoke(this, new DownloadedCardsEventArgs(result));
            result.Clear();
        }

    }
}
