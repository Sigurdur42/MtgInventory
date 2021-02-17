using System;
using System.Collections.Generic;
using System.Linq;
using MtgJson.CsvModels;

namespace MtgJson.Sqlite.Models
{
    internal class DbCardFactory
    {
        public Dictionary<Guid, CsvCard>? AllCards { get; set; }
        public IGrouping<Guid, CsvForeignData>[]? ForeignByCard { get; set; }
        public IGrouping<Guid, CsvLegalities>[]? LegalitiesByCard { get; set; }
        public CsvSet[]? LoadedSets { get; set; }

        public IList<DbCard> CreateCards()
        {
            var result = new List<DbCard>();
            if (AllCards == null || ForeignByCard == null || LegalitiesByCard == null || LoadedSets == null)
            {
                return result;
            }

            foreach (var sourceCard in AllCards)
            {
                var source = sourceCard.Value;
                var card = new DbCard()
                {
                    Name = source.Name,
                    CollectorNumber = source.number,
                    TypeLine = source.originalType,
                    SetCode = source.setCode,
                    OracleText = source.originalText,
                    ScryfallId = source.scryfallId,
                    CardMarketId = source.mcmId,
                    Uuid = source.Id.ToString(),
                };

                result.Add(card);
                // TODO: Other properties
            }

            return result;
        }
    }
}