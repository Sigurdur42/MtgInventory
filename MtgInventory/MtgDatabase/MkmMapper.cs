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
        public static string UrlPrefix => "https://www.cardmarket.com/de/Magic/Products/Singles/";

        public string GenerateUrl(QueryableMagicCard card)
        {
            // - name
            // - setName
            var setName = PatchSetName(card.SetName, card.Name);
            var cardName = PatchCardName(card.SetName, card.Name);

            // TODO: some cards and sets have different names

            return HttpUtility.HtmlEncode($"{UrlPrefix}{setName}/{cardName}");
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