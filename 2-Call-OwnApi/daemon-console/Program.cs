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

// These options are populated with data from the appsettings.json file and provided to the services.
services.AddOptions<AzureAdOptions>()
    .Configure(azureAdOptions =>
        configuration.GetSection(AzureAdOptions.AzureAd).Bind(azureAdOptions));

services.AddOptions<DownstreamApiOptions>()
    .Configure(downstreamApiOptions =>
        configuration.GetSection(DownstreamApiOptions.DownstreamApi).Bind(downstreamApiOptions));

services.AddTransient<AccessTokenHandler>();

services.AddSingleton<IConfidentialClientApplicationService, ConfidentialClientApplicationService>();
services.AddSingleton<IPostTodosService, PostTodosService>();
services.AddSingleton<IDataDisplayService, DataDisplayService>();

// Inject a custom HttpClient to the services using the API url set in the appsettings.json file and provide the
// AccessTokenHandler as a message handler to include access tokens with each request.
services.AddHttpClient<ITodoService, TodoService>((serviceProvider, httpClient) =>
    {
        httpClient.BaseAddress = new Uri(configuration["DownStreamApi:BaseUrl"]);
    })
    .AddHttpMessageHandler<AccessTokenHandler>();

var serviceProvider = services.BuildServiceProvider();

var todoService = serviceProvider.GetService<ITodoService>();
var postTodosService = serviceProvider.GetService<IPostTodosService>();
var dataDisplayService = serviceProvider.GetService<IDataDisplayService>();

try
{
    await dataDisplayService.DisplayAllTodosAsync();

    var postedTodoIds = await postTodosService.UploadSampleTodosAsync();

    Console.WriteLine("Id's of uploaded to-do's\n");

    foreach (var id in postedTodoIds)
    {
        Console.WriteLine(id);
    }

    await dataDisplayService.DisplayAllTodosAsync();

    var singleTodoId = postedTodoIds.FirstOrDefault();

    if (singleTodoId == Guid.Empty)
    {
        Console.WriteLine("No to-do's uploaded.");
        return;
    }

    await dataDisplayService.DisplayTodoAsync(singleTodoId);

    await dataDisplayService.DisplayTodoAsync(await todoService.UpdateTodoAsync(singleTodoId, new Todo()
    {
        UserId = Guid.NewGuid(),
        Title = "Something else.",
        Owner = "Carol"
    }));

    var aDifferentTodoId = postedTodoIds.LastOrDefault();

    if (aDifferentTodoId == singleTodoId || aDifferentTodoId == Guid.Empty)
    {
        Console.WriteLine("Unable tor retrieve the ID of another to-do.");
        return;
    }

    await dataDisplayService.DisplayTodoAsync(aDifferentTodoId);

    await todoService.DeleteTodoAsync(aDifferentTodoId);

    Console.WriteLine("Attempting to retrieve deleted to-do\n");

    try
    {
        await todoService.GetTodoAsync(aDifferentTodoId);
    }
    catch (HttpRequestException exception)
    {
        Console.WriteLine(exception.Message);
    }

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
