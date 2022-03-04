using System.Text.Json.Serialization;

namespace AppInspectFrontend.Resources
{
    public class CountryCodeEntry
    {
        [JsonPropertyName("Code")]
        public string? Code { get; set; }

        [JsonPropertyName("Name")]
        public string? Name { get; set; }
    }
}
