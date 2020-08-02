using Newtonsoft.Json;

namespace MtgBinders.Domain.ValueObjects
{
    public class MtgInventoryCard
    {
        public MtgInventoryCard(MtgFullCard card)
        {
            FullCard = card;
            CardId = card?.UniqueId;
            Condition = MtgCondition.NearMint;
        }

        [JsonConstructor]
        internal MtgInventoryCard()
        {
            // This is mainly used for deserialization
        }

        [JsonIgnore]
        public MtgFullCard FullCard { get; internal set; }

        [JsonProperty("CardId")]
        public string CardId { get; internal set; }

        [JsonProperty("Quantity")]
        public int Quantity { get; set; }

        [JsonProperty("IsFoil")]
        public bool IsFoil { get; set; }

        [JsonProperty("Condition")]
        public MtgCondition Condition { get; set; }

        [JsonProperty("LanguageCode")]
        public string LanguageCode { get; set; }
    }
}