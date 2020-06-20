using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MtgDomain
{
    public static class CardInfoExtension
    {
        public static Uri GetNormalImage(this CardInfo card)
        {
            return card.ImageUrls.FirstOrDefault(i => i.Key == "normal")?.Url ?? card?.ImageUrls.FirstOrDefault()?.Url;
        }
    }
}
