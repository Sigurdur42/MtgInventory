using MtgBinders.Domain.ValueObjects;
using MtgScryfall;
using System.Linq;

namespace MtgBinders.Domain.Scryfall
{
    internal class ScryfallService : IScryfallService
    {
        private IScryfallApi _scryfallApi;

        public ScryfallService(IScryfallApi api)
        {
            _scryfallApi = api;
        }

        public MagicSetInfo[] LoadAllSets()
        {
            var allSets = _scryfallApi.GetAllSets();
            if (!allSets.Success)
            {
                // TODO: actual error handling
                return new MagicSetInfo[0];
            }

            return allSets.DeserializeSetData().Select(s => new MagicSetInfo
            {
                SetCode = s.SetCode,
                IsDigitalOnly = s.IsDigitalOnly,
                SetName = s.SetName,
                SvgUrl = s.SvgUrl
            }).ToArray();
        }
    }
}