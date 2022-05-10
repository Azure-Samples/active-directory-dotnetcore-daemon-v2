using Microsoft.Graph;
using Newtonsoft.Json;

namespace daemon_console.Services
{
    public class DataDisplayService : IDataDisplayService
    {
        private IGraphUserService _graphUserService;
        public DataDisplayService(IGraphUserService graphUserService)
        {
            _graphUserService = graphUserService;
        }

        public async Task DisplayAllUsersAsync()
        {
            try
            {
                var users = await _graphUserService.GetAllUserData();

                Console.WriteLine($"Found {users.Count} users in tenant.");

                Console.WriteLine(JsonConvert.SerializeObject(users, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }));
            }
            catch (ServiceException ex)
            {
                if (ex.Error.Code == "Authorization_RequestDenied")
                {
                    throw new Exception("Application has insufficient privileges to access user data. Be sure to add 'User.Read.All' delegated permission to your application in the Azure Portal.");
                }
                throw ex;
            }
        }
    }
}