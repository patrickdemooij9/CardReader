using System.Text.Json.Serialization;

namespace CardReader.models
{
    public class SearchResponseImageModel
    {
        [JsonPropertyName("url")]
        public string ImageUrl { get; set; }
    }
}
