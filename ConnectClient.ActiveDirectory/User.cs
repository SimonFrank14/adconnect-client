using System;
using System.Collections.Generic;

namespace ConnectClient.ActiveDirectory
{
    public class User
    {
        public bool IsActive { get; set; }

        public string Username { get; set; }

        public string UPN { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string DisplayName { get; set; }

        public string Guid { get; set; }

        public string OU { get; set; }

        public string Email { get; set; }

        public IEnumerable<string> Groups { get; set; } = new List<string>();

        public DateTime LastModified { get; set; }
    }
}
