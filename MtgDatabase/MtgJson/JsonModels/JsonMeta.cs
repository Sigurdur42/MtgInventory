using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MtgDatabase.MtgJson.JsonModels
{
    public class JsonMeta
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; } = DateTime.MinValue;

        [JsonPropertyName("version")]
        public string Version { get; set; } = "";
    }
}
