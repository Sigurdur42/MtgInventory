using System;
using System.Text;

namespace ScryfallApiServices
{
    public class ScryfallConfiguration
    {
        public int UpdateSetCacheInDays { get; set; } = 14;
        public int UpdateCardCacheInDays { get; set; } = 28;
        
        public int UpdateCardCachePriceInDays { get; set; } = 2;

        public string DumpSettings()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"{nameof(UpdateSetCacheInDays)}: {UpdateSetCacheInDays}");
            builder.AppendLine($"{nameof(UpdateCardCacheInDays)}: {UpdateCardCacheInDays}");
            builder.AppendLine($"{nameof(UpdateCardCachePriceInDays)}: {UpdateCardCachePriceInDays}");
            return builder.ToString();
        }

        public bool IsSetOutdated(DateTime? setUpdateDate)
        {
            return DateTime.Now.AddDays(-1 * UpdateSetCacheInDays).Date > (setUpdateDate ?? DateTime.MinValue).Date;
        }
        
        public bool IsCardOutdated(DateTime? setUpdateDate)
        {
            return DateTime.Now.AddDays(-1 * UpdateCardCacheInDays).Date > (setUpdateDate ?? DateTime.MinValue).Date;
        }
        
        public bool IsCardPriceOutdated(DateTime? setUpdateDate)
        {
            return DateTime.Now.AddDays(-1 * UpdateCardCachePriceInDays).Date > (setUpdateDate ?? DateTime.MinValue).Date;
        }
    }
}