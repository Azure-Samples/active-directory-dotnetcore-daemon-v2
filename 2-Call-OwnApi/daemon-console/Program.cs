// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using daemon_console.Models;
using daemon_console.Options;
using daemon_console.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var services = new ServiceCollection();

services.AddOptions<AzureAdOptions>()
    .Configure(azureAdOptions =>
        configuration.GetSection(AzureAdOptions.AzureAd).Bind(azureAdOptions));

services.AddOptions<DownstreamApiOptions>()
    .Configure(downstreamApiOptions =>
        configuration.GetSection(DownstreamApiOptions.DownstreamApi).Bind(downstreamApiOptions));

services.AddSingleton<IConfidentialClientApplicationService, ConfidentialClientApplicationService>();
services.AddSingleton<ITodoService, TodoService>();
services.AddSingleton<IUploadTodosService, UploadTodosService>();
services.AddSingleton<IDataDisplayService, DataDisplayService>();

var serviceProvider = services.BuildServiceProvider();

var todoService = serviceProvider.GetService<ITodoService>();
var uploadTodosService = serviceProvider.GetService<IUploadTodosService>();
var dataDisplayService = serviceProvider.GetService<IDataDisplayService>();

try
{
    Console.WriteLine("\nChecking all to-do's currently in API store\n");

    await dataDisplayService.DisplayAllTodosAsync();

    Console.WriteLine("Uploading to-do's to API store\n");

    var uploadedTodoIds = await uploadTodosService.UpoloadTodos();

    Console.WriteLine("Id's of uploaded to-do's\n");

    foreach (var id in uploadedTodoIds)
    {
        Console.WriteLine(id);
    }

    Console.WriteLine("\nChecking all to-do's currently in API store\n");

    await dataDisplayService.DisplayAllTodosAsync();

    var singleTodoId = uploadedTodoIds.FirstOrDefault();

    if (singleTodoId == Guid.Empty)
    {
        Console.WriteLine("No to-do's uploaded.");
        return;
    }

    Console.WriteLine("Getting single to-do\n");

    await dataDisplayService.DisplayTodoAsync(singleTodoId);

    Console.WriteLine("Editing single to-do\n");

    await dataDisplayService.DisplayTodoAsync(await todoService.UpdateAsync(singleTodoId, new Todo()
    {
        UserId = Guid.NewGuid(),
        Title = "Something else.",
        Owner = "Carol"
    }));

    var aDifferentTodoId = uploadedTodoIds.LastOrDefault();

    if (aDifferentTodoId == singleTodoId || aDifferentTodoId == Guid.Empty)
    {
        Console.WriteLine("Unable tor retrieve the ID of another to-do.");
        return;
    }

    Console.WriteLine("Getting another to-do\n");

    await dataDisplayService.DisplayTodoAsync(aDifferentTodoId);

    Console.WriteLine("Deleteing this to-do\n");

    await todoService.DeleteAsync(aDifferentTodoId);

    Console.WriteLine("Attempting to retrieve deleted to-do\n");

    try
    {
        await todoService.GetAsync(aDifferentTodoId);
    }
    catch (HttpRequestException exception)
    {
        Console.WriteLine(exception.Message);
    }

    Console.WriteLine("Checking all to-do's currently in API store\n");

    await dataDisplayService.DisplayAllTodosAsync();

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