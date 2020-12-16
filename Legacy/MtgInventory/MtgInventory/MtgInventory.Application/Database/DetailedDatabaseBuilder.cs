using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using MkmApi.Entities;
using MtgInventory.Service.Models;
using ScryfallApiServices.Models;

namespace MtgInventory.Service.Database
{
    internal class DetailedDatabaseBuilder
    {
        private readonly CardDatabase _database;
        private readonly ILogger<DetailedDatabaseBuilder> _logger;
        private readonly object _sync = new object();
        private readonly ReferenceDataService _referenceService = new ReferenceDataService();
        private bool _buildingInProgress;

        public DetailedDatabaseBuilder(
            CardDatabase database,
            ILoggerFactory loggerFactory)
        {
            _database = database;
            _logger = loggerFactory.CreateLogger<DetailedDatabaseBuilder>();
        }

        public void BuildMkmSetData()
        {
            var indexedSets = _database?.MagicSets?.FindAll()
                ?.Where(s => !string.IsNullOrWhiteSpace(s.SetCodeMkm))
                ?.ToDictionary(s => s.SetCodeMkm)
                ?? new Dictionary<string, DetailedSetInfo>();

            var setsToInsert = new List<DetailedSetInfo>();
            var setsToUpdate = new List<DetailedSetInfo>();

            // Log.Debug("Rebuilding MKM Set data...");
            foreach (var mkm in _database?.MkmExpansion?.FindAll() ?? new List<Expansion>())
            {
                var key = mkm.Abbreviation.ToUpperInvariant();
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
                found.IsKnownMkmOnlySet = _referenceService.IsMkmOnly(mkm.Abbreviation);
                UpdateSetMigrationStatus(found);
            }

            if (setsToInsert.Any())
            {
                _database?.MagicSets?.InsertBulk(setsToInsert);
            }

            if (setsToUpdate.Any())
            {
                _database?.MagicSets?.Update(setsToUpdate);
            }

            _database?.EnsureSetIndex();
        }

        public void BuildScryfallSetData()
        {
            var indexedSets = _database?.MagicSets?.FindAll()
                                  ?.ToDictionary(s => s.SetCode)
                              ?? new Dictionary<string, DetailedSetInfo>();

            var setsToInsert = new List<DetailedSetInfo>();
            var setsToUpdate = new List<DetailedSetInfo>();

            // Log.Debug("Rebuilding Scryfall Set data...");
            // foreach (var scryfall in _database.ScryfallSets.FindAll())
            // {
            //     if (scryfall.Code == null)
            //     {
            //         continue;
            //     }
            //
            //     var scryfallKey = scryfall.Code?.ToUpperInvariant() ?? "";
            //     var key = _referenceService.GetMkmSetCode(scryfallKey);
            //     if (string.IsNullOrEmpty(key))
            //     {
            //         key = scryfallKey;
            //     }
            //     if (!indexedSets.TryGetValue(key, out var found))
            //     {
            //         found = new DetailedSetInfo();
            //         indexedSets.Add(key, found);
            //         setsToInsert.Add(found);
            //     }
            //     else
            //     {
            //         setsToUpdate.Add(found);
            //     }
            //
            //     found.UpdateFromScryfall(key, scryfall);
            //     found.IsKnownScryfallOnlySet = _referenceService.IsScryfallOnly(scryfallKey);
            //
            //     UpdateSetMigrationStatus(found);
            // }

            if (setsToInsert.Any())
            {
                _database.MagicSets?.InsertBulk(setsToInsert);
            }

            if (setsToUpdate.Any())
            {
                _database.MagicSets?.Update(setsToUpdate);
            }

            _database.EnsureSetIndex();
        }

