using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using daemon_console.Options;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace daemon_console.Services;

/*
 * This service contains the main ConfidentialClientApplication that is used throughout the app. This service is
 * responsible for acquiring access tokens form Azure using the credentials provided in your appsettings.json file.
 *
 * You can find a more detailed reading here:
 *
 * https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-net-instantiate-confidential-client-config-options
 */
public class ConfidentialClientApplicationService : IConfidentialClientApplicationService
{
    private AzureAdOptions _azureAdOptions;
    private DownstreamApiOptions _downStreamApiOptions;

    /*
     * The 'AzureAdOptions' and 'DownStreamApiOptions' are provided from the 'AzureAd' and 'DownStreamApi' sections of
     * the appsettings.json file respectively.
     */
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
                // Create a new ConfidentialClientApplication based on whether or not your application is configured to
                // use a client secret or certificate in your appsettings.json file.
                if (!string.IsNullOrWhiteSpace(_azureAdOptions.ClientSecret))
                {
                    _confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(_azureAdOptions.ClientId)
                        .WithAuthority(new Uri(_azureAdOptions.Authority))
                        .WithClientSecret(_azureAdOptions.ClientSecret)
                        .Build();
                }
                else if (_azureAdOptions.ClientCertificates is not null && _azureAdOptions.ClientCertificates.Any())
                {

                    ICertificateLoader certificateLoader = new DefaultCertificateLoader();
                    certificateLoader.LoadIfNeeded(_azureAdOptions.ClientCertificates.First());

                    _confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(_azureAdOptions.ClientId)
                        .WithAuthority(new Uri(_azureAdOptions.Authority))
                        .WithCertificate(_azureAdOptions.ClientCertificates.First().Certificate)
                        .Build();
                }
                else
                {
                    throw new Exception("You must choose between using a client secret or certificate. Please update the appsettings.json file.");
                }

            }

            return _confidentialClientApplication;
        }
    }

    public async Task<string> GetAccessTokenAsync()
    {
        if (string.IsNullOrEmpty(_downStreamApiOptions.Scopes))
        {
            throw new Exception("'Scopes' must be set in the 'DownStreamApi' of appsettings.json file.");
        }

        // Scopes provide the information for what the application has access to.
        var scopes = _downStreamApiOptions.Scopes.Split(' ');

        // The authenticationResult contains the access token acquired from Azure by the ConfidentialClientApplication.
        // When the ConfidentialClientApplication is created in the code above it uses the credentials provided from
        // within the appsettings.json file to request the token. The scopes provided set what actions and information
        // are available when using that token.
        var authenticationResult = await ConfidentialClientApplication
            .AcquireTokenForClient(scopes)
            .ExecuteAsync();

        return authenticationResult.AccessToken;
    }
}
