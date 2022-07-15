using System.Collections.Generic;
using System.Threading.Tasks;
using daemon_console.Models;

public interface ITodoService
{
    Task<List<Todo>> GetAllTodos();
}