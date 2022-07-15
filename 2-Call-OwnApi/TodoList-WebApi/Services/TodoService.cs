using System;
using System.Collections.Generic;
using System.Linq;
using TodoList_WebApi.Models;

namespace TodoList_WebApi.Services;

public class TodoService : ITodoService
{
    private Dictionary<Guid, Todo> _todoStore = new Dictionary<Guid, Todo>();

    public IEnumerable<Todo> GetTodos(bool hasAppPermissions, string userIdentifier)
    {
        if (hasAppPermissions)
        {
            return _todoStore.Values;
        }

        return _todoStore.Values
            .Where(td => td.UserId == userIdentifier);
    }

    public Todo GetTodo(bool hasAppPermissions, Guid id, string userIdentifier)
    {
        if (hasAppPermissions)
        {
            _todoStore.TryGetValue(id, out var todo);

            return todo;
        }

        var usersTodos = _todoStore
            .Where(td => td.Value.UserId == userIdentifier)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        usersTodos.TryGetValue(id, out var userTodo);

        return userTodo;

    }

    public Guid AddTodo(bool hasAppPermissions, Todo todo, string userIdentifier, string owner)
    {
        todo.Id = Guid.NewGuid();

        if (hasAppPermissions)
        {
            _todoStore.Add(todo.Id, todo);
            return todo.Id;
        }

        // Don't let users post to-do's under other people's names.
        if (userIdentifier != todo.UserId || owner != todo.Owner)
        {
            return Guid.Empty;
        }

        _todoStore.Add(todo.Id, todo);
        return todo.Id;
    }

    public Guid UpdateTodo(bool hasAppPermissions, Todo todo, string userIdentifier, string owner)
    {
        var todoExists = _todoStore.TryGetValue(todo.Id, out var oldTodo);

        if (!todoExists)
        {
            return Guid.Empty;
        }

        if (hasAppPermissions)
        {
            _todoStore[todo.Id] = todo;
            return todo.Id;
        }

        if (oldTodo.UserId != userIdentifier && oldTodo.Owner != owner)
        {
            return Guid.Empty;
        }

        _todoStore[todo.Id] = todo;

        return todo.Id;
    }

    public bool DeleteTodo(bool hasAppPermissions, Guid id, string userIdentifier)
    {
        if (hasAppPermissions)
        {
            return _todoStore.Remove(id);
        }

        var isUsersTodo = _todoStore.Values
            .Any(td => td.Id == id && td.UserId == userIdentifier);

        return isUsersTodo && _todoStore.Remove(id);
    }
}