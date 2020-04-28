using Newtonsoft.Json;

namespace ConnectClient.Models.Response
{
    public class ListActiveDirectoryUserResponse : IResponse
    {
        [JsonProperty("users")]
        public string[] UserGuids { get; set; }
    }
}
