using System;
using System.Threading.Tasks;

namespace daemon_console.Services;

public interface IDataDisplayService
{
    public Task DisplayAllTodosAsync();
    public Task DisplayTodoAsync(Guid id);
}
