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
            _logger = loggerFactory?.CreateLogger<ScryfallService>();
        }

        public MagicSetInfo[] LoadAllSets()
        {
            var allSets = _scryfallApi.GetAllSets();
            if (!allSets.Success)
            {
                // TODO: actual error handling
                return new MagicSetInfo[0];
            }

            var deserialzed = allSets.DeserializeSetData();
            _logger?.LogDebug($"{nameof(LoadAllSets)} - Loaded {deserialzed.Length} sets");

            return deserialzed.Select(s => new MagicSetInfo
            {
                SetCode = s.SetCode,
                IsDigitalOnly = s.IsDigitalOnly,
                SetName = s.SetName,
                SvgUrl = s.SvgUrl
            }).ToArray();
        }
    }
}