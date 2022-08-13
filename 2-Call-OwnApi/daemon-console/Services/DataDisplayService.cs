using System;
using System.Threading.Tasks;
using System.Linq;
using daemon_console.Models;

namespace daemon_console.Services;

public class DataDisplayService : IDataDisplayService
{
    private ITodoService _todoService;

    public DataDisplayService(ITodoService todoService)
    {
        _todoService = todoService;
    }

    public async Task DisplayAllTodosAsync()
    {
        var todos = await _todoService.GetAllTodosAsync();

        if (!todos.Any())
        {
            Console.WriteLine("No to-do's returned from API.\n");
        }

        foreach (var todo in todos)
        {
            PrintTodo(todo);
        }
    }

    public async Task DisplayTodoAsync(Guid id)
    {
        PrintTodo(await _todoService.GetTodoAsync(id));
    }

    private void PrintTodo(Todo todo)
    {
        foreach (var property in todo.GetType().GetProperties())
        {
            Console.WriteLine($"{property.Name} = {property.GetValue(todo)}");
        }
        Console.WriteLine();
    }
}
