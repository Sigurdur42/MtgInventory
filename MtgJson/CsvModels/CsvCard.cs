using System;
using System.Collections.Generic;
using System.Text;
using CsvHelper.Configuration.Attributes;

namespace MtgJson.CsvModels
{
    public class CsvCard
    {
        public string artist { get; set; } = "";

        public string asciiName { get; set; } = "";

        public string availability { get; set; } = "";

        public string borderColor { get; set; } = "";

        public string cardKingdomFoilId { get; set; } = "";

        public string cardKingdomId { get; set; } = "";

        public string colorIdentity { get; set; } = "";

        public string colorIndicator { get; set; } = "";

        public string colors { get; set; } = "";

        public string convertedManaCost { get; set; } = "";

        public string duelDeck { get; set; } = "";

        public string edhrecRank { get; set; } = "";

        public string faceConvertedManaCost { get; set; } = "";

        public string faceName { get; set; } = "";

        public string flavorName { get; set; } = "";

        public string flavorText { get; set; } = "";

        public string frameEffects { get; set; } = "";

        public string frameVersion { get; set; } = "";

        public string hand { get; set; } = "";

        public string hasAlternativeDeckLimit { get; set; } = "";

        public string hasContentWarning { get; set; } = "";

        public string hasFoil { get; set; } = "";

        public string hasNonFoil { get; set; } = "";

        [Name("uuid")]
        public Guid Id { get; set; } = Guid.Empty;

        public string isAlternative { get; set; } = "";

        public string isFullArt { get; set; } = "";

        [Name("isOnlineOnly")]
        public bool IsOnlineOnly { get; set; }

        [Name("isOversized")]
        public bool IsOversized { get; set; }

        public string isPromo { get; set; } = "";
        public string isReprint { get; set; } = "";
        public string isReserved { get; set; } = "";
        public string isStarter { get; set; } = "";
        public string isStorySpotlight { get; set; } = "";
        public string isTextless { get; set; } = "";
        public string isTimeshifted { get; set; } = "";

        public string

 keywords

        { get; set; } = "";

        public string layout { get; set; } = "";
        public string leadershipSkills { get; set; } = "";
        public string life { get; set; } = "";
        public string loyalty { get; set; } = "";
        public string manaCost { get; set; } = "";
        public string mcmId { get; set; } = "";
        public string mcmMetaId { get; set; } = "";
        public string mtgArenaId { get; set; } = "";
        public string mtgjsonV4Id { get; set; } = "";
        public string mtgoFoilId { get; set; } = "";
        public string mtgoId { get; set; } = "";

        public string multiverseId { get; set; } = "";

        [Name("name")] public string Name { get; set; } = "";

        public string number { get; set; } = "";
        public string originalReleaseDate { get; set; } = "";
        public string originalText { get; set; } = "";
        public string originalType { get; set; } = "";
        public string otherFaceIds { get; set; } = ""; public string power { get; set; } = ""; public string printings { get; set; } = ""; public string promoTypes { get; set; } = ""; public string

              purchaseUrls

        { get; set; } = ""; public string rarity { get; set; } = ""; public string scryfallId { get; set; } = ""; public string scryfallIllustrationId { get; set; } = ""; public string scryfallOracleId { get; set; } = ""; public string setCode { get; set; } = ""; public string side { get; set; } = ""; public string subtypes { get; set; } = ""; public string supertypes { get; set; } = ""; public string

              tcgplayerProductId
        { get; set; } = ""; public string text { get; set; } = ""; public string toughness { get; set; } = ""; public string type { get; set; } = "";

        public string types { get; set; } = "";
        public string variations { get; set; } = "";
        public string watermark { get; set; } = "";
    }
}