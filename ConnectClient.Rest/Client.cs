using ConnectClient.ActiveDirectory;
using ConnectClient.Models.Request;
using ConnectClient.Models.Response;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ConnectClient.Rest
{
    public class Client : IClient
    {

        private const string Endpoint = "/api/ad_connect";

        private readonly EndpointSettings settings;
        private readonly ILogger<Client> logger;

        public Client(EndpointSettings settings, ILogger<Client> logger)
        {
            this.settings = settings;
            this.logger = logger;
        }

        private async Task<IResponse> SendAsync<T>(IRequest request, Func<HttpClient, StringContent, Task<HttpResponseMessage>> action)
            where T : IResponse
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(settings.Url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("X-Token", settings.Token);

                logger.LogDebug($"Endpoint Base-URL is: {client.BaseAddress.ToString()}");

                var json = JsonConvert.SerializeObject(request, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await action(client, content);

                logger.LogDebug($"Got HTTP status code {response.StatusCode}.");

                try
                {
                    if (response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                        {
                            return new EmptyResponse();
                        }

                        return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        return JsonConvert.DeserializeObject<ViolationListResponse>(responseContent);
                    }

                    return JsonConvert.DeserializeObject<ErrorResponse>(responseContent);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Failed to send request.");
                }
            }

            return null;
        }

        public Task<IResponse> AddUserAsync(User user)
        {
            return SendAsync<ListActiveDirectoryUserResponse>(ToActiveDirectoryUser(user), (client, content) => client.PostAsync(Endpoint, content));
        }

        public Task<IResponse> GetUsersAsync()
        {
            return SendAsync<ListActiveDirectoryUserResponse>(null, (client, content) => client.GetAsync(Endpoint));
        }

        public Task<IResponse> RemoveUserAsync(string guid)
        {
            if(string.IsNullOrEmpty(guid))
            {
                throw new ArgumentException("guid must not be empty or null.");
            }

            var endpoint = Endpoint.TrimEnd('/') + "/" + guid;
            return SendAsync<ListActiveDirectoryUserResponse>(null, (client, content) => client.DeleteAsync(endpoint));
        }

        public Task<IResponse> UpdateUserAsync(User user)
        {
            CheckUserAndThrowIfInvalid(user);
            var endpoint = Endpoint.TrimEnd('/') + "/" + user.Guid;
            return SendAsync<ListActiveDirectoryUserResponse>(ToActiveDirectoryUser(user), (client, content) => client.PatchAsync(endpoint, content));
        }

        private void CheckUserAndThrowIfInvalid(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user must not be null");
            }

            if (string.IsNullOrEmpty(user.Guid))
            {
                throw new ArgumentException("user.Guid must not be null");
            }
        }

        private ActiveDirectoryUser ToActiveDirectoryUser(User user)
        {
            return new ActiveDirectoryUser
            {
                ObjectGuid = user.Guid,
                SamAccountName = user.Username,
                UserPrincipalName = user.UPN,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                Email = user.Email,
                Ou = user.OU,
                UniqueId = user.UniqueId,
                Groups = user.Groups.ToArray()
            };
        }
    }
}
