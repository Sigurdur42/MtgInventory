using System;
using System.Collections.Generic;
using System.Linq;
using MtgDatabase.Models;
using MtgJson.CsvModels;

namespace MtgDatabase.MtgJson
{
    internal class MtgJsonCardFactory
    {
        public Dictionary<Guid, CsvCard>? AllCards { get; set; }
        public IGrouping<Guid, CsvForeignData>[]? ForeignByCard { get; set; }
        public IGrouping<Guid, CsvLegalities>[]? LegalitiesByCard { get; set; }
        public CsvSet[]? LoadedSets { get; set; }

        public IList<QueryableMagicCard> CreateCards()
        {
            var result = new List<QueryableMagicCard>();
            if (AllCards == null || ForeignByCard == null || LegalitiesByCard == null || LoadedSets == null)
            {
                return result;
            }

            foreach (var sourceCard in AllCards)
            {
                var source = sourceCard.Value;
                var card = new QueryableMagicCard()
                {
                    Name = source.Name,
                    Id = source.Id,
                    CollectorNumber = source.number,
                    TypeLine = source.originalType,
                    SetCode = source.setCode,
                    OracleText = source.originalText,
                };

                result.Add(card);
                // TODO: Other properties
            }

            return result;
        }
    }
}