using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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

    public async Task<Guid> AddAsync(Todo todo)
    {
        var httpClient = await PrepareHttpClientAsync();
        var response = await httpClient.PostAsJsonAsync($"{_downStreamApiOptions.BaseUrl}api/todo", todo);

        if (response.IsSuccessStatusCode)
        {
            var todoIdResponse = (await response.Content.ReadAsStringAsync()).Trim('"');

            if (Guid.TryParse(todoIdResponse, out var todoId))
            {
                return todoId;
            }
        }

        throw new HttpRequestException($"Request failed with status code: {response.StatusCode}\n");
    }

    public async Task DeleteAsync(Guid id)
    {
        var httpClient = await PrepareHttpClientAsync();
        var response = await httpClient.DeleteAsync($"{_downStreamApiOptions.BaseUrl}api/todo/{id}");

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        throw new HttpRequestException($"Request failed with status code: {response.StatusCode}\n");
    }

    public async Task<List<Todo>> GetAllAsync()
    {
        var httpClient = await PrepareHttpClientAsync();
        var response = await httpClient.GetAsync($"{_downStreamApiOptions.BaseUrl}api/todo");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();

            return JsonNode.Parse(json).Deserialize<List<Todo>>();
        }
        else
        {
            var content = await response.Content.ReadAsStringAsync();

            throw new HttpRequestException($"Request failed with status code: {response.StatusCode}\n");
        }
    }

    public async Task<Todo> GetAsync(Guid id)
    {
        var httpClient = await PrepareHttpClientAsync();
        var response = await httpClient.GetAsync($"{_downStreamApiOptions.BaseUrl}api/todo/{id}");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();

            return JsonNode.Parse(json).Deserialize<Todo>();
        }
        else
        {
            var content = await response.Content.ReadAsStringAsync();

            throw new HttpRequestException($"Request failed with status code: {response.StatusCode}\n");
        }
    }

    public async Task<Guid> UpdateAsync(Guid id, Todo todo)
    {
        var httpClient = await PrepareHttpClientAsync();
        var response = await httpClient.PostAsJsonAsync<Todo>($"{_downStreamApiOptions.BaseUrl}api/todo/{id}", todo);

        if (response.IsSuccessStatusCode)
        {
            var todoId = await response.Content.ReadAsStringAsync();

            return Guid.Parse(todoId.Trim('"'));
        }
        else
        {
            var content = await response.Content.ReadAsStringAsync();

            throw new HttpRequestException($"Request failed with status code: {response.StatusCode}\n");
        }
    }

    private async Task<HttpClient> PrepareHttpClientAsync()
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

        return httpClient;
    }
}