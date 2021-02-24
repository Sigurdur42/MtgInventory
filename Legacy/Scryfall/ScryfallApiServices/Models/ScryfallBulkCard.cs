using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ScryfallApiServices.Models
{
    public class ScryfallBulkCard
    {
        [JsonPropertyName("id")] public Guid Id { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; } = "";

        [JsonPropertyName("printed_name")] public string PrintedName { get; set; } = "";

        [JsonPropertyName("lang")] public string Language { get; set; } = "";

        [JsonPropertyName("set")] public string Set { get; set; } = "";

        [JsonPropertyName("set_name")] public string SetName { get; set; } = "";

        [JsonPropertyName("collector_number")] public string CollectorNumber { get; set; } = "";

        [JsonPropertyName("type_line")] public string TypeLine { get; set; } = "";

        [JsonPropertyName("printed_type_line")]
        public string PrintedTypeLine { get; set; } = "";

        [JsonPropertyName("oracle_text")] public string OracleText { get; set; } = "";

        public DateTime? released_at { get; set; }

        [JsonPropertyName("mana_cost")] public string ManaCost { get; set; } = "";

        [JsonPropertyName("legalities")] public Dictionary<string, string> Legalities { get; set; } = new Dictionary<string, string>();

        [JsonPropertyName("reserved")] public bool Reserved { get; set; }

        [JsonPropertyName("image_uris")] public Dictionary<string, Uri> ImageUris { get; set; } = new Dictionary<string, Uri>();

        [JsonPropertyName("digital")] public bool Digital { get; set; }

        [JsonPropertyName("rarity")] public string Rarity { get; set; } = "";
    }
}