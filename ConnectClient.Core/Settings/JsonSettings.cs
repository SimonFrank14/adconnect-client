using ConnectClient.ActiveDirectory;
using ConnectClient.Rest;
using Newtonsoft.Json;

namespace ConnectClient.Core.Settings
{
    public class JsonSettings
    {
        [JsonProperty("endpoint")]
        public EndpointSettings Endpoint { get; set; } = new EndpointSettings();

        [JsonProperty("ldap")]
        public LdapSettings Ldap { get; set; } = new LdapSettings();

        [JsonProperty("interal_id")]
        public string UniqueIdAttributeName { get; set; }

        [JsonProperty("organizational_units")]
        public string[] OrganizationalUnits { get; set; }
    }
}
