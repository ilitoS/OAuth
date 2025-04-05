using System.Text.Json.Serialization;

namespace OAuth.Server.ExternalModels
{
    public class GetUser
    { 
        [JsonPropertyName("email")]
        public string Email { get; set; }
    }
}
