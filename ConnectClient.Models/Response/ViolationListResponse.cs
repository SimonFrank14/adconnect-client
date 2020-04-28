using Newtonsoft.Json;

namespace ConnectClient.Models.Response
{
    public class ViolationListResponse : ErrorResponse
    {
        [JsonProperty("violations")]
        public Violation[] Violations { get; set; }
    }
}
