using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using daemon_console.Models;

namespace daemon_console.Services;

/*
 * Simple class used to upload sample to-do's
 */
public class PostTodosService : IPostTodosService
{
    private readonly ITodoService _todoService;

    public PostTodosService(ITodoService todoService)
    {
        _todoService = todoService;
    }

    public async Task<IEnumerable<Guid>> UploadSampleTodosAsync()
    {
        Console.WriteLine("Uploading to-do's to API store\n");

        var userOne = "Alice";
        var userTwo = "Bob";

        var userOneIdentifier = Guid.NewGuid();
        var userTwoIdentifier = Guid.NewGuid();

        var sampleTodos = new List<Todo>() {
            new Todo() {
                UserId = userOneIdentifier,
                Owner = userOne,
                Title = "Feed cat."
            },
            new Todo() {
                UserId = userOneIdentifier,
                Owner = userOne,
                Title = "Feed dog."
            },
            new Todo() {
                UserId = userTwoIdentifier,
                Owner = userTwo,
                Title = "Bake bread."
            },
            new Todo() {
                UserId = userTwoIdentifier,
                Owner = userTwo,
                Title = "Butter bread."
            },
        };

        return await Task.WhenAll(sampleTodos
            .Select(td => _todoService.CreateTodoAsync(td)));
    }
}
