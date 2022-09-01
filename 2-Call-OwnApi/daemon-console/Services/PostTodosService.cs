using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace daemon_console.Services;

/*
 * Simple class used to upload sample to-do's
 */
public class PostTodosService : IPostTodosService
{
    private readonly ITodoService _todoService;
    private readonly GraphServiceClient _graphServiceClient;

    public PostTodosService(ITodoService todoService, GraphServiceClient graphServiceClient)
    {
        _todoService = todoService;
        _graphServiceClient = graphServiceClient;
    }

    public async Task<IEnumerable<Guid>> UploadSampleTodosAsync()
    {
        Console.WriteLine("Uploading to-do's to API store\n");

        var users = await _graphServiceClient.Users
            .Request()
            .GetAsync();


        foreach (var user in users)
        {
            Console.WriteLine(user.DisplayName);
            Console.WriteLine(user.Id);
            Console.WriteLine();
        }

        var usersToUploadTodosFor = users.Take(2);

        var sampleTodos = usersToUploadTodosFor
            .Select(user => new daemon_console.Models.Todo[] {
                new daemon_console.Models.Todo() {
                    UserId = Guid.Parse(user.Id),
                    Owner = user.DisplayName,
                    Title = "Bake bread."
                },
                new daemon_console.Models.Todo() {
                    UserId = Guid.Parse(user.Id),
                    Owner = user.DisplayName,
                    Title = "Butter bread."
                },
                new daemon_console.Models.Todo() {
                    UserId = Guid.Parse(user.Id),
                    Owner = user.DisplayName,
                    Title = "Feed Cat."
                }
            })
            .SelectMany(td => td);

        return await Task.WhenAll(sampleTodos
            .Select(td => _todoService.CreateTodoAsync(td)));
    }

    public async Task<Guid> ChangeTodoDemo(Guid todoId)
    {
        var users = await _graphServiceClient.Users
            .Request()
            .GetAsync();

        // Use a user that has not been associated with a TO-DO yet.
        var newUserInTodo = users.Take(3).Last();

        await _todoService.UpdateTodoAsync(todoId, new daemon_console.Models.Todo() {
            UserId = Guid.Parse(newUserInTodo.Id),
            Title = "Something else.",
            Owner = newUserInTodo.DisplayName
        });

        return todoId;
    }

}
