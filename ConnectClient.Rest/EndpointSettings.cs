using Newtonsoft.Json;

namespace ConnectClient.Rest
{
    public class EndpointSettings
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
