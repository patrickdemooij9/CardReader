using System.Text.Json.Serialization;

namespace CardReader.models
{
    public class SearchResponseModel
    {
        [JsonPropertyName("items")]
        public SearchResponseItemModel[] Items { get; set; }
    }
}
