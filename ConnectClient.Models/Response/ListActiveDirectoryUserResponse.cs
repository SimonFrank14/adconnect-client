using Newtonsoft.Json;

namespace ConnectClient.Models.Response
{
    public class ListActiveDirectoryUserResponse : IResponse
    {
        [JsonProperty("users")]
        public OnlineUser[] Users { get; set; }
    }
}
