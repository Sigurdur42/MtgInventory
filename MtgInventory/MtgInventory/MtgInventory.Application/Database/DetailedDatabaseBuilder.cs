using System;
using System.Collections.Generic;
using System.Globalization;
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
            var indexedSets = _database.MagicSets.FindAll().Where(s => !string.IsNullOrWhiteSpace(s.SetCodeMkm)).ToDictionary(s => s.SetCodeMkm);
            var setsToInsert = new List<DetailedSetInfo>();
            var setsToUpdate = new List<DetailedSetInfo>();

            Log.Debug("Rebuilding MKM Set data...");
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
                    indexedSets.Add(key, found);
                }

                found.UpdateFromMkm(key, mkm);
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
            var indexedSets = _database.MagicSets.FindAll().ToDictionary(s => s.SetCode);
            var setsToInsert = new List<DetailedSetInfo>();
            var setsToUpdate = new List<DetailedSetInfo>();

            Log.Debug("Rebuilding Scryfall Set data...");
            foreach (var scryfall in _database.ScryfallSets.FindAll())
            {
                if (scryfall.Code == null)
                {
                    continue;
                }

                var key = GetMainSetCodeForScryfall(scryfall.Code?.ToUpperInvariant() ?? "");
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
            var setCodePatched = setCode.ToUpperInvariant() ?? "";
            return setCodePatched switch
            {
                "CST" => "CSPTD",
                "MP2" => "AKHI",
                "PCMP" => "CPR",
                "PEMN" => "EMNP",
                "PGPX" => "GPPR",
                "GUL" => "PGRU",
                "IBS" => "ITP",
                "PXLN" => "XLNP",
                "MPS" => "KLDS",
                "AMH1" => "XMH1",
                "CON" => "CONF",
                "PALP" => "APL",
                "FBB" => "DTL",
                "4BB" => "4EBB",
                "PGRU" => "GUL",
                "HHO" => "HHPR",
                "PHJ" => "HJCC",
                "CEI" => "IED",
                "ITP" => "IBS",
                "MGB" => "MUL",
                "MB1" => "MYS",
                "CMB1" => "MYSPT",
                "PREL" => "REL",
                "REN" => "RE",
                "PRES" => "RSP",
                "RIN" => "RI",
                "PDSC" => "CC13",
                "PS14" => "CC14",
                "PS15" => "CC15",
                "PS16" => "CC16",
                "PS17" => "CC17",
                "PS18" => "CC18",
                "PS19" => "CC19",
                "PLIST" => "ZNL",
                "UGIN" => "UGFP",
                "PUMA" => "XUMA",
                "EXP" => "ZEX",
                "PWAR" => "XWAR",
                "PIKO" => "XICO",
                _ => setCode
            };
        }

        internal static string GetMainSetCodeForMkm(string setCode)
        {
            return setCode;
        }

        internal void RebuildMkmCardsForSet(string mkmSetCode)
        {
            if (string.IsNullOrWhiteSpace(mkmSetCode))
            {
                return;
            }

            var lastSets = _database.MagicSets.Query().Where(s => s.SetCodeMkm == mkmSetCode).ToArray();
            var lastSet = lastSets.FirstOrDefault();
            var allSetCodes = lastSets.Select(s => s.SetCode).ToArray();

            Log.Debug($"Rebuilding MKM card data for set code {mkmSetCode} {lastSet?.SetName} ({lastSets.Length} sets)...");

            var indexedCards = _database.MagicCards
                                        .Query()
                                        .Where(c => allSetCodes.Contains(c.SetCode))
                                        .ToArray()
                                        .GroupBy(c => c.NameEn)
                                        .ToDictionary(c => c.Key);

            var allMkmCards = _database.MkmProductInfo
                                       .Query()
                                       .Where(c => c.CategoryId == 1 && c.ExpansionCode == mkmSetCode)
                                       .OrderBy(c => c.MetacardId)
                                       .ToArray()
                                       .GroupBy(c => c.Name)
                                       .ToArray();

            var cardsToInsert = new List<DetailedMagicCard>();
            var cardsToUpdate = new List<DetailedMagicCard>();

            foreach (var mkm in allMkmCards)
                if (indexedCards.TryGetValue(mkm.Key, out var indexed))
                {
                    // Found this group - update all cards
                    if (indexed.Count() == mkm.Count())
                    {
                        // Easy path - card count matches. Simply update all
                        var i = 0;
                        foreach (var card in indexed.OrderBy(c => c.CollectorNumber))
                        {
                            card.UpdateFromMkm(mkm.ElementAt(i), lastSet);
                            i++;
                            cardsToUpdate.Add(card);
                        }
                    }
                    else
                    {
                        // Something is off here - card count mismatch
                        // TODO: Handle this specificly
                        Log.Debug($"MKM different card count {mkm.Key}_{mkmSetCode}: MKM: {mkm.Count()}, internal: {indexed.Count()}...");

                        var max = Math.Min(indexed.Count(), mkm.Count());

                        var ordered = indexed.OrderBy(c => c.CollectorNumber).ToArray();
                        for (var index = 0; index < max; ++index)
                        {
                            var card = ordered.ElementAt(index);
                            card.UpdateFromMkm(mkm.ElementAt(index), lastSet);
                            cardsToUpdate.Add(card);
                        }
                    }
                }
                else
                {
                    // The card does not exist - add it
                    var cards = mkm
                                .Select(
                                    c =>
                                    {
                                        var card = new DetailedMagicCard();
                                        card.UpdateFromMkm(c, lastSet);
                                        return card;
                                    })
                                .ToArray();

                    cardsToInsert.AddRange(cards);
                }

            _database.MagicCards.InsertBulk(cardsToInsert);
            _database.MagicCards.Update(cardsToUpdate);

            _database.EnsureMagicCardsIndex();
        }

        internal void RebuildScryfallCardsForSet(string scryfallSetId)
        {
            if (string.IsNullOrWhiteSpace(scryfallSetId))
            {
                return;
            }

            var lastSets = _database.MagicSets.Query().Where(s => s.SetCodeScryfall == scryfallSetId).ToArray();
            var lastSet = lastSets.FirstOrDefault() ?? new DetailedSetInfo();
            var allSetCodes = lastSets.Select(s => s.SetCode).ToArray();

            Log.Debug($"Rebuilding Scryfall card data for set code {scryfallSetId} {lastSet.SetName} ({lastSets.Length} sets)...");

            var indexedCards = _database.MagicCards
                                        .Query()
                                        .Where(c => allSetCodes.Contains(c.SetCode))
                                        .ToArray()
                                        .GroupBy(c => c.NameEn)
                                        .ToDictionary(c => c.Key);

            var allScryfallCards = _database.ScryfallCards
                                            .Query()
                                            .Where(c => c.Set == scryfallSetId)
                                            .OrderBy(c => c.CollectorNumber)
                                            .ToArray()
                                            .GroupBy(c => c.Name)
                                            .ToArray();

            var cardsToInsert = new List<DetailedMagicCard>();
            var cardsToUpdate = new List<DetailedMagicCard>();

            foreach (var scryfallPerSet in allScryfallCards)
            {
                var key = scryfallPerSet.Key;
                var remainingCardsOfSet = ResolveSpecialScryfallCard(
                        scryfallPerSet,
                        cardsToUpdate,
                        cardsToInsert,
                        lastSet)
                    .ToArray();

                if (!remainingCardsOfSet.Any())
                {
                    // All cards are already processed
                    continue;
                }

                if (indexedCards.TryGetValue(key, out var indexed))
                {
                    // Found this group - update all cards
                    if (indexed.Count() == remainingCardsOfSet.Count())
                    {
                        // Easy path - card count matches. Simply update all
                        var i = 0;
                        foreach (var card in indexed.OrderBy(c => c.CollectorNumber))
                        {
                            card.UpdateFromScryfall(remainingCardsOfSet.ElementAt(i), lastSet);
                            i++;
                            cardsToUpdate.Add(card);
                        }
                    }
                    else
                    {
                        // Something is off here - card count mismatch
                        // TODO: Handle this specificly
                        var debugData = string.Join(
                            Environment.NewLine,
                            remainingCardsOfSet.Select(s => $"'Id: {s.Id}' CollectorNr: '{s.CollectorNumber}' Set: '{s.Set}' Name: '{s.Name}'"));

                        var debugDataMkm = string.Join(
                            Environment.NewLine,
                            indexed.Select(s => $"MKM id: {s.MkmId}"));

                        Log.Debug($"Scryfall different card count {key}_{scryfallSetId}: Scryfall: {remainingCardsOfSet.Count()}, internal: {indexed.Count()}...{Environment.NewLine}{debugData}{Environment.NewLine}{debugDataMkm}");
                        var max = Math.Min(indexed.Count(), remainingCardsOfSet.Count());
                        var ordered = indexed.OrderBy(c => c.CollectorNumber).ToArray();
                        for (var index = 0; index < max; ++index)
                        {
                            var card = ordered.ElementAt(index);
                            card.UpdateFromScryfall(remainingCardsOfSet.ElementAt(index), lastSet);
                            cardsToUpdate.Add(card);
                        }
                    }
                }
                else
                {
                    // The card does not exist - add it
                    var cards = remainingCardsOfSet
                        .Select(
                            c =>
                            {
                                var card = new DetailedMagicCard();
                                card.UpdateFromScryfall(c, lastSet);
                                return card;
                            })
                        .ToArray();
                    cardsToInsert.AddRange(cards);
                }
            }

            _database.MagicCards.InsertBulk(cardsToInsert);
            _database.MagicCards.Update(cardsToUpdate);
            _database.EnsureMagicCardsIndex();
        }

        private static string GetCollectorNumber(string existingNumber)
        {
            var number = existingNumber
                .Replace("★", "0")
                .Replace("*", "0");

            if (int.TryParse(number, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            {
                return string.Format(CultureInfo.InvariantCulture, "{0:D6}", parsed);
            }

            Log.Warning($"Cannot convert collector number '{existingNumber}' to decimal.");
            return existingNumber;
        }

        private IEnumerable<ScryfallCard> ResolveSpecialScryfallCard(
            IEnumerable<ScryfallCard> cards,
            IList<DetailedMagicCard> cardsToUpdate,
            IList<DetailedMagicCard> cardsToInsert,
            DetailedSetInfo setInfo)
        {
            var result = new List<ScryfallCard>();

            var cardsCount = cards.Count();
            var index = 0;
            foreach (var card in cards.OrderBy(c => GetCollectorNumber(c.CollectorNumber)))
            {
                ++index;

                if (cardsCount == 2 && index == 2)
                {
                    // Handle one version alternate art here
                    var setCodeToSearch = $"X{card.Set}";
                    var found = _database.MagicCards
                        .Query()
                        .Where(c => c.NameEn == card.Name && c.SetCode == setCodeToSearch)
                        .FirstOrDefault();

                    if (found != null)
                    {
                        found.UpdateFromScryfall(card, null);
                        cardsToUpdate.Add(found);
                        continue;
                    }
                }

                // Commander oversized - Scryfall does not know oversized cards
                switch (card.Set)
                {
                    case "OC13":
                    case "OC14":
                    case "OC15":
                    case "OC16":
                    case "OC17":
                    case "OC18":
                    case "OC19":
                    case "OC20":
                        {
                            var found = _database.MagicCards.Query()
                                .Where(c => c.NameEn == card.Name && c.SetCode == card.Set)
                                .OrderByDescending(c => c.CollectorNumber)
                                .FirstOrDefault();

                            if (found != null)
                            {
                                found.UpdateFromScryfall(card, null);
                                cardsToUpdate.Add(found);
                                continue;
                            }
                        }
                        break;

                    case "PWAR":
                        if (card.CollectorNumber.Contains("S", StringComparison.InvariantCultureIgnoreCase)
                            || (card.CollectorNumber.Contains("★", StringComparison.InvariantCultureIgnoreCase)))
                    {
                            // This is japanese alt art. MKM has a differnt set for this
                            var found = _database?.MagicCards
                                ?.Query()
                                ?.Where(c => c.NameEn == card.Name && c.SetCodeMkm == "JWAR")
                                ?.OrderByDescending(c => c.CollectorNumber)
                                ?.FirstOrDefault();

                            if (found != null)
                            {
                                found.UpdateFromScryfall(card, null);
                                cardsToUpdate.Add(found);
                                continue;
                            }
                        }
                        break;
                }

                switch (card.Id.ToString().ToUpperInvariant())
                {
                    case "7240709D-A469-4801-82BD-F5DB50859763":
                        cardsToUpdate.Add(FindByMkmIdAndUpdate("9374", card, setInfo, false));
                        break;

                    case "73D28603-B116-4948-A46F-B95AE9118D9E":
                        cardsToUpdate.Add(FindByMkmIdAndUpdate("9374", card, setInfo, true));
                        break;

                    default:
                        // Use standard mapping for this card
                        result.Add(card);
                        break;
                }
            }

            return result;
        }

        private DetailedMagicCard FindByMkmIdAndUpdate(
            string mkmId,
            ScryfallCard card,
            DetailedSetInfo setInfo,
            bool resetMkmData)
        {
            var found = _database?.MagicCards?.Query().Where(c => c.MkmId == mkmId).FirstOrDefault() ?? new DetailedMagicCard();
            found.UpdateFromScryfall(card, setInfo);

            if (resetMkmData)
            {
                found.MkmId = "";
            }

            return found;
        }
    }
}