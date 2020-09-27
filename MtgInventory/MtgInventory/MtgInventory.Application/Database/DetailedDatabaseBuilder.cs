using System.Collections.Generic;
using System.Linq;
using MtgInventory.Service.Models;
using Serilog;

namespace MtgInventory.Service.Database
{
    internal class DetailedDatabaseBuilder
    {
        private readonly CardDatabase _database;
        private readonly object _sync = new object();
        private bool _buildingInProgress;

        public DetailedDatabaseBuilder(
            CardDatabase database)
        {
            _database = database;
        }

        // TODO: Make thread safe

        public void BuildMkmSetData()
        {
            var indexedSets = _database.MagicSets.FindAll().ToDictionary(s => s.SetCodeMkm);
            var setsToInsert = new List<DetailedSetInfo>();
            var setsToUpdate = new List<DetailedSetInfo>();

            Log.Debug($"Rebuilding MKM Set data...");
            foreach (var mkm in _database.MkmExpansion.FindAll())
            {
                var key = GetMainSetCodeForMkm(mkm.Abbreviation.ToUpperInvariant());
                if (indexedSets.TryGetValue(key, out var found))
                {
                    setsToUpdate.Add(found);
                }
                else
                {
                    found = new DetailedSetInfo();
                    setsToInsert.Add(found);
                }

                found.UpdateFromMkm(key, mkm);
                indexedSets.Add(key, found);
            }

            if (setsToInsert.Any())
            {
                _database.MagicSets.InsertBulk(setsToInsert);
            }

            if (setsToUpdate.Any())
            {
                _database.MagicSets.Update(setsToUpdate);
            }

            _database.EnsureSetIndex();
        }

        public void BuildScryfallSetData()
        {
            var indexedSets = _database.MagicSets.FindAll().ToDictionary(s => s.SetCodeMkm);
            var setsToInsert = new List<DetailedSetInfo>();
            var setsToUpdate = new List<DetailedSetInfo>();

            Log.Debug($"Rebuilding Scryfall Set data...");
            foreach (var scryfall in _database.ScryfallSets.FindAll())
            {
                if (scryfall.Code == null)
                {
                    continue;
                }

                var key = GetMainSetCodeForScryfall(scryfall.Code?.ToUpperInvariant());
                if (!indexedSets.TryGetValue(key, out var found))
                {
                    found = new DetailedSetInfo();
                    indexedSets.Add(key, found);
                    setsToInsert.Add(found);
                }
                else
                {
                    setsToUpdate.Add(found);
                }

                found.UpdateFromScryfall(key, scryfall);
            }
            
            if (setsToInsert.Any())
            {
                _database.MagicSets.InsertBulk(setsToInsert);
            }

            if (setsToUpdate.Any())
            {
                _database.MagicSets.Update(setsToUpdate);
            }

            _database.EnsureSetIndex();
        }

        internal static string GetMainSetCodeForScryfall(string setCode)
        {
            switch (setCode?.ToUpperInvariant())
            {
                case "CST": return "CSPTD";
                case "MP2": return "AKHI";
                case "PCMP": return "CPR";
                case "PEMN": return "EMNP";
                case "PGPX": return "GPPR";
                case "GUL": return "PGRU";
                case "IBS": return "ITP";
                case "PXLN": return "XLNP";
                case "MPS": return "KLDS";
                case "AMH1": return "XMH1";

                // TODO: Handle oversized cards
                    // Commander oversized
                    ////case "OCMD": return "CMD";
                    ////case "OC13": return "C13";
                    ////case "OC14": return "C14";
                    ////case "OC15": return "C15";
                    ////case "OC16": return "C16";
                    ////case "OC17": return "C17";
                    ////case "OC18": return "C18";
                    ////case "OC19": return "C19";
            }
            return setCode;
        }

        internal static string GetMainSetCodeForMkm(string setCode)
        {
            return setCode;
        }

    }
}