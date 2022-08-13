using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace daemon_console.Services;

public interface IPostTodosService
{
    public Task<IEnumerable<Guid>> UploadSampleTodosAsync();
}