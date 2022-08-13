using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using daemon_console.Models;

namespace daemon_console.Services;


/*
 * This class provides the basic create, read, update, delete (CRUD) actions available to this application from the
 * TodoList-WebApi.
 */
public interface ITodoService
{
    Task<List<Todo>> GetAllTodosAsync();
    Task<Todo> GetTodoAsync(Guid id);
    Task<Guid> CreateTodoAsync(Todo todo);
    Task<Guid> UpdateTodoAsync(Guid id, Todo todo);
    Task DeleteTodoAsync(Guid id);
}