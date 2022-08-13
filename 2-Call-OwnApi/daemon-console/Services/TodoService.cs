using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using daemon_console.Models;

namespace daemon_console.Services;

/*
 * This class provides the basic create, read, update, delete (CRUD) actions available to this application from the
 * TodoList-WebApi. Authentication is handled automatically before each request by the custom HttpClient created
 * in the application's configuration.
 */
public class TodoService : ITodoService
{
    private IConfidentialClientApplicationService _confidentialClientApplicationService;
    private HttpClient _httpClient;

    public TodoService(
        IConfidentialClientApplicationService confidentialClientApplicationService,
        HttpClient httpClient)
    {
        _confidentialClientApplicationService = confidentialClientApplicationService;
        _httpClient = httpClient;
    }

    public async Task<Guid> CreateTodoAsync(Todo todo)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/todo", todo);

        if (response.IsSuccessStatusCode)
        {
            var todoIdResponse = await response.Content.ReadAsStringAsync();

            // Need to remove the excess '"' characters from the raw response
            if (Guid.TryParse(todoIdResponse.Trim('"'), out var todoId))
            {
                return todoId;
            }
        }

        throw new HttpRequestException($"Request failed with status code: {response.StatusCode}\n");
    }

    public async Task DeleteTodoAsync(Guid id)
    {
        Console.WriteLine("Deleting to-do\n");

        var response = await _httpClient.DeleteAsync($"/api/todo/{id}");

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        throw new HttpRequestException($"Request failed with status code: {response.StatusCode}\n");
    }

    public async Task<List<Todo>> GetAllTodosAsync()
    {
        Console.WriteLine("\nFetching all to-do's currently in API store\n");

        var response = await _httpClient.GetAsync("/api/todo");

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

    public async Task<Todo> GetTodoAsync(Guid id)
    {
        Console.WriteLine("Getting single to-do\n");

        var response = await _httpClient.GetAsync($"/api/todo/{id}");

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

    public async Task<Guid> UpdateTodoAsync(Guid id, Todo todo)
    {
        Console.WriteLine("Updating single to-do\n");

        var response = await _httpClient.PostAsJsonAsync<Todo>($"/api/todo/{id}", todo);

        if (response.IsSuccessStatusCode)
        {
            var todoId = await response.Content.ReadAsStringAsync();

            // Need to remove the excess '"' characters from the raw response
            return Guid.Parse(todoId.Trim('"'));
        }
        else
        {
            var content = await response.Content.ReadAsStringAsync();

            throw new HttpRequestException($"Request failed with status code: {response.StatusCode}\n");
        }
    }
}
