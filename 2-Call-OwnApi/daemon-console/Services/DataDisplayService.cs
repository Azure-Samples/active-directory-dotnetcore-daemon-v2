using System;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using System.Net.Http;

namespace daemon_console.Services
{
    public class DataDisplayService : IDataDisplayService
    {
        private ITodoService _todoService;

        public DataDisplayService(ITodoService todoService)
        {
            _todoService = todoService;
        }

        public async Task DisplayAllTodosAsync()
        {
            try
            {
                var todos = await _todoService.GetAllTodos();

                foreach (var todo in todos)
                {
                    foreach (var property in todo.GetType().GetProperties())
                    {
                        Console.WriteLine($"{property.Name} = {property.GetValue(todo)}");
                    }
                    Console.WriteLine();
                }
            }
            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                // Invalid scope. The scope has to be of the form "https://resourceurl/.default"
                // Mitigation: change the scope to be as expected
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Scope provided is not supported");
                Console.ResetColor();
            }
            catch (HttpRequestException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to call the web API: {ex.StatusCode}");

                // Note that if you got reponse.Code == 403 and reponse.content.code == "Authorization_RequestDenied"
                // this is because the tenant admin as not granted consent for the application to call the Web API
                Console.WriteLine($"Content: {ex.Message}");

                Console.ResetColor();
            }
        }
    }
}