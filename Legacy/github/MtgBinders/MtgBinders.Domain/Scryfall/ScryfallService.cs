using Microsoft.Extensions.Logging;
using MtgBinders.Domain.ValueObjects;
using MtgScryfall;
using MtgScryfall.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MtgBinders.Domain.Scryfall
{
    internal class ScryfallService : IScryfallService
    {
        private readonly ILogger _logger;
        private readonly IScryfallApi _scryfallApi;

        public ScryfallService(
            ILoggerFactory loggerFactory,
            IScryfallApi api)
        {
            _scryfallApi = api;
            _logger = loggerFactory?.CreateLogger(nameof(ScryfallService));
        }

        public MtgSetInfo[] LoadAllSets()
        {
            var allSets = _scryfallApi.GetAllSets();
            if (!allSets.Success)
            {
                // TODO: actual error handling
                return new MtgSetInfo[0];
            }

            var deserialzed = allSets.DeserializeSetData();
            _logger?.LogDebug($"{nameof(LoadAllSets)} - Loaded {deserialzed.Length} sets");

            return deserialzed.Select(s => new MtgSetInfo
            {
                SetCode = s.SetCode.ToUpperInvariant(),
                IsDigitalOnly = s.IsDigitalOnly,
                IsFoilOnly = s.IsFoilOnly,
                SetName = s.SetName,
                SvgUrl = s.SvgUrl,
                NumberOfCards = s.NumberOfCards,
                ReleaseDate = s.ReleaseDate,
                ParentSetCode = s.ParentSetData,
            }).ToArray();
        }

        public MtgFullCard[] LoadCardsOfSet(string setCode)
        {
            _logger?.LogDebug($"Loading cards for set {setCode}...");
            var loadResult = _scryfallApi.GetCardsBySet(setCode);
            if (!loadResult.Success)
            {
                _logger?.LogError($"Load cards for set {setCode} failed with status code {loadResult.StatusCode}");
                return new MtgFullCard[0];
            }

            var result = loadResult.CardData.Select(c => CreateCardFromResult(c, _logger)).ToArray();

            _logger?.LogInformation($"Loaded {result.Length} cards for set {setCode}.");

            return result;
        }

        public MtgFullCard LoadCardByScryfallId(string scryfallId)
        {
            var loadResult = _scryfallApi.GetCardByScryfallId(scryfallId);
            if (!loadResult.Success)
            {
                _logger?.LogError($"Load cards for id {scryfallId} failed with status code {loadResult.StatusCode}");
                return null;
            }

            var result = loadResult.CardData.Select(c => CreateCardFromResult(c, _logger)).First();

            return result;
        }

        public MtgFullCard[] LoadAllCards()
        {
            var result = new List<MtgFullCard>();
            CardDataRequestResult loadResult;

            var remainingPages = 0;
            var page = 1;
            do
            {
                var totalDump = remainingPages <= 0 ? "---" : remainingPages.ToString();
                _logger?.LogDebug($"Loading all cards (page {page}, remaining {totalDump})...");

                loadResult = _scryfallApi.GetCardsByPage(page);
                if (!loadResult.Success)
                {
                    _logger?.LogError($"Load all cards  failed with status code {loadResult.StatusCode}");
                    return result.ToArray();
                }

                var thisPage = loadResult.CardData.Select(c => CreateCardFromResult(c, _logger)).ToArray();

                var remainingCards = loadResult.TotalCards - result.Count;
                remainingPages = remainingCards <= 0 ? 0 : (remainingCards / thisPage.Length) + 1;

                result.AddRange(thisPage);

                page += 1;
            } while (loadResult.HasMoreData);

            _logger?.LogInformation($"Loaded {result.Count} cards.");

            return result.ToArray();
        }

        internal static decimal? ConvertToDecimal(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (decimal.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }

            return null;
        }

        private static MtgFullCard CreateCardFromResult(CardData c, ILogger logger)
        {
            return new MtgFullCard
            {
                UniqueId = c.UniqueId,
                Name = c.Name,
                SetCode = c.SetCode.ToUpperInvariant(),
                Rarity = c.Rarity.ToMtgRarity(logger),

                ManaCost = c.ManaCost,
                ConvertedManaCost = c.ConvertedManaCost,
                TypeLine = c.TypeLine,
                OracleText = c.OracleText,
                CollectorNumber = c.CollectorNumber,
                IsDigitalOnly = c.IsDigitalOnly,
                Layout = c.Layout,

                IsPauperLegal = c.IsPauperLegal,
                IsCommanderLegal = c.IsCommanderLegal,
                IsStandardLegal = c.IsStandardLegal,
                IsModernLegal = c.IsModernLegal,
                IsLegacyLegal = c.IsLegacyLegal,
                IsVintageLegal = c.IsVintageLegal,

                ImageLarge = c.ImageLarge,
                MkmLink = c.MkmLink,
                ScryfallLink = c.ScryfallLink,
                GathererLink = c.GathererLink,
                PriceUsd = ConvertToDecimal(c.PriceUsd),
                PriceTix = ConvertToDecimal(c.PriceTix),
                PriceEur = ConvertToDecimal(c.PriceEur),

                LastUpdate = DateTime.UtcNow,
            };
        }
    }
}