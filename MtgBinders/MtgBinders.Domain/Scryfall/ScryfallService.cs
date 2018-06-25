using Microsoft.Extensions.Logging;
using MtgBinders.Domain.ValueObjects;
using MtgScryfall;
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
                SetCode = s.SetCode,
                IsDigitalOnly = s.IsDigitalOnly,
                IsFoilOnly = s.IsFoilOnly,
                SetName = s.SetName,
                SvgUrl = s.SvgUrl,
                NumberOfCards = s.NumberOfCards,
                ReleasDate = s.ReleaseDate,
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

            var result = loadResult.CardData.Select(c => new MtgFullCard
            {
                Name = c.Name,
                SetCode = c.SetCode,
                Rarity = c.Rarity.ToMtgRarity(_logger),

                ManaCost = c.ManaCost,
                ConvertedManaCost = c.ConvertedManaCost,
                TypeLine = c.TypeLine,
                OracleText = c.OracleText,
                CollectorNumber = c.CollectorNumber,
                IsDigitalOnly = c.IsDigitalOnly,
                Layout = c.Layout,

                ImageLarge = c.ImageLarge,
            }).ToArray();

            _logger?.LogInformation($"Loaded {result.Length} cards for set {setCode}.");

            return result;
        }
    }
}