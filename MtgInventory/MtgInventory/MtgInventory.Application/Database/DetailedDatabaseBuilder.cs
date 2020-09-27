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

            Log.Debug($"Rebuilding MKM Set data...");
            foreach (var mkm in _database.MkmExpansion.FindAll())
            {
                var key = mkm.Abbreviation.ToUpperInvariant();
                if (indexedSets.TryGetValue(key, out var found))
                {
                    //// Log.Warning($"Duplicate MKM set found: {mkm}");
                }
                else
                {
                    found = new DetailedSetInfo();
                    setsToInsert.Add(found);
                }

                found.UpdateFromMkm(mkm);
                indexedSets.Add(key, found);
            }

            if (setsToInsert.Any())
            {
                _database.MagicSets.InsertBulk(setsToInsert);
            }

            _database.EnsureSetIndex();
        }

        public void BuildScryfallSetData()
        {
            var indexedSets = _database.MagicSets.FindAll().ToDictionary(s => s.SetCodeMkm);
            var setsToInsert = new List<DetailedSetInfo>();
            
            Log.Debug($"Rebuilding Scryfall Set data...");
            foreach (var scryfall in _database.ScryfallSets.FindAll())
            {
                if (scryfall.Code == null)
                {
                    continue;
                }

                var key = scryfall.Code?.ToUpperInvariant();
                if (!indexedSets.TryGetValue(key, out var found))
                {
                    found = new DetailedSetInfo();
                    indexedSets.Add(key, found);
                    setsToInsert.Add(found);
                }

                found.UpdateFromScryfall(scryfall);
            }
            
            if (setsToInsert.Any())
            {
                _database.MagicSets.InsertBulk(setsToInsert);
            }

            _database.EnsureSetIndex();
        }
    }
}