using System;
using System.Collections.Generic;
using TodoList_WebApi.Models;

namespace TodoList_WebApi.Services;

public interface ITodoService
{
    IEnumerable<Todo> GetTodos(bool hasAppPermissions, string userIdentifier);
    Todo GetTodo(bool hasAppPermissions, Guid id, string userIdentifier);
    Guid AddTodo(bool hasAppPermissions, Todo todo, string userIdentifier, string owner);
    Guid UpdateTodo(bool hasAppPermissions, Todo todo, string userIdentifier, string owner);
    bool DeleteTodo(bool hasAppPermissions, Guid id, string userIdentifier);
}