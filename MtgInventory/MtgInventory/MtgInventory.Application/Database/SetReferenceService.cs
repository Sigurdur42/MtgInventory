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

        public SetReferenceService()
        {
            var raw = ReadEmbedded();
            _mkmIndexedSetData = raw.ToDictionary(s => s.MkmCode);
            _scryfallIndexedSetData = raw.ToDictionary(s => s.ScryfallCode);
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

        private SetReferenceData[] ReadEmbedded()
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