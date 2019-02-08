using Newtonsoft.Json;

namespace QboxNext.Server.Auth0.Models
{
    public class AppMetadata
    {
        [JsonProperty("qboxSerialNumber")]
        public string QboxSerialNumber { get; set; }
    }
}