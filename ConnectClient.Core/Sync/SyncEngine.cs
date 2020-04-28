using ConnectClient.ActiveDirectory;
using ConnectClient.Models.Response;
using ConnectClient.Rest;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ConnectClient.Core.Sync
{
    public class SyncEngine : ISyncEngine
    {
        private readonly string uniqueIdAttributeName;
        private readonly string[] organizationalUnits;
        private readonly ILdapUserProvider ldapUserProvider;
        private readonly IClient client;
        private readonly ILogger<SyncEngine> logger;

        public SyncEngine(string uniqueIdAttributeName, string[] organizationalUnits, ILdapUserProvider ldapUserProvider, IClient client, ILogger<SyncEngine> logger)
        {
            this.uniqueIdAttributeName = uniqueIdAttributeName;
            this.organizationalUnits = organizationalUnits;
            this.ldapUserProvider = ldapUserProvider;
            this.client = client;
            this.logger = logger;
        }

        public async Task SyncAsync(bool fullSync)
        {
            var lastSync = await GetLastSyncDateTimeAsync();
            logger.LogInformation("Start synchronization.");
            logger.LogInformation($"Last sync: {(lastSync == null ? "never" : lastSync.ToString())}");

            if(lastSync == null)
            {
                fullSync = true;
            }

            if (fullSync)
            {
                logger.LogInformation("Performing full sync.");
            }
            else
            {
                logger.LogInformation("Performing delta sync.");
            }

            logger.LogInformation("Fetching current user base...");

            var response = await client.GetUsersAsync();
            var userListResponse = response as ListActiveDirectoryUserResponse;

            if(userListResponse == null)
            {
                logger.LogError("Failed to fetch users from the cloud. Try again in DEBUG mode.");
                return;
            }

            var cloudUserGuids = userListResponse.UserGuids;

            logger.LogInformation($"Got {cloudUserGuids.Length} user(s).");

            logger.LogInformation("Searching Active Directory...");
            var ldapUsers = await Task.Run(() => ldapUserProvider.GetUsers(organizationalUnits, uniqueIdAttributeName));
            logger.LogInformation($"Found {ldapUsers.Count} user(s).");

            logger.LogInformation("Compute changeset...");

            var add = new List<User>();
            var addSkiped = new List<User>();
            var update = new List<User>();
            var updateSkipped = new List<User>();
            var remove = new List<string>();

            foreach (var ldapUser in ldapUsers)
            {
                if (cloudUserGuids.Contains(ldapUser.Guid))
                {
                    update.Add(ldapUser);
                }
                else
                {
                    add.Add(ldapUser);
                }
            }

            var ldapGuids = ldapUsers.Select(x => x.Guid).Where(x => x != null);

            foreach (var guid in cloudUserGuids)
            {
                if (ldapGuids.Contains(guid) == false)
                {
                    remove.Add(guid);
                }
            }

            logger.LogInformation($"Changeset: {add.Count} add(s), {update.Count} update(s) and {remove.Count} removal(s).");

            foreach(var user in add)
            {
                if (IsValidUser(user))
                {
                    logger.LogDebug($"Adding user {user.Guid}...");
                    await client.AddUserAsync(user);
                    logger.LogDebug("Done.");
                }
                else
                {
                    logger.LogDebug($"Skipping invalid user {user.Guid}.");
                    addSkiped.Add(user);
                }
            }

            logger.LogInformation($"{add.Count} user(s) added.");

            foreach(var user in update)
            {
                if (fullSync == false && user.LastModified < lastSync)
                {
                    logger.LogDebug($"Skipping user {user.Guid} as it was not modified since last sync.");
                    updateSkipped.Add(user);
                }
                else
                {
                    logger.LogDebug($"Updating user {user.Guid}...");
                    await client.UpdateUserAsync(user);
                    logger.LogDebug("Done.");
                }
            }

            logger.LogInformation($"{add.Count - addSkiped.Count} user(s) added.");
            logger.LogInformation($"{addSkiped.Count} user(s) skipped.");
            logger.LogInformation($"{update.Count - updateSkipped.Count} user(s) updated.");
            logger.LogInformation($"{updateSkipped.Count} user(s) skipped.");

            foreach (var guid in remove)
            {
                logger.LogDebug($"Removing user {guid}...");
                await client.RemoveUserAsync(guid);
                logger.LogDebug("Done.");
            }

            logger.LogInformation($"{remove.Count} user(s) removed.");
            logger.LogInformation("Done.");

            await SetLastSyncDateTimeAsync();
        }

        private string GetLastSyncDateTimePath() => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "SchulIT", "AD Connect Client", "last_sync.json");

        private async Task<DateTime?> GetLastSyncDateTimeAsync()
        {
            var path = GetLastSyncDateTimePath();

            if(!File.Exists(path))
            {
                return null;
            }

            using(var streamReader = new StreamReader(path))
            {
                var contents = await streamReader.ReadToEndAsync();
                return DateTime.Parse(contents);
            }
        }

        private async Task SetLastSyncDateTimeAsync()
        {
            var path = GetLastSyncDateTimePath();
            using (var streamReader = new StreamWriter(path))
            {
                await streamReader.WriteLineAsync(DateTime.Now.ToString());
            }
        }

        private bool IsValidUser(User user)
        {
            return !string.IsNullOrEmpty(user.Guid)
                && !string.IsNullOrEmpty(user.UPN)
                && !string.IsNullOrEmpty(user.Username)
                && !string.IsNullOrEmpty(user.Firstname)
                && !string.IsNullOrEmpty(user.Lastname)
                && !string.IsNullOrEmpty(user.Email)
                && !string.IsNullOrEmpty(user.OU);
        }
    }
}
