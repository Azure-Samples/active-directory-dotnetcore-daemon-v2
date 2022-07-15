using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TodoList_WebApi.Models;

namespace TodoList_WebApi.Services;

public class TodoService : ITodoService
{
    private ConcurrentDictionary<Guid, Todo> _todoStore = new ConcurrentDictionary<Guid, Todo>();

    public IEnumerable<Todo> GetTodos(bool hasAppPermissions, Guid userIdentifier)
    {
        if (hasAppPermissions)
        {
            return _todoStore.Values;
        }

        return _todoStore.Values
            .Where(td => td.UserId == userIdentifier);
    }

    public Todo GetTodo(bool hasAppPermissions, Guid id, Guid userIdentifier)
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

    public Guid AddTodo(bool hasAppPermissions, Todo todo, Guid userIdentifier, string owner)
    {
        todo.Id = Guid.NewGuid();

        if (hasAppPermissions)
        {
            if (!_todoStore.TryAdd(todo.Id, todo))
            {
                return Guid.Empty;
            }

            return todo.Id;
        }

        // Don't let users post to-do's under other people's names.
        if (userIdentifier != todo.UserId || owner != todo.Owner || !_todoStore.TryAdd(todo.Id, todo))
        {
            return Guid.Empty;
        }

        return todo.Id;
    }

    public Guid UpdateTodo(bool hasAppPermissions, Guid id, Todo todo, Guid userIdentifier, string owner)
    {
        var todoExists = _todoStore.TryGetValue(id, out var oldTodo);

        if (!todoExists)
        {
            return Guid.Empty;
        }

        todo.Id = id;

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

    public bool DeleteTodo(bool hasAppPermissions, Guid id, Guid userIdentifier)
    {
        if (!_todoStore.TryGetValue(id, out var todo))
        {
            return false;
        }

        var kvp = new KeyValuePair<Guid, Todo>(id, todo);

        if (hasAppPermissions)
        {
            return _todoStore.TryRemove(kvp);
        }

        var isUsersTodo = _todoStore.Values
            .Any(td => td.Id == id && td.UserId == userIdentifier);

        return isUsersTodo && _todoStore.TryRemove(kvp);
    }
}