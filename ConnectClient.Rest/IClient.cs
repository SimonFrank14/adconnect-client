using ConnectClient.ActiveDirectory;
using ConnectClient.Models.Response;
using System.Threading.Tasks;

namespace ConnectClient.Rest
{
    public interface IClient
    {
        Task<IResponse> GetUsersAsync(EndpointSettings settings);

        Task<IResponse> AddUserAsync(User user, EndpointSettings settings);

        Task<IResponse> UpdateUserAsync(User user, EndpointSettings settings);

        Task<IResponse> RemoveUserAsync(string guid, EndpointSettings settings);
    }
}
