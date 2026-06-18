using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CardReader.models
{
    public class AIConfigTypeModel
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("isDoubleSided")]
        public bool IsDoubleSided { get; set; }
        [JsonPropertyName("promptFile")]
        public string PromptFile { get; set; }
        [JsonPropertyName("backSidePromptFile")]
        public string? BackSidePromptFile { get; set; }
        [JsonPropertyName("properties")]
        public List<AIConfigPropertyModel> Properties { get; set; }
        [JsonPropertyName("backSideProperties")]
        public List<AIConfigPropertyModel>? BackSideProperties { get; set; }
    }
}
