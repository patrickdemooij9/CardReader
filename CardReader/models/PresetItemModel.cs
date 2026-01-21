using System.Text.Json.Serialization;

namespace CardReader.models
{
    public class PresetItemModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("variants")]
        public PresetVariantModel[] Variants { get; set; }
    }
}
