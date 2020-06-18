using System;
using System.Globalization;
using System.Linq;
using MtgDomain;
using ScryfallApi.Client.Models;

namespace MtgBinder.Domain.Database
{
    public static class CardInfoExtension
    {
        public static CardInfo ToCardInfo(this Card c)
        {
            var uniqueId = $"{c.Set?.ToUpperInvariant()}/{c.CollectorNumber}/{c.Name}/{c.Id}";

            return new CardInfo()
            {
                Id = uniqueId,
                ScryfallId = c.Id,
                Name = c.Name,
                SetCode = c.Set,
                Cmc = c.ConvertedManaCost,
                OracleText = c.OracleText,
                TypeLine = c.TypeLine,
                ManaCost = c.ManaCost,
                ColorIdentity = c.ColorIdentity,
                Rarity = c.Rarity,
                Updated = DateTime.Now,
                IsReserverd = c.Reserved,
                CollectorNumber = c.CollectorNumber,

                Legalities = c.Legalities?.Select(i => new CardLegality()
                {
                    Format = i.Key,
                    Legalitiy = i.Value,
                }).ToArray() ?? new CardLegality[0],

                ImageUrls = c.ImageUris?.Select(i => new ImageUrl()
                {
                    Key = i.Key,
                    Url = i.Value,
                })?.ToArray() ?? new ImageUrl[0],
            };
        }

        public static CardPrice ToCardPrice(this Card c)
        {
            return new CardPrice()
            {
                DateUtc = DateTime.Now.Date,
                DateTimeLookup = DateTime.Now.Date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture),
                ScryfallId = c.Id,
                Usd = c.Price.Usd,
                UsdFoil = c.Price.UsdFoil,
                Eur = c.Price.Eur,
                EurFoil = c.Price.EurFoil,
                Tix = c.Price.Tix
            };
        }
    }
}