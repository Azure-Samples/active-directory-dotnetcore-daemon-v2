using Microsoft.Graph;

namespace daemon_console.Services
{
    public interface IGraphUserService
    {
        public Task<IGraphServiceUsersCollectionPage> GetAllUserData();
    }
}
