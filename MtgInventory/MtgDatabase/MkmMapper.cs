using System.Web;

namespace MtgDatabase
{
    public interface IMkmMapper
    {
        string GenerateUrl(FoundMagicCard card);
    }

    public class MkmMapper : IMkmMapper
    {
        public string GenerateUrl(FoundMagicCard card)
        {
            // TODO: change to pass in all required data separate
            // - name
            // - setName
            var setName = PatchSetName(card.PrintInfo.SetName);
            var cardName = PatchCardName(card.Card.Name);
        
            // TODO: some cards and sets have different names
            
            return HttpUtility.HtmlEncode($"https://www.cardmarket.com/de/Magic/Products/Singles/{setName}/{cardName}");
        }

        public static string PatchSetName(string cardName)
        {
            return cardName
                .Replace("Shadows over Innistrad Tokens", "Shadows over Innistrad")
                .Replace("Magic Origins Tokens", "Magic Origins")    
                .Replace("-", "")
                .Replace(":", "")
                .Replace("'", "-")
                .Replace(",", "")
                .Replace(" ", "-")  ;
        }
        
        public static string PatchCardName(string cardName)
        {
            return cardName
                .Replace("-", "")
                .Replace("'", "-")
                .Replace(",", "")
                .Replace(" ", "-")  ;
        }
    }
}