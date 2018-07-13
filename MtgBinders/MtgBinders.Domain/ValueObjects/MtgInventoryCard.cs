using Newtonsoft.Json;

namespace MtgBinders.Domain.ValueObjects
{
    public class MtgInventoryCard
    {
        public MtgInventoryCard(MtgFullCard card)
        {
            FullCard = card;
            CardId = card.UniqueId;
            Condition = MtgCondition.NearMint;
        }

        internal MtgInventoryCard()
        {
            // This is mainly used for deserialization
        }

        [JsonIgnore]
        public MtgFullCard FullCard { get; internal set; }

        public string CardId { get; internal set; }

        public int Quantity { get; set; }

        public bool IsFoil { get; set; }

        public MtgCondition Condition { get; set; }

        public string LanguageCode { get; set; }
    }
}