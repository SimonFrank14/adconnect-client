using System.Threading.Tasks;

namespace ConnectClient.Core.Sync
{
    public interface ISyncEngine
    {
        Task SyncAsync(bool fullSync);
    }
}
