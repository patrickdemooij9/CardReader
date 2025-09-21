using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CardReader.models
{
    public class AIConfigModel
    {
        [JsonPropertyName("initialPrompt")]
        public string InitialPrompt { get; set; }
        [JsonPropertyName("types")]
        public List<AIConfigTypeModel> Types { get; set; }
    }
}
