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
        [JsonPropertyName("promptFile")]
        public string PromptFile { get; set; }
        [JsonPropertyName("properties")]
        public List<AIConfigPropertyModel> Properties { get; set; }
    }
}
