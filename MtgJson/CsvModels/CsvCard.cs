using System;
using System.Collections.Generic;
using System.Text;
using CsvHelper.Configuration.Attributes;

namespace MtgJson.CsvModels
{
    public class CsvCard
    {
        // 	,artist,asciiName,availability,borderColor,cardKingdomFoilId,cardKingdomId,colorIdentity,
        // colorIndicator,colors,convertedManaCost,duelDeck,edhrecRank,faceConvertedManaCost,faceName,flavorName,
        // flavorText,frameEffects,frameVersion,hand,hasAlternativeDeckLimit,hasContentWarning,hasFoil,hasNonFoil,
        // isAlternative,isFullArt,,,isPromo,isReprint,isReserved,isStarter,isStorySpotlight,isTextless,isTimeshifted,
        // keywords,layout,leadershipSkills,life,loyalty,manaCost,mcmId,mcmMetaId,mtgArenaId,mtgjsonV4Id,mtgoFoilId,mtgoId,
        // multiverseId,number,originalReleaseDate,originalText,originalType,otherFaceIds,power,printings,promoTypes,
        // purchaseUrls,rarity,scryfallId,scryfallIllustrationId,scryfallOracleId,setCode,side,subtypes,supertypes,
        // tcgplayerProductId,text,toughness,type,types,,variations,watermark
        [Name("uuid")]
        public Guid Id { get; set; } = Guid.Empty;

        [Name("isOnlineOnly")]
        public bool IsOnlineOnly { get; set; }

        [Name("isOversized")]
        public bool IsOversized { get; set; }

        [Name("name")] public string Name { get; set; } = "";
    }
}