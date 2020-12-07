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
        public string GenerateUrl(QueryableMagicCard card)
        {
            // TODO: change to pass in all required data separate
            // - name
            // - setName
            var setName = PatchSetName(card.SetName, card.Name);
            var cardName = PatchCardName(card.SetName, card.Name);
        
            // TODO: some cards and sets have different names
            
            return HttpUtility.HtmlEncode($"https://www.cardmarket.com/de/Magic/Products/Singles/{setName}/{cardName}");
        }

        public static string PatchSetName(string setName, string cardName)
        {
            return setName
                .Replace("Shadows over Innistrad Tokens", "Shadows over Innistrad")
                .Replace("Magic Origins Tokens", "Magic Origins")    
                .Replace("-", "")
                .Replace(":", "")
                .Replace("'", "-")
                .Replace(",", "")
                .Replace(" ", "-")  ;
        }
        
        public static string PatchCardName(string setName, string cardName)
        {
            return cardName
                .Replace("-", "")
                .Replace("'", "-")
                .Replace(",", "")
                .Replace(" ", "-")  ;
        }
    }
}