        internal void RebuildMkmCardsForSet(DetailedSetInfo lastSet)
        {
            _logger.LogDebug($"Rebuilding MKM card data for set code {lastSet.SetCodeMkm} {lastSet?.SetName}...");

            var indexedCards = _database?.MagicCards
                                        ?.Query()
                                        ?.Where(c => lastSet.SetCodeMkm == c.SetCode)
                                        ?.ToArray()
                                        ?.GroupBy(c => c.NameEn)
                                        ?.ToDictionary(c => c.Key)
                ?? new Dictionary<string, IGrouping<string, DetailedMagicCard>>();

            var allMkmCards = _database?.MkmProductInfo
                                       ?.Query()
                                       ?.Where(c => c.CategoryId == 1 && c.ExpansionCode == lastSet.SetCodeMkm)
                                       ?.OrderBy(c => c.MetacardId)
                                       ?.ToArray()
                                       ?.GroupBy(c => c.Name)
                                       ?.ToArray()
                ?? new IGrouping<string, MkmProductInfo>[0];

            lastSet.MkmCardCount = allMkmCards?.Length ?? 0;
            _database?.MagicSets?.Update(lastSet);

            var cardsToInsert = new List<DetailedMagicCard>();
            var cardsToUpdate = new List<DetailedMagicCard>();

            foreach (var mkm in allMkmCards)
            {
                // Resolve all manual mapped first:
                var manualMatched = new List<DetailedMagicCard>();
                foreach (var mkmManualMapped in mkm)
                {
                    var detailedCard = UpdateDetailsFromManualMapped(_referenceService.GetMappedCard(mkmManualMapped.Id));
                    if (detailedCard != null)
                    {
                        manualMatched.Add(detailedCard);
                        cardsToUpdate.Add(detailedCard);
                    }
                }

                if (manualMatched.Count == mkm.Count())
                {
                    // all cards have been manual matched - skip this section
                    _logger.LogTrace($"All {mkm.Key} have been manually mapped - skipping auto detection");
                    continue;
                }

                var manualMatchedIds = manualMatched.Select(c => c.MkmId).ToDictionary(c => c);

                if (indexedCards.TryGetValue(mkm.Key, out var indexed))
                {
                    var remainingCards = indexed
                        .Where(i => !manualMatchedIds.ContainsKey(i.MkmId))
                        .ToList();

                    // Found this group - update all cards
                    if (remainingCards.Count() == mkm.Count())
                    {
                        _logger.LogTrace($"{mkm.Key}: MKM and Scryfall card count are identical");
                        // Easy path - card count matches. Simply update all
                        var i = 0;
                        foreach (var card in remainingCards.OrderBy(c => c.CollectorNumber))
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
                        _logger.LogDebug($"MKM different card count {mkm.Key}_{lastSet.SetCodeMkm}: MKM: {mkm.Count()}, internal: {indexed.Count()}...");

                        var max = Math.Min(remainingCards.Count(), mkm.Count());

                        var ordered = remainingCards.OrderBy(c => c.CollectorNumber).ToArray();
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
                    _logger.LogTrace($"{mkm.Key}: Card does not exist in Scryfall");

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
            }

            _database?.MagicCards?.InsertBulk(cardsToInsert);
            _database?.MagicCards?.Update(cardsToUpdate);

            _database?.EnsureMagicCardsIndex();
        }

        internal void RebuildMkmCardsForSet(string mkmSetCode)
        {
            if (string.IsNullOrWhiteSpace(mkmSetCode))
            {
                return;
            }

            var lastSets = _database.MagicSets
                ?.Query()
                ?.Where(s => s.SetCodeMkm == mkmSetCode)
                ?.ToArray() ?? Array.Empty<DetailedSetInfo>();

            var lastSet = lastSets.First();

            RebuildMkmCardsForSet(lastSet);
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

            _logger.LogDebug($"Rebuilding Scryfall card data for set code {scryfallSetId} {lastSet.SetName} ({lastSets.Length} sets)...");

            var indexedCards = _database?.MagicCards
                                        ?.Query()
                                        ?.Where(c => allSetCodes.Contains(c.SetCode))
                                        ?.ToArray()
                                        ?.GroupBy(c => NormalizeName(c.NameEn))
                                        ?.ToDictionary(c => c.Key) ?? new Dictionary<string, IGrouping<string, DetailedMagicCard>>();

            // var allScryfallCards = _database.ScryfallCards
            //                                 .Query()
            //                                 .Where(c => c.Set == scryfallSetId)
            //                                 .OrderBy(c => c.CollectorNumber)
            //                                 .ToArray()
            //                                 .GroupBy(c => NormalizeName(c.Name))
            //                                 .ToArray();

            var cardsToInsert = new List<DetailedMagicCard>();
            var cardsToUpdate = new List<DetailedMagicCard>();

            // foreach (var scryfallPerSet in allScryfallCards)
            // {
            //     var key = scryfallPerSet.Key;
            //     var remainingCardsOfSet = ResolveSpecialScryfallCard(
            //             scryfallPerSet,
            //             cardsToUpdate,
            //             cardsToInsert,
            //             lastSet)
            //         .ToArray();
            //
            //     if (!remainingCardsOfSet.Any())
            //     {
            //         // All cards are already processed
            //         continue;
            //     }
            //
            //     if (indexedCards.TryGetValue(key, out var indexed))
            //     {
            //         // Found this group - update all cards
            //         if (indexed.Count() == remainingCardsOfSet.Count())
            //         {
            //             // Easy path - card count matches. Simply update all
            //             var i = 0;
            //             foreach (var card in indexed.OrderBy(c => c.CollectorNumber))
            //             {
            //                 card.UpdateFromScryfall(remainingCardsOfSet.ElementAt(i), lastSet);
            //                 i++;
            //                 cardsToUpdate.Add(card);
            //             }
            //         }
            //         else
            //         {
            //             // Something is off here - card count mismatch
            //             // TODO: Handle this specificly
            //             var debugData = string.Join(
            //                 Environment.NewLine,
            //                 remainingCardsOfSet.Select(s => $"'Id: {s.Id}' CollectorNr: '{s.CollectorNumber}' Set: '{s.Set}' Name: '{s.Name}'"));
            //
            //             var debugDataMkm = string.Join(
            //                 Environment.NewLine,
            //                 indexed.Select(s => $"MKM id: {s.MkmId}"));
            //
            //             _logger.LogDebug($"Scryfall different card count {key}_{scryfallSetId}: Scryfall: {remainingCardsOfSet.Count()}, internal: {indexed.Count()}...{Environment.NewLine}{debugData}{Environment.NewLine}{debugDataMkm}");
            //             var max = Math.Min(indexed.Count(), remainingCardsOfSet.Count());
            //             var ordered = indexed.OrderBy(c => c.CollectorNumber).ToArray();
            //             for (var index = 0; index < max; ++index)
            //             {
            //                 var card = ordered.ElementAt(index);
            //                 card.UpdateFromScryfall(remainingCardsOfSet.ElementAt(index), lastSet);
            //                 cardsToUpdate.Add(card);
            //             }
            //         }
            //     }
            //     else
            //     {
            //         foreach (var remainingCard in remainingCardsOfSet)
            //         {
            //             // This could be a double faced card. Scryfall has them only with
            //             // the front half registered
            //             var found = indexedCards
            //                 .Where(c => c.Key.StartsWith(remainingCard.Name, StringComparison.InvariantCultureIgnoreCase) && c.Key.Contains("/", StringComparison.InvariantCultureIgnoreCase))
            //                 .ToArray();
            //
            //             var haveAdded = false;
            //             if (found.Any())
            //             {
            //                 if (found.Length != 1)
            //                 {
            //                     // TODO: What do we do here?
            //                     _logger.LogWarning($"Unexpected card count found for card {remainingCard.Set} - {remainingCard.Name}: {found.Length}");
            //                 }
            //
            //                 var firstCard = found.First().Value;
            //                 if (firstCard.Count() != 1)
            //                 {
            //                     // TODO: What do we do here?
            //                     _logger.LogWarning($"Unexpected card count found for card {remainingCard.Set} - {remainingCard.Name}: {firstCard.Count()}");
            //                 }
            //                 else
            //                 {
            //                     var card = firstCard.First();
            //                     card.UpdateFromScryfall(remainingCard, lastSet);
            //                     cardsToUpdate.Add(card);
            //
            //                     haveAdded = true;
            //                 }
            //             }
            //
            //             if (!haveAdded)
            //             {
            //                 // The card does not exist - add it
            //                 var card = new DetailedMagicCard();
            //                 card.UpdateFromScryfall(remainingCard, lastSet);
            //                 cardsToInsert.Add(card);
            //             }
            //         }
            //     }
            // }

            _database.MagicCards.InsertBulk(cardsToInsert);
            _database.MagicCards.Update(cardsToUpdate);
            _database.EnsureMagicCardsIndex();
        }

        private static string NormalizeName(string name)
        {
            return name
                       ?.Replace("Æ", "Ae")
                       ?.Replace("//", "/")
                       ?? "";
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

            return existingNumber;
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

        private DetailedMagicCard? UpdateDetailsFromManualMapped(
            CardReferenceData? manualMapped)
        {
            if (manualMapped != null)
            {
                var found = _database?.MagicCards
                    ?.Query()
                    ?.Where(c => c.MkmId == manualMapped.MkmId)
                    ?.FirstOrDefault();

                if (found == null)
                {
                    _logger.LogError($"Cannot find manual mapped reference card with mkm id {manualMapped.MkmId}");
                }
                else
                {
                    found.UpdateManualMapped(manualMapped);
                }

                return found;
            }
            else
            {
                return null;
            }
        }

        private IEnumerable<ScryfallCard> ResolveSpecialScryfallCard(
            IEnumerable<ScryfallCard> cards,
            IList<DetailedMagicCard> cardsToUpdate,
            IList<DetailedMagicCard> cardsToInsert,
            DetailedSetInfo setInfo)
        {
            var result = new List<ScryfallCard>();

            var cardsArray = cards?.ToArray() ?? new ScryfallCard[0];

            var cardsCount = cardsArray.Length;
            var index = 0;
            foreach (var card in cardsArray.OrderBy(c => GetCollectorNumber(c.CollectorNumber)))
            {
                ++index;

                // Look for already mapped
                var alreadyManualMapped = _database?.MagicCards
                    ?.Query()
                    // ?.Where(c => c.IsMappedByReferenceCard)
                    ?.Where(c => c.ScryfallId == card.Id)
                    ?.FirstOrDefault();
                if (alreadyManualMapped?.IsMappedByReferenceCard ?? false)
                {
                    // This card already has a manual mapping
                    // Just update here
                    alreadyManualMapped.UpdateFromScryfall(card, setInfo);
                    continue;
                }

                // Look for manual mapped cards here
                var manualMapped = _referenceService.GetMappedCard(card.CollectorNumber, card.Set);
                if (manualMapped != null)
                {
                    var found = _database?.MagicCards
                        ?.Query()
                        ?.Where(c => c.MkmId == manualMapped.MkmId)
                        ?.FirstOrDefault();

                    if (found == null)
                    {
                        // Log.Error($"Cannot find manual mapped reference card with mkm id {manualMapped.MkmId}");
                    }
                    else
                    {
                        found.UpdateFromScryfall(card, setInfo);
                        found.UpdateManualMapped(manualMapped);
                        cardsToUpdate.Add(found);
                        continue;
                    }
                }

                // TODO: Add special handling for double faced cards here

                if (cardsCount == 2 && index == 2)
                {
                    // Handle one version alternate art here
                    var setCodeToSearch = $"X{card.Set}";
                    var found = _database?.MagicCards
                        ?.Query()
                        ?.Where(c => c.NameEn == card.Name && c.SetCode == setCodeToSearch)
                        ?.FirstOrDefault();

                    if (found != null)
                    {
                        found.UpdateFromScryfall(card, null);
                        cardsToUpdate.Add(found);
                        continue;
                    }
                }

                result.Add(card);
            }

            return result;
        }

        private void UpdateSetMigrationStatus(DetailedSetInfo set)
        {
            if (set.IsKnownMkmOnlySet)
            {
                set.MigrationStatus = SetMigrationStatus.Migrated;
                return;
            }

            var scryfallCode = _referenceService.GetMkmSetCode(set.SetCodeScryfall);
            if (!string.IsNullOrEmpty(scryfallCode))
            {
                set.MigrationStatus = SetMigrationStatus.Migrated;
                return;
            }

            var mkmCode = _referenceService.GetScryfallSetCode(set.SetCodeMkm);
            if (!string.IsNullOrEmpty(mkmCode))
            {
                set.MigrationStatus = SetMigrationStatus.Migrated;
                return;
            }

            set.MigrationStatus = SetMigrationStatus.Unknown;
        }
    }
}