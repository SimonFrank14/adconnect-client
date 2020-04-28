using ConnectClient.ActiveDirectory;
using ConnectClient.Models.Response;
using System.Threading.Tasks;

namespace ConnectClient.Rest
{
    public interface IClient
    {
        Task<IResponse> GetUsersAsync();

        Task<IResponse> AddUserAsync(User user);

        Task<IResponse> UpdateUserAsync(User user);

        Task<IResponse> RemoveUserAsync(string guid);
    }
}
