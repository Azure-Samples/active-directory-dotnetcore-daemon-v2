using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace daemon_console.Services;

public interface IConfidentialClientApplicationService
{
    public Task<string> GetAccessTokenAsync ();
}
