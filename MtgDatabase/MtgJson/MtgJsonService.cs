using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MtgDatabase.MtgJson.JsonModels;
using MtgDatabase.Scryfall;
using Newtonsoft.Json;

namespace MtgDatabase.MtgJson
{
    public class MtgJsonService
    {

        public void DownloadPriceData(FileInfo localFile)
        {
            using var sr = File.OpenRead(localFile.FullName);
            ReadFromStreamByText(sr, 1000, null);

            ////var content = File.ReadAllText(localFile.FullName);
            ////dynamic config = JsonConvert.DeserializeObject<ExpandoObject>(content, new ExpandoObjectConverter());

            ////var metaVersion = config.meta.version;
            ////var metaDate = config.meta.date;

            ////foreach (dynamic dataSection in config.data)
            ////{

            ////}
            ////////var meta = new
            ////////{
            ////////    Date = "",
            ////////    Version = "",
            ////////};

            ////////var root = JsonConvert.DeserializeObject<Root>(content);
            ////int debug = 0;
        }

        private void ReadFromStreamByText(Stream inputStream, int batchSize, IProgress<int>? progress)
        {
            var currentLine = new StringBuilder();

            using var reader = new StreamReader(inputStream);

            var buffer = new Span<char>(new[] { ' ' });

            // skip root {

            reader.Read(buffer);

            // read meta
            var metaHeader = SkipUntilNextStart(reader);

            var metaLine = ReadUntilMatchingEnd(reader);
            var jsonMeta = JsonConvert.DeserializeObject<JsonMeta>("{"+metaLine+"}");
            Console.WriteLine(metaLine);

            ReadDataBlock(reader);

        }

        /// <summary>
        /// Reads the block starting with "data": {
        /// </summary>
        private void ReadDataBlock(StreamReader reader)
        {
            var dataHeader = SkipUntilNextStart(reader);
            ReadCardPriceBlock(reader);
        }

        /// <summary>
        /// Reads a block starting with the card UID
        /// </summary>
        private void ReadCardPriceBlock(StreamReader reader)
        {
            var cardPriceHeader = SkipUntilNextStart(reader);

            var uid = ExtractToken(cardPriceHeader).FirstOrDefault() ?? "";

            // TODO: Read UID
            var next = ' ';
            do
            {
                next = (char)reader.Peek();
                if (next != '"')
                {
                    continue;
                }

                // Read card type (paper, online, etc.)
                var cardTypeInput = SkipUntilNextStart(reader);
                var cardType = ExtractToken(cardTypeInput).FirstOrDefault() ?? "";
                ReadPriceByMarketPlace(reader);

            }
            while (next == '"');
        }

        private void ReadPriceByMarketPlace(StreamReader reader)
        {
            var marketPlaceInput = SkipUntilNextStart(reader);
            var marketPlace = ExtractToken(marketPlaceInput).FirstOrDefault() ?? "";

            var next = ' ';
            do
            {
                next = (char)reader.Peek();
                if (next != '"')
                {
                    continue;
                }

                ReadMarketPlaceBuyListOrRetailLevel(reader);

            }
            while (next == '"');

        }

        private void ReadMarketPlaceBuyListOrRetailLevel(StreamReader reader)
        {
            var levelInput = SkipUntilNextStart(reader);
            var level = ExtractToken(levelInput).FirstOrDefault() ?? "";

            switch (level)
            {
                case "buylist":
                    ReadPriceFoilOrNot(reader);
                    break;

                case "currency":
                    break;

                case "retail":
                    ReadPriceFoilOrNot(reader);
                    break;
            }
        }

        private void ReadPriceFoilOrNot(StreamReader reader)
        {
            var tokenInput = SkipUntilNextStart(reader);
            var token = ExtractToken(tokenInput).FirstOrDefault() ?? "";

            var next = ' ';
            do
            {
                next = (char)reader.Peek();
                if (next != '"')
                {
                    continue;
                }

                // ReadMarketPlaceBuyListOrRetailLevel(reader);

            }
            while (next == '"');
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

        private string ReadUntilMatchingEnd(StreamReader reader)
        {
            var currentLine = new StringBuilder();
            var done = false;
            var buffer = new Span<char>(new[] { ' ' });

            var endCount = 1;
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

                        endCount++;
                        break;


                    case '}':
                        endCount--;
                        done = endCount == 0;
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
