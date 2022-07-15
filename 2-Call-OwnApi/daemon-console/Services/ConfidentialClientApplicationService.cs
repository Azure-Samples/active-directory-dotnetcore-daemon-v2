using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using daemon_console.Options;
using System;
using System.Threading.Tasks;

namespace daemon_console.Services
{
    public class ConfidentialClientApplicationService : IConfidentialClientApplicationService
    {
        private AzureAdOptions _azureAdOptions;
        private DownstreamApiOptions _downStreamApiOptions;

        public ConfidentialClientApplicationService(IOptions<AzureAdOptions> azureAdOptions, IOptions<DownstreamApiOptions> downStreamApiOptions)
        {
            _azureAdOptions = azureAdOptions.Value;
            _downStreamApiOptions = downStreamApiOptions.Value;
        }

        private IConfidentialClientApplication _confidentialClientApplication;
        private IConfidentialClientApplication ConfidentialClientApplication
        {
            get
            {
                if (_confidentialClientApplication is null)
                {

                    var clientSecretPlaceholderValue = "[Enter here a client secret for your application]";

                    if (!string.IsNullOrWhiteSpace(_azureAdOptions.ClientSecret) &&
                        _azureAdOptions.ClientSecret != clientSecretPlaceholderValue)
                    {
                        _confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(_azureAdOptions.ClientId)
                            .WithClientSecret(_azureAdOptions.ClientSecret)
                            .WithAuthority(new Uri(_azureAdOptions.Authority))
                            .Build();
                    }
                    else if (_azureAdOptions.Certificate is not null)
                    {
                        ICertificateLoader certificateLoader = new DefaultCertificateLoader();
                        certificateLoader.LoadIfNeeded(_azureAdOptions.Certificate);

                        _confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(_azureAdOptions.ClientId)
                            .WithCertificate(_azureAdOptions.Certificate.Certificate)
                            .WithAuthority(new Uri(_azureAdOptions.Authority))
                            .Build();
                    }
                    else
                    {
                        throw new Exception("You must choose between using client secret or certificate. Please update appsettings.json file.");
                    }

                    _confidentialClientApplication.AddInMemoryTokenCache();
                }

                return _confidentialClientApplication;
            }
        }

        public async Task<AuthenticationResult> GetAuthenticationResultAsync()
        {
            if (string.IsNullOrEmpty(_downStreamApiOptions.Scopes))
            {
                throw new Exception("'Scopes' must be set in the 'DownStreamApi' of appsettings.json file.");
            }

            var scopes = _downStreamApiOptions.Scopes.Split(' ');

            var authenticationResult = await ConfidentialClientApplication
                .AcquireTokenForClient(scopes)
                .ExecuteAsync();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Token acquired \n");
            Console.ResetColor();

            return authenticationResult;
        }
    }
}
