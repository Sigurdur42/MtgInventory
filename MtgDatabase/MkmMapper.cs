using System;
using System.Linq;
using System.Web;
using MtgDatabase.Models;

namespace MtgDatabase
{
    public interface IMkmMapper
    {
        string GenerateUrl(QueryableMagicCard card);
    }

    public class MkmMapper : IMkmMapper
    {
        private readonly Database.MtgDatabase _database;
        public static string UrlPrefix => "https://www.cardmarket.com/de/Magic/Products/Singles/";

        public MkmMapper(Database.MtgDatabase database)
        {
            _database = database;
        }
        public string GenerateUrl(QueryableMagicCard card)
        {
            // - name
            // - setName
            var setName = PatchSetName(card.SetName, card.Name);
            var cardName = PatchCardName(card.SetName, card.Name);

            if (card.IsBasicLand)
            {
                cardName = ResolveBasicLandCardName(card);
            }
            
            // TODO: some cards and sets have different names

            return HttpUtility.HtmlEncode($"{UrlPrefix}{setName}/{cardName}");
        }

        private string ResolveBasicLandCardName(QueryableMagicCard card)
        {
            var allOfThisTypeAndSet = _database.Cards?.Query()
                .Where(c => c.Name == card.Name && c.SetCode == card.SetCode && c.Language.Equals("EN", StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(c => c.CollectorNumber)
                .ToArray()
                ?? Array.Empty<QueryableMagicCard>();

            var version = 1 + allOfThisTypeAndSet
                .TakeWhile(cardOfSet => cardOfSet.CollectorNumber != card.CollectorNumber)
                .Count();

            return $"{card.Name}-V-{version}";
        }

        public static string PatchSetName(string setName, string cardName) =>
            setName
                .Replace("Shadows over Innistrad Tokens", "Shadows over Innistrad")
                .Replace("Magic Origins Tokens", "Magic Origins")
                .Replace("-", "")
                .Replace(":", "")
                .Replace("'", "-")
                .Replace(",", "")
                .Replace(" ", "-");

        public static string PatchCardName(string setName, string cardName) =>
            cardName
                .Replace("-", "")
                .Replace("'", "-")
                .Replace(",", "")
                .Replace(" ", "-");
    }
}