using MtgBinders.Domain.ValueObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MtgBinders.Domain.Entities
{
    public sealed class MtgWantListCard
    {
        public string CardId { get; set; }
        public int WantCount { get; set; }

        [JsonIgnore]
        public MtgFullCard FullCard { get; set; }
    }
}