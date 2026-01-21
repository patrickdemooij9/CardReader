using System.Text.Json.Serialization;

namespace CardReader.models
{
    public class SearchPostModel
    {
        [JsonPropertyName("query")]
        public string Query { get; set; }

        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; } = 1;

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; } = 10;

        [JsonPropertyName("variantTypeId")]
        public int VariantTypeId { get; set; } = 0;
    }
}
