using System;
using System.Collections.Generic;
using TodoList_WebApi.Models;

namespace TodoList_WebApi.Services;

public interface ITodoService
{
    IEnumerable<Todo> GetTodos(bool hasAppPermissions, Guid userIdentifier);
    Todo GetTodo(bool hasAppPermissions, Guid id, Guid userIdentifier);
    Guid AddTodo(bool hasAppPermissions, Todo todo, Guid userIdentifier, string owner);
    Guid UpdateTodo(bool hasAppPermissions, Guid id, Todo todo, Guid userIdentifier, string owner);
    bool DeleteTodo(bool hasAppPermissions, Guid id, Guid userIdentifier);
}