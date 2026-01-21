using System.Text.Json.Serialization;

namespace CardReader.models
{
    public class PresetVariantModel
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("properties")]
        public Dictionary<string, string> Properties { get; set; }
    }
}
