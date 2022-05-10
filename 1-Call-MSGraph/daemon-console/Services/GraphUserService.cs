using System.Net.Http.Headers;
using daemon_console.Options;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace daemon_console.Services
{
    public class GraphUserService : IGraphUserService
    {
        private DownstreamApiOptions _downStreamApiOptions;
        private IConfidentialClientApplicationService _confidentialClientApplicationService;
        private GraphServiceClient? _graphServiceClient;
        private GraphServiceClient GraphServiceClient
        {
            get
            {
                if (_graphServiceClient is null)
                {
                    _graphServiceClient = new GraphServiceClient(_downStreamApiOptions.BaseUrl,
                    new DelegateAuthenticationProvider(async requestMessage =>
                    {
                        // Retrieve an access token for Microsoft Graph (gets a fresh token if needed).
                        var result = await _confidentialClientApplicationService.GetAuthenticationResultAsync();

                        // Add the access token in the Authorization header of the API request.
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", result.AccessToken);

                    }));
                }

                return _graphServiceClient;
            }
        }

        public GraphUserService(IConfidentialClientApplicationService confidentialClientApplicationService,
            IOptions<DownstreamApiOptions> downStreamApiOptions)
        {
            _confidentialClientApplicationService = confidentialClientApplicationService;
            _downStreamApiOptions = downStreamApiOptions.Value;
        }

        public async Task<IGraphServiceUsersCollectionPage> GetAllUserData()
        {
            try
            {
                return await GraphServiceClient
                    .Users
                    .Request()
                    .GetAsync();
            }
            catch (ServiceException ex)
            {
                if (ex.InnerException is not null && ex.InnerException.GetType() == typeof(MsalServiceException))
                {
                    switch (ex.InnerException.Message)
                    {
                        case var m when ex.InnerException.Message.Contains("AADSTS7000215"):
                            throw new Exception("Incorrect client secret provided. Check the appsettings.json file.");

                        case var m when ex.InnerException.Message.Contains("AADSTS1002012"):
                            throw new Exception("The scope provided is incorrect. Check the appsettings.json file.");

                        default:
                            throw ex;
                    }
                }

                throw ex;
            }
        }
    }
}