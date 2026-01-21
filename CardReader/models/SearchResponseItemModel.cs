using System.Text.Json.Serialization;

namespace CardReader.models
{
    public class SearchResponseItemModel
    {
        [JsonPropertyName("baseId")]
        public int BaseId { get; set; }

        [JsonPropertyName("variantId")]
        public int VariantId { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("imageUrl")]
        public SearchResponseImageModel ImageUrl { get; set; }

        [JsonPropertyName("attributes")]
        public Dictionary<string, string[]> Attributes { get; set; }
    }
}
