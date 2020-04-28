using Newtonsoft.Json;

namespace ConnectClient.Models.Response
{
    public class ErrorResponse : IResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
