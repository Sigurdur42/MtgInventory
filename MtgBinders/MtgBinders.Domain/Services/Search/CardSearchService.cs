using MoreLinq;
using MtgBinders.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtgBinders.Domain.Services
{
    internal class CardSearchService : ICardSearchService
    {
        private readonly IMtgDatabaseService _mtgDatabase;

        public CardSearchService(
            IMtgDatabaseService mtgDatabase)
        {
            _mtgDatabase = mtgDatabase;
        }

        public MtgFullCard[] Search(string searchPattern, CardSearchSettings settings)
        {
            var found = _mtgDatabase.CardData
                    .Where(c => c.Name.IndexOf(searchPattern, 0, StringComparison.InvariantCultureIgnoreCase) >= 0);

            if (!settings.ShowUniquePrints)
            {
                found = found.DistinctBy(c => c.Name);
            }

            return found.OrderBy(c => c.Name).ToArray();
        }

        // TODO: Continue
    }
}