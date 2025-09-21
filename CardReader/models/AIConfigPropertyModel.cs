using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CardReader.models
{
    public class AIConfigPropertyModel
    {
        [JsonPropertyName("alias")]
        public string Alias { get; set; }
        [JsonPropertyName("description")]
        public string Description { get; set; }
        [JsonPropertyName("toTitleCase")]
        public bool ToTitleCase { get; set; }
        [JsonPropertyName("split")]
        public string Split { get; set; }
    }
}
