using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CsvHelper;
using Serilog;

namespace MtgInventory.Service.Database
{
    internal class SetReferenceService
    {
        private readonly Dictionary<string, SetReferenceData> _mkmIndexedSetData;
        private readonly Dictionary<string, SetReferenceData> _scryfallIndexedSetData;
        private readonly string[] _mkmOnly;
        private readonly string[] _scryfallOnly;

        public SetReferenceService()
        {
            var raw = ReadEmbeddedSetReferenceData();
            _mkmIndexedSetData = raw.ToDictionary(s => s.MkmCode);
            _scryfallIndexedSetData = raw.ToDictionary(s => s.ScryfallCode);

            _mkmOnly = ReadEmbeddedMkmOnly("SetReferenceMkm.txt");
            _scryfallOnly = ReadEmbeddedMkmOnly("SetReferenceScryfall.txt");
        }

        public bool IsMkmOnly(string mkmSetCode)
        {
            return _mkmOnly.Any(c => c.Equals(mkmSetCode, StringComparison.InvariantCultureIgnoreCase));
        }

        public bool IsScryfallOnly(string scryfallSetCode)
        {
            return _scryfallOnly.Any(c => c.Equals(scryfallSetCode, StringComparison.InvariantCultureIgnoreCase));
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
                Log.Error($"Cannot load mkm only reference data: {error}");
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
                Log.Error($"Cannot load set reference data: {error}");
                return new SetReferenceData[0];
            }
        }
    }
}