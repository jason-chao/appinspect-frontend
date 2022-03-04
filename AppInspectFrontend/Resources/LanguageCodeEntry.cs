using System.Text.Json.Serialization;

namespace AppInspectFrontend.Resources
{
    public class LanguageCodeEntry
    {
        [JsonPropertyName("alpha2")]
        public string? Code { get; set; }

        [JsonPropertyName("English")]
        public string? Name { get; set; }
    }
}
