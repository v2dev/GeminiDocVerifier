using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace GeminiDocVerifier.Models
{
    public class GeminiResponse
    {
        [JsonProperty(PropertyName = "document_type")]
        public string DocumentType {  get; set; }

        [JsonProperty(PropertyName = "page_number")]
        public string PageNumber { get; set; }

        [JsonProperty(PropertyName = "fields")]
        public List<Fields> Fields { get; set; }
    }


    public class Fields
    {
        [JsonProperty(PropertyName = "field_name")]
        public string FieldName { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }
}
