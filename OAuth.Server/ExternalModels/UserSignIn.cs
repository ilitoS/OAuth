using System.Text.Json.Serialization;

namespace OAuth.Server.ExternalModels
{
    public class UserSignIn
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
