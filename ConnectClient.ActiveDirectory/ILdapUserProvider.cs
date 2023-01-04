using System.Collections.Generic;

namespace ConnectClient.ActiveDirectory
{
    public interface ILdapUserProvider
    {
        List<User> GetUsers(IEnumerable<string> organizationalUnits, LdapSettings settings);
    }
}
