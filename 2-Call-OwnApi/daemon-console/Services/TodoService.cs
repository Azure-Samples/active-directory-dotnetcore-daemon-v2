using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using daemon_console.Models;
using daemon_console.Options;
using Microsoft.Extensions.Options;

namespace daemon_console.Services;

public class TodoService : ITodoService
{
    private IConfidentialClientApplicationService _confidentialClientApplicationService;
    private DownstreamApiOptions _downStreamApiOptions;

    public TodoService(
        IConfidentialClientApplicationService confidentialClientApplicationService,
        IOptions<AzureAdOptions> azureAdOptions,
        IOptions<DownstreamApiOptions> downStreamApiOptions)
    {
        _confidentialClientApplicationService = confidentialClientApplicationService;
        _downStreamApiOptions = downStreamApiOptions.Value;
    }
    public async Task<List<Todo>> GetAllTodos()
    {
        var authenticationResult = await _confidentialClientApplicationService.GetAuthenticationResultAsync();

        var httpClient = new HttpClient();
        var defaultRequestHeaders = httpClient.DefaultRequestHeaders;

        if (defaultRequestHeaders.Accept is null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
        {
            httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

        HttpResponseMessage response = await httpClient.GetAsync($"{_downStreamApiOptions.BaseUrl}api/todo");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();

            return JsonNode.Parse(json).Deserialize<List<Todo>>();
        }
        else
        {
            var content = await response.Content.ReadAsStringAsync();

            throw new HttpRequestException(content, null, response.StatusCode);
        }
    }
}