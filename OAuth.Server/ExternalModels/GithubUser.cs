using System.Text.Json.Serialization;

namespace OAuth.Server.ExternalModels
{
    public class GithubUser
    {
        [JsonPropertyName("login")]
        public string Username { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }
    }
}
