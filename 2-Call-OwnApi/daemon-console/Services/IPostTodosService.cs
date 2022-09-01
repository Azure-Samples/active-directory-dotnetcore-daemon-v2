using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using daemon_console.Models;

namespace daemon_console.Services;

public interface IPostTodosService
{
    public Task<IEnumerable<Guid>> UploadSampleTodosAsync();
    public Task<Guid> ChangeTodoDemo(Guid todoId);
}