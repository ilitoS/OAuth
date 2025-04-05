using System.Text.Json.Serialization;

namespace OAuth.Server.ExternalModels
{
    public class GithubEmail
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("verified")]
        public bool Verified { get; set; }

        [JsonPropertyName("primary")]
        public bool Primary { get; set; }
    }
}
