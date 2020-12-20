using System.CodeDom.Compiler;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MtgDatabase.Scryfall
{
#pragma warning disable CS8618

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    [GeneratedCode("https://json2csharp.com", "1.0")]
    public class ImageUris
    {
        [JsonProperty("small")] public string Small { get; set; }

        [JsonProperty("normal")] public string Normal { get; set; }

        [JsonProperty("large")] public string Large { get; set; }

        [JsonProperty("png")] public string Png { get; set; }

        [JsonProperty("art_crop")] public string ArtCrop { get; set; }

        [JsonProperty("border_crop")] public string BorderCrop { get; set; }
    }

    [GeneratedCode("https://json2csharp.com", "1.0")]
    public class Legalities
    {
        [JsonProperty("standard")] public string Standard { get; set; }

        [JsonProperty("future")] public string Future { get; set; }

        [JsonProperty("historic")] public string Historic { get; set; }

        [JsonProperty("pioneer")] public string Pioneer { get; set; }

        [JsonProperty("modern")] public string Modern { get; set; }

        [JsonProperty("legacy")] public string Legacy { get; set; }

        [JsonProperty("pauper")] public string Pauper { get; set; }

        [JsonProperty("vintage")] public string Vintage { get; set; }

        [JsonProperty("penny")] public string Penny { get; set; }

        [JsonProperty("commander")] public string Commander { get; set; }

        [JsonProperty("brawl")] public string Brawl { get; set; }

        [JsonProperty("duel")] public string Duel { get; set; }

        [JsonProperty("oldschool")] public string Oldschool { get; set; }
    }

    [GeneratedCode("https://json2csharp.com", "1.0")]
    public class Prices
    {
        [JsonProperty("usd")] public object Usd { get; set; }

        [JsonProperty("usd_foil")] public object UsdFoil { get; set; }

        [JsonProperty("eur")] public object Eur { get; set; }

        [JsonProperty("eur_foil")] public object EurFoil { get; set; }

        [JsonProperty("tix")] public object Tix { get; set; }
    }

    [GeneratedCode("https://json2csharp.com", "1.0")]
    public class RelatedUris
    {
        [JsonProperty("gatherer")] public string Gatherer { get; set; }

        [JsonProperty("tcgplayer_decks")] public string TcgplayerDecks { get; set; }

        [JsonProperty("edhrec")] public string Edhrec { get; set; }

        [JsonProperty("mtgtop8")] public string Mtgtop8 { get; set; }
    }

    [GeneratedCode("https://json2csharp.com", "1.0")]
    public class ScryfallJsonCard
    {
        [JsonProperty("object")] public string Object { get; set; }

        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("oracle_id")] public string OracleId { get; set; }

        [JsonProperty("multiverse_ids")] public List<int> MultiverseIds { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("printed_name")] public string PrintedName { get; set; }

        [JsonProperty("lang")] public string Lang { get; set; }

        [JsonProperty("released_at")] public string ReleasedAt { get; set; }

        [JsonProperty("uri")] public string Uri { get; set; }

        [JsonProperty("scryfall_uri")] public string ScryfallUri { get; set; }

        [JsonProperty("layout")] public string Layout { get; set; }

        [JsonProperty("highres_image")] public bool HighresImage { get; set; }

        [JsonProperty("image_uris")] public ImageUris ImageUris { get; set; }

        [JsonProperty("mana_cost")] public string ManaCost { get; set; }

        [JsonProperty("cmc")] public double Cmc { get; set; }

        [JsonProperty("type_line")] public string TypeLine { get; set; }

        [JsonProperty("printed_type_line")] public string PrintedTypeLine { get; set; }

        [JsonProperty("oracle_text")] public string OracleText { get; set; }

        [JsonProperty("printed_text")] public string PrintedText { get; set; }

        [JsonProperty("power")] public string Power { get; set; }

        [JsonProperty("toughness")] public string Toughness { get; set; }

        [JsonProperty("colors")] public List<string> Colors { get; set; }

        [JsonProperty("color_identity")] public List<string> ColorIdentity { get; set; }

        [JsonProperty("keywords")] public List<string> Keywords { get; set; }

        [JsonProperty("legalities")] public Legalities Legalities { get; set; }

        [JsonProperty("games")] public List<string> Games { get; set; }

        [JsonProperty("reserved")] public bool Reserved { get; set; }

        [JsonProperty("foil")] public bool Foil { get; set; }

        [JsonProperty("nonfoil")] public bool Nonfoil { get; set; }

        [JsonProperty("oversized")] public bool Oversized { get; set; }

        [JsonProperty("promo")] public bool Promo { get; set; }

        [JsonProperty("reprint")] public bool Reprint { get; set; }

        [JsonProperty("variation")] public bool Variation { get; set; }

        [JsonProperty("set")] public string Set { get; set; }

        [JsonProperty("set_name")] public string SetName { get; set; }

        [JsonProperty("set_type")] public string SetType { get; set; }

        [JsonProperty("set_uri")] public string SetUri { get; set; }

        [JsonProperty("set_search_uri")] public string SetSearchUri { get; set; }

        [JsonProperty("scryfall_set_uri")] public string ScryfallSetUri { get; set; }

        [JsonProperty("rulings_uri")] public string RulingsUri { get; set; }

        [JsonProperty("prints_search_uri")] public string PrintsSearchUri { get; set; }

        [JsonProperty("collector_number")] public string CollectorNumber { get; set; }

        [JsonProperty("digital")] public bool Digital { get; set; }

        [JsonProperty("rarity")] public string Rarity { get; set; }

        [JsonProperty("flavor_text")] public string FlavorText { get; set; }

        [JsonProperty("card_back_id")] public string CardBackId { get; set; }

        [JsonProperty("artist")] public string Artist { get; set; }

        [JsonProperty("artist_ids")] public List<string> ArtistIds { get; set; }

        [JsonProperty("illustration_id")] public string IllustrationId { get; set; }

        [JsonProperty("border_color")] public string BorderColor { get; set; }

        [JsonProperty("frame")] public string Frame { get; set; }

        [JsonProperty("full_art")] public bool FullArt { get; set; }

        [JsonProperty("textless")] public bool Textless { get; set; }

        [JsonProperty("booster")] public bool Booster { get; set; }

        [JsonProperty("story_spotlight")] public bool StorySpotlight { get; set; }

        [JsonProperty("edhrec_rank")] public int EdhrecRank { get; set; }

        [JsonProperty("prices")] public Prices Prices { get; set; }

        [JsonProperty("related_uris")] public RelatedUris RelatedUris { get; set; }
    }

#pragma warning restore CS8618
}