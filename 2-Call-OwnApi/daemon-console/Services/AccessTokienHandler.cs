using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace daemon_console.Services;

/*
 * Handler for retrieving including an access token with the authentication header before each request.
 *
 * Leverages the IConfidentialClientApplicationService to retrieve tokens either from Azure or the
 * ConfidentialClientApplication token cache.
 */
public class AccessTokenHandler : DelegatingHandler
{
    private IConfidentialClientApplicationService _confidentialClientApplicationService;

    public AccessTokenHandler(IConfidentialClientApplicationService confidentialClientApplicationService)
    {
        _confidentialClientApplicationService = confidentialClientApplicationService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Retrieves access tokens straight from the token cache in the ConfidentialClientApplication. If a token has
        // not yet been cached it will retrieve a token directly from Azure and then cache it.
        var accessToken = await _confidentialClientApplicationService.GetAccessTokenAsync();

        request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
