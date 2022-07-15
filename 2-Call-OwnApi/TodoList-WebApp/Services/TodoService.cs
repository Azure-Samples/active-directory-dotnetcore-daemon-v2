// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using TodoList_WebApp.Models;
using TodoList_WebApp.Options;

namespace TodoList_WebApp.Services;

public class TodoService : ITodoService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenAcquisition _tokenAcquisition;
    private DownstreamApiOptions _downStreamApiOptions;

    private string ApiEndpoint
    {
        get
        {
            return $"{_downStreamApiOptions.BaseUrl}api/todo/";
        }
    }

    public TodoService(
        HttpClient httpClient,
        ITokenAcquisition tokenAcquisition,
        IOptions<DownstreamApiOptions> downStreamApiOption)
    {
        _httpClient = httpClient;
        _tokenAcquisition = tokenAcquisition;
        _downStreamApiOptions = downStreamApiOption.Value;
    }

    public async Task<Todo> AddAsync(Todo todo)
    {
        await SetAuthenticationHeader();

        var response = await _httpClient.PostAsJsonAsync(ApiEndpoint, todo);

        if (response.IsSuccessStatusCode)
        {
            var todoIdResponse = (await response.Content.ReadAsStringAsync()).Trim('"');

            if (Guid.TryParse(todoIdResponse, out var todoId))
            {
                todo.Id = todoId;

                return todo;
            }
        }

        throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
    }

    public async Task DeleteAsync(Guid id)
    {
        await SetAuthenticationHeader();

        Console.WriteLine(id);

        var response = await _httpClient.DeleteAsync($"{ApiEndpoint}{id}");

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
    }

    public async Task<Todo> EditAsync(Todo todo)
    {
        await SetAuthenticationHeader();

        var response = await _httpClient.PostAsJsonAsync($"{ApiEndpoint}{todo.Id}", todo);

        var todoIdResponse = (await response.Content.ReadAsStringAsync()).Trim('"');

        if (Guid.TryParse(todoIdResponse, out var todoId))
        {
            todo.Id = todoId;

            return todo;
        }

        throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
    }

    public async Task<IEnumerable<Todo>> GetAsync()
    {
        await SetAuthenticationHeader();

        var response = await _httpClient.GetAsync($"{ApiEndpoint}");

        if (response.IsSuccessStatusCode)
        {
            var todos = await response.Content.ReadFromJsonAsync<IEnumerable<Todo>>();

            return todos!;
        }

        throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
    }

    public async Task<Todo> GetAsync(Guid id)
    {
        await SetAuthenticationHeader();

        var response = await _httpClient.GetAsync($"{ApiEndpoint}{id}");

        if (response.IsSuccessStatusCode)
        {
            var todo = await response.Content.ReadFromJsonAsync<Todo>();

            return todo!;
        }

        throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
    }

    private async Task SetAuthenticationHeader()
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer",
                await _tokenAcquisition.GetAccessTokenForUserAsync(_downStreamApiOptions.Scopes!.Split(' ')));
    }
}
