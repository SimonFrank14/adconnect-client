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

        public List<User> GetUsers(IEnumerable<string> organizationalUnits, LdapSettings settings)
        {
            var list = new List<User>();

            if(string.IsNullOrEmpty(settings.Server))
            {
                return list;
            }

            using (var ldapConnection = new LdapConnection())
            {
                ConnectLdapConnection(ldapConnection, settings);
                ldapConnection.Bind(settings.Username, settings.Password);

                var attributes = new List<string>() { LastModifiedAttribute, UserPrincipalNameAttribute, MemberOfAttribute, EmailAttribute, DisplayNameAttribute, FirstnameAttribute, LastnameAttribute, UsernameAttribute, AccountControlAttribute, GuidAttribute };

                foreach (var ou in organizationalUnits)
                {
                    var results = ldapConnection.Search(ou, LdapConnection.SCOPE_SUB, SearchFilter, attributes.ToArray(), false);

                    while (results.HasMore())
                    {
                        var entry = results.Next();

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
                            Groups = entry.getAttribute(MemberOfAttribute)?.StringValueArray,
                            OU = GetOU(entry.DN),
                            LastModified = lastModified
                        });
                    }
                }


                // Needed to prevent Dispose() from an infinite call, see https://github.com/dsbenghe/Novell.Directory.Ldap.NETStandard/issues/101
                if (ldapConnection.TLS)
                {
                    ldapConnection.StopTls();
                }
            }

            return list;
        }

        private void ConnectLdapConnection(LdapConnection ldapConnection, LdapSettings settings)
        {
            if (!string.IsNullOrEmpty(settings.CertificateThumbprint))
            {
                ldapConnection.UserDefinedServerCertValidationDelegate += (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
                {
                    foreach (var cert in chain.ChainElements)
                    {
                        if (cert.Certificate.Thumbprint.Equals(settings.CertificateThumbprint, StringComparison.CurrentCultureIgnoreCase))
                        {
                            return true;
                        }
                    }

                    return false;
                };
            }

            /**
             * CONFIGURE SSL
             */
            if (settings.UseSSL)
            {
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
                ldapConnection.StartTls();
            }
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
