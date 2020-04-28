using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace ConnectClient.ActiveDirectory
{
    public class LdapUserProvider : ILdapUserProvider
    {
        private const string MemberOfAttribute = "memberOf";
        private const string EmailAttribute = "mail";
        private const string DisplayNameAttribute = "displayName";
        private const string FirstnameAttribute = "givenName";
        private const string LastnameAttribute = "sn";
        private const string UsernameAttribute = "sAMAccountName";
        private const string AccountControlAttribute = "UserAccountControl";
        private const string GuidAttribute = "objectGUID";
        private const string UserPrincipalNameAttribute = "userPrincipalName";
        private const string LastModifiedAttribute = "whenChanged";
        private const string LastModifiedDateFormat = "yyyyMMddHHmmss.f'Z'";
        private const int IsActiveAttributeValue = 0x2;

        private const string SearchFilter = "(objectclass=user)";


        private readonly LdapSettings settings;
        private readonly ILogger<LdapUserProvider> logger;

        public LdapUserProvider(LdapSettings settings, ILogger<LdapUserProvider> logger)
        {
            this.settings = settings;
            this.logger = logger;
        }

        public List<User> GetUsers(IEnumerable<string> organizationalUnits, string uniqueIdAttributeName)
        {
            var list = new List<User>();

            using (var ldapConnection = new LdapConnection())
            {
                try
                {
                    ConnectLdapConnection(ldapConnection);
                    ldapConnection.Bind(settings.Username, settings.Password);

                    var attributes = new List<string>() { LastModifiedAttribute, UserPrincipalNameAttribute, MemberOfAttribute, EmailAttribute, DisplayNameAttribute, FirstnameAttribute, LastnameAttribute, UsernameAttribute, AccountControlAttribute, GuidAttribute };

                    if (!string.IsNullOrEmpty(uniqueIdAttributeName))
                    {
                        attributes.Add(uniqueIdAttributeName);
                    }

                    foreach (var ou in organizationalUnits)
                    {
                        logger.LogDebug($"Search OU {ou}...");
                        var results = ldapConnection.Search(ou, LdapConnection.SCOPE_SUB, SearchFilter, attributes.ToArray(), false);

                        while (results.HasMore())
                        {
                            var entry = results.Next();

                            logger.LogDebug($"Found user {entry.DN}");

                            string uniqueId = null;

                            if (!string.IsNullOrEmpty(uniqueIdAttributeName))
                            {
                                uniqueId = entry.getAttribute(uniqueIdAttributeName).StringValue;
                            }

                            var isActive = false;
                            var accountControlValue = entry.getAttribute(AccountControlAttribute)?.StringValue;

                            if (accountControlValue != null)
                            {
                                var accountControlIntValue = int.Parse(accountControlValue);
                                isActive = !((accountControlIntValue & IsActiveAttributeValue) == IsActiveAttributeValue);
                            }

                            var lastModified = DateTime.ParseExact(
                                entry.getAttribute(LastModifiedAttribute).StringValue,
                                LastModifiedDateFormat,
                                CultureInfo.InvariantCulture
                            );

                            list.Add(new User
                            {
                                IsActive = isActive,
                                Username = entry.getAttribute(UsernameAttribute)?.StringValue,
                                UPN = entry.getAttribute(UserPrincipalNameAttribute)?.StringValue,
                                Firstname = entry.getAttribute(FirstnameAttribute)?.StringValue,
                                Lastname = entry.getAttribute(LastnameAttribute)?.StringValue,
                                DisplayName = entry.getAttribute(DisplayNameAttribute)?.StringValue,
                                Email = entry.getAttribute(EmailAttribute)?.StringValue,
                                Guid = GetGuidAsString(entry.getAttribute(GuidAttribute)?.ByteValueArray),
                                UniqueId = uniqueId,
                                Groups = entry.getAttribute(MemberOfAttribute)?.StringValueArray,
                                OU = GetOU(entry.DN),
                                LastModified = lastModified
                            });
                        }
                    }
                }
                catch (LdapException e)
                {
                    logger.LogError(e, "LDAP error.");
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Non-LDAP error.");
                }

                // Needed to prevent Dispose() from an infinite call, see https://github.com/dsbenghe/Novell.Directory.Ldap.NETStandard/issues/101
                if (ldapConnection.TLS)
                {
                    ldapConnection.StopTls();
                }
            }

            return list;
        }

        private void ConnectLdapConnection(LdapConnection ldapConnection)
        {
            ldapConnection.UserDefinedServerCertValidationDelegate += CheckCertificateCallback;

            /**
             * CONFIGURE SSL
             */
            if (settings.UseSSL)
            {
                logger.LogDebug("Starting SSL session.");
                ldapConnection.SecureSocketLayer = true;
            }

            /**
             * CONNECT
             */
            ldapConnection.Connect(settings.Server, settings.Port);

            /**
             * CONFIGURE TLS
             */
            if (settings.UseTLS)
            {
                logger.LogDebug("Starting TLS session.");
                ldapConnection.StartTls();
            }
        }

        private bool CheckCertificateCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            foreach (var cert in chain.ChainElements)
            {
                if (cert.Certificate.Thumbprint.ToLower() == settings.CertificateThumbprint.ToLower())
                {
                    return true;
                }
            }

            return false;
        }

        private string GetOU(string dn)
        {
            return (new Novell.Directory.Ldap.Utilclass.DN(dn).Parent.ToString());
        }

        private string GetGuidAsString(sbyte[][] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return null;
            }

            var guid = new Guid(Array.ConvertAll(bytes[0], x => (byte)x));
            return guid.ToString();
        }
    }
}
