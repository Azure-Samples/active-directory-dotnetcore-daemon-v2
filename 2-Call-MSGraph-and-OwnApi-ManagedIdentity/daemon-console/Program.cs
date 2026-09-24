using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoList_WebApi.Models;

var tokenAcquirerFactory = TokenAcquirerFactory.GetDefaultInstance();

tokenAcquirerFactory.Services
    .AddLogging(logging => logging.SetMinimumLevel(LogLevel.Warning).AddConsole())
    .AddMicrosoftGraph()
    .AddDownstreamApi(
        "TodoListApi",
        tokenAcquirerFactory.Configuration.GetSection("TodoListApi"));

var serviceProvider = tokenAcquirerFactory.Build();
var target = args.FirstOrDefault()?.ToLowerInvariant() ?? "all";

switch (target)
{
    case "graph":
        await CallMicrosoftGraphAsync(serviceProvider, tokenAcquirerFactory.Configuration);
        break;
    case "custom-api":
        await CallCustomApiAsync(serviceProvider);
        break;
    case "all":
        await CallMicrosoftGraphAsync(serviceProvider, tokenAcquirerFactory.Configuration);
        await CallCustomApiAsync(serviceProvider);
        break;
    default:
        throw new ArgumentException("Target must be 'graph', 'custom-api', or 'all'.");
}

static async Task CallMicrosoftGraphAsync(
    IServiceProvider serviceProvider,
    IConfiguration configuration)
{
    var scopes = configuration.GetSection("MicrosoftGraph:Scopes").Get<string[]>()
        ?? throw new InvalidOperationException("Configure MicrosoftGraph:Scopes in appsettings.json.");
    var userAssignedClientId = configuration["MicrosoftGraph:UserAssignedClientId"];

    var graphServiceClient = serviceProvider.GetRequiredService<GraphServiceClient>();
    var users = await graphServiceClient.Users.GetAsync(request =>
        request.Options.WithAppOnly().WithAuthenticationOptions(options =>
        {
            options.Scopes = scopes;
            options.AcquireTokenOptions.ManagedIdentity = new();

            if (!string.IsNullOrWhiteSpace(userAssignedClientId))
            {
                options.AcquireTokenOptions.ManagedIdentity.UserAssignedClientId = userAssignedClientId;
            }
        }));

    Console.WriteLine($"Microsoft Graph returned {users?.Value?.Count ?? 0} users.");
}

static async Task CallCustomApiAsync(IServiceProvider serviceProvider)
{
    var downstreamApi = serviceProvider.GetRequiredService<IDownstreamApi>();
    var todoItems = await downstreamApi.GetForAppAsync<IEnumerable<TodoItem>>("TodoListApi");

    Console.WriteLine($"The custom API returned {todoItems?.Count() ?? 0} todo items.");
}
