using Newtonsoft.Json;

namespace ConnectClient.ActiveDirectory
{
    public class LdapSettings
    {
        /// <summary>
        /// The Hostname or IP of the domain controller
        /// </summary>
        [JsonProperty("server")]
        public string Server { get; set; }

        /// <summary>
        /// Port of the LDAP server
        /// </summary>
        [JsonProperty("port")]
        public int Port { get; set; }

        /// <summary>
        /// Flag whether to use SSL (note: you also need to specify the certificate fingerpint!)
        /// </summary>
        [JsonProperty("use_ssl")]
        public bool UseSSL { get; set; }

        /// <summary>
        /// Flag whether to use TLS (note: you also need to specify the certificate fingerpint!)
        /// </summary>
        [JsonProperty("use_tls")]
        public bool UseTLS { get; set; }

        /// <summary>
        /// The name of the domain including 
        /// </summary>
        [JsonProperty("domain_fqdn")]
        public string DomainFQDN { get; set; }

        /// <summary>
        /// The domain name (NetBIOS)
        /// </summary>
        [JsonProperty("domain_netbios")]
        public string DomainNetBIOS { get; set; }

        /// <summary>
        /// The username of a read-only ldap user
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>
        /// The password of a read-only ldap user
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; set; }

        /// <summary>
        /// Thumbprint of a certificate which is included in the certificate
        /// chain used by the Domain Controller.
        /// </summary>
        [JsonProperty("certificate_thumbprint")]
        public string CertificateThumbprint { get; set; }

        /// <summary>
        /// Specifies which attribute is used for usernames.
        /// </summary>
        public UsernameProperty UsernameProperty { get; set; } = UsernameProperty.UserPrincipalName;
    }
}
