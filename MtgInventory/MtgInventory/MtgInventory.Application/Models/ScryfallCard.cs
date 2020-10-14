using System;
using System.Linq;
using ScryfallApi.Client.Models;

namespace MtgInventory.Service.Models
{
    public class ScryfallCard : Card
    {
        public ScryfallCard()
        {
        }

        public ScryfallCard(
            Card scryfallCard)
        {
            Id = scryfallCard.Id;
            MultiverseIds = scryfallCard.MultiverseIds;
            MtgoId = scryfallCard.MtgoId;
            MtgoFoilId = scryfallCard.MtgoFoilId;
            Uri = scryfallCard.Uri;
            ScryfallUri = scryfallCard.ScryfallUri;
            PrintsSearchUri = scryfallCard.PrintsSearchUri;
            RulingsUri = scryfallCard.RulingsUri;
            Name = scryfallCard.Name;
            Layout = scryfallCard.Layout;
            ConvertedManaCost = scryfallCard.ConvertedManaCost;
            TypeLine = scryfallCard.TypeLine;
            OracleText = scryfallCard.OracleText;
            ManaCost = scryfallCard.ManaCost;
            Power = scryfallCard.Power;
            Toughness = scryfallCard.Toughness;
            Loyalty = scryfallCard.Loyalty;
            LifeModifier = scryfallCard.LifeModifier;
            HandModifier = scryfallCard.HandModifier;
            Colors = scryfallCard.Colors;
            ColorIndicator = scryfallCard.ColorIndicator;
            ColorIdentity = scryfallCard.ColorIdentity;
            AllParts = scryfallCard.AllParts;
            CardFaces = scryfallCard.CardFaces;
            Legalities = scryfallCard.Legalities;
            Reserved = scryfallCard.Reserved;
            EdhrecRank = scryfallCard.EdhrecRank;
            Set = scryfallCard.Set?.ToUpperInvariant();
            SetName = scryfallCard.SetName;
            CollectorNumber = scryfallCard.CollectorNumber;
            SetSearchUri = scryfallCard.SetSearchUri;
            ScryfallSetUri = scryfallCard.ScryfallSetUri;
            ImageUris = scryfallCard.ImageUris;
            HasHighresImage = scryfallCard.HasHighresImage;
            Reprint = scryfallCard.Reprint;
            Digital = scryfallCard.Digital;
            Rarity = scryfallCard.Rarity;
            FlavorText = scryfallCard.FlavorText;
            Artist = scryfallCard.Artist;
            IllustrationId = scryfallCard.IllustrationId;
            Frame = scryfallCard.Frame;
            FullArt = scryfallCard.FullArt;
            Watermark = scryfallCard.Watermark;
            BorderColor = scryfallCard.BorderColor;
            StorySpotlightNumber = scryfallCard.StorySpotlightNumber;
            StorySpotlightUri = scryfallCard.StorySpotlightUri;
            Timeshifted = scryfallCard.Timeshifted;
            Colorshifted = scryfallCard.Colorshifted;
            Futureshifted = scryfallCard.Futureshifted;
            Price = scryfallCard.Price;
            RelatedUris = scryfallCard.RelatedUris;
            RetailerUris = scryfallCard.RetailerUris;
            Object = scryfallCard.Object;

            Images = scryfallCard.ImageUris.Select(i => new ImageLinkUri()
            {
                Category = i.Key,
                Uri = i.Value.ToString(),
            }).ToArray();

            UpdateDateUtc = DateTime.Now;
        }

        public ImageLinkUri[] Images { get; set; } = new ImageLinkUri[0];

        public DateTime UpdateDateUtc { get; set; }
    }
}