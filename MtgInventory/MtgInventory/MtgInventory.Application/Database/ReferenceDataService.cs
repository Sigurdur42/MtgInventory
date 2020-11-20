using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using MoreLinq;

namespace MtgInventory.Service.Database
{
    internal class ReferenceDataService
    {
        private readonly Dictionary<string, CardReferenceData> _cardReferenceDataByMkmId;
        private readonly Dictionary<string, CardReferenceData> _cardReferenceDataByScryfall;
        private readonly Dictionary<string, SetReferenceData> _mkmIndexedSetData;
        private readonly string[] _mkmOnly;
        private readonly Dictionary<string, SetReferenceData> _scryfallIndexedSetData;
        private readonly string[] _scryfallOnly;

        public ReferenceDataService()
        {
            var rawSetData = ReadEmbeddedSetReferenceData();
            _mkmIndexedSetData = rawSetData.DistinctBy(s => s.MkmCode).ToDictionary(s => s.MkmCode);
            _scryfallIndexedSetData = rawSetData.DistinctBy(s => s.ScryfallCode).ToDictionary(s => s.ScryfallCode);

            _mkmOnly = ReadEmbeddedMkmOnly("SetReferenceMkm.txt");
            _scryfallOnly = ReadEmbeddedMkmOnly("SetReferenceScryfall.txt");

            var rawCardData = ReadEmbeddedCardReferenceData();
            _cardReferenceDataByMkmId = rawCardData.ToDictionary(c => c.MkmId);
            _cardReferenceDataByScryfall = rawCardData.ToDictionary(c => c.GetScryfallIndexKey());
        }

        public CardReferenceData? GetMappedCard(
            string collectorNumber,
            string scryfallSetCode)
        {
            var key = CardReferenceData.MakeScryfallKey(collectorNumber, scryfallSetCode);
            if (_cardReferenceDataByScryfall.TryGetValue(key, out var found))
            {
                return found;
            }

            return null;
        }

        public CardReferenceData? GetMappedCard(
            string mkmId)
        {
            if (_cardReferenceDataByMkmId.TryGetValue(mkmId, out var found))
            {
                return found;
            }

            return null;
        }

        public string GetMkmSetCode(string scryfallSetCode)
        {
            if (!string.IsNullOrEmpty(scryfallSetCode)
                && _scryfallIndexedSetData.TryGetValue(scryfallSetCode, out var found))
            {
                return found.MkmCode;
            }

            return "";
        }

        public string GetScryfallSetCode(string mkmSetCode)
        {
            if (!string.IsNullOrEmpty(mkmSetCode)
                && _mkmIndexedSetData.TryGetValue(mkmSetCode, out var found))
            {
                return found.ScryfallCode;
            }

            return "";
        }

        public bool IsMkmOnly(string mkmSetCode)
        {
            return _mkmOnly.Any(c => c.Equals(mkmSetCode, StringComparison.InvariantCultureIgnoreCase));
        }

        public bool IsScryfallOnly(string scryfallSetCode)
        {
            return _scryfallOnly.Any(c => c.Equals(scryfallSetCode, StringComparison.InvariantCultureIgnoreCase));
        }

        public void WriteCardReferenceData(
            IEnumerable<CardReferenceData> cardReference,
            string targetFile)
        {
            try
            {
                using TextWriter writer = new StreamWriter(targetFile, false, System.Text.Encoding.UTF8);
                var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ",",
                    Encoding = Encoding.UTF8,
                    HasHeaderRecord = true,
                    ShouldQuote = (value, context) =>
                    {
                        if (string.IsNullOrEmpty(value))
                        {
                            return false;
                        }

                        if (value.StartsWith("http", StringComparison.InvariantCultureIgnoreCase)
                        || value.Contains(" ", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return true;
                        }

                        return false;
                    }
                };

                using var csv = new CsvWriter(writer, configuration);
                csv.WriteRecords(cardReference);
                csv.Flush();
            }
            catch (Exception error)
            {
                // Log.Error($"Cannot load card reference data: {error}");
            }
        }

        private CardReferenceData[] ReadEmbeddedCardReferenceData()
        {
            var resourceLoader = new ResourceLoader();
            var fileList = resourceLoader.GetEmbeddedResourceString(
                GetType().Assembly,
                "_CardReferenceDataFileList.txt")
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .ToArray();

            var referenceData = new List<CardReferenceData>();
            foreach (var file in fileList)
            {
                try
                {
                    var yaml = resourceLoader.GetEmbeddedResourceString(
                        GetType().Assembly,
                        file);

                    using var stringReader = new StringReader(yaml);
                    using var csv = new CsvReader(stringReader, CultureInfo.InvariantCulture);

                    csv.Configuration.HasHeaderRecord = true;
                    csv.Configuration.Delimiter = ",";
                    csv.Configuration.BadDataFound = (context) => { };

                    referenceData.AddRange(csv.GetRecords<CardReferenceData>().ToArray());
                }
                catch (Exception error)
                {
                    // Log.Error($"Cannot load card reference data: {error}");
                    // return new CardReferenceData[0];
                }
            }

            return referenceData.ToArray();
        }

        private string[] ReadEmbeddedMkmOnly(string resourceName)
        {
            try
            {
                var resourceLoader = new ResourceLoader();
                var yaml = resourceLoader.GetEmbeddedResourceString(
                    GetType().Assembly,
                    resourceName);

                return yaml.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => i.Trim())
                    .ToArray();
            }
            catch (Exception error)
            {
                // Log.Error($"Cannot load mkm only reference data: {error}");
                return new string[0];
            }
        }

        private SetReferenceData[] ReadEmbeddedSetReferenceData()
        {
            try
            {
                var resourceLoader = new ResourceLoader();
                var yaml = resourceLoader.GetEmbeddedResourceString(
                    GetType().Assembly,
                    "SetReferenceData.csv");

                using var stringReader = new StringReader(yaml);
                using var csv = new CsvReader(stringReader, CultureInfo.InvariantCulture);

                csv.Configuration.HasHeaderRecord = true;
                csv.Configuration.Delimiter = ",";
                csv.Configuration.BadDataFound = (context) => { };

                return csv.GetRecords<SetReferenceData>().ToArray();
            }
            catch (Exception error)
            {
                // Log.Error($"Cannot load set reference data: {error}");
                return new SetReferenceData[0];
            }
        }
    }
}