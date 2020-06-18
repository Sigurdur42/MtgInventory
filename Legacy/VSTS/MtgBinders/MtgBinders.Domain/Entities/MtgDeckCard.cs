using MtgBinders.Domain.ValueObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtgBinders.Domain.Entities
{
    public class MtgDeckCard
    {
        public int Quantity { get; set; }
        public string Name { get; set; }

        public string LanguageCode { get; set; }
        public MtgCondition Condition { get; set; }
        public bool IsFoil { get; set; }

        [JsonIgnore]
        public MtgFullCard FullCard { get; set; }
    }
}