using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using daemon_console.Models;

namespace daemon_console.Services;

public interface ITodoService
{
    Task<List<Todo>> GetAllAsync();
    Task<Todo> GetAsync(Guid id);
    Task<Guid> AddAsync(Todo todo);
    Task<Guid> UpdateAsync(Guid id, Todo todo);
    Task DeleteAsync(Guid id);
}