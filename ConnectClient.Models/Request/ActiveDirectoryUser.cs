using Newtonsoft.Json;

namespace ConnectClient.Models.Request
{
    public class ActiveDirectoryUser : IRequest
    {
        [JsonProperty("object_guid")]
        public string ObjectGuid { get; set; }

        [JsonProperty("sam_account_name")]
        public string SamAccountName { get; set; }

        [JsonProperty("user_principal_name")]
        public string UserPrincipalName { get; set; }

        [JsonProperty("firstname")]
        public string Firstname { get; set; }

        [JsonProperty("lastname")]
        public string Lastname { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("ou")]
        public string Ou { get; set; }

        [JsonProperty("groups")]
        public string[] Groups { get; set; }
    }
}
