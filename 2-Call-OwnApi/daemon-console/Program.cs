// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using daemon_console.Models;
using daemon_console.Options;
using daemon_console.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// These objects fetch the configurations from the appsettings.json file.
var azureAdOptions = new AzureAdOptions();
configuration.GetSection(AzureAdOptions.AzureAd).Bind(azureAdOptions);

var downstreamApiOptions = new DownstreamApiOptions();
configuration.GetSection(DownstreamApiOptions.DownstreamApi).Bind(downstreamApiOptions);

// The ConfidentialClientApplicationService contains an instance of the IConfidentialClientApplication that is reused
// throughout this app. Create an instance of it here so that an access token can be retrieved from Azure and cached.
var confidentialClientApplicationService = new ConfidentialClientApplicationService(
    Options.Create<AzureAdOptions>(azureAdOptions),
    Options.Create<DownstreamApiOptions>(downstreamApiOptions));

Console.WriteLine("Acquiring access token...\n");

try {
    // If this is successful a token is acquired from Azure and cached for later use.
    await confidentialClientApplicationService.GetAccessTokenAsync();
}
catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS7000215"))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("The secret provided is not recognized by the app");
    Console.ResetColor();
    return;
}
catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS700027"))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Certificate used is not uploaded for this app");
    Console.ResetColor();
    return;
}
catch(Exception exception)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Exception was thrown\n");
    Console.WriteLine($"Message:\n{exception.Message}");

    Console.WriteLine($"\nStack trace:\n{exception.StackTrace}");
    Console.ResetColor();
    return;
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Token acquired and cached\n");
Console.ResetColor();

var services = new ServiceCollection();

services
    .AddSingleton<IConfidentialClientApplicationService>(confidentialClientApplicationService)
    .AddSingleton<IPostTodosService, PostTodosService>()
    .AddSingleton<IDataDisplayService, DataDisplayService>()
    .AddHttpClient<ITodoService, TodoService>(async (serviceProvider, httpClient) =>
    {
        httpClient.BaseAddress = new Uri(downstreamApiOptions.BaseUrl);

        // Reuse the same IConfidentialClientApplicationService to have access to the cached access token.
        var confidentialClientApplicationService = serviceProvider
            .GetRequiredService<IConfidentialClientApplicationService>();

        var accessToken = await confidentialClientApplicationService.GetAccessTokenAsync();

        httpClient.DefaultRequestHeaders
            .Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // The access token for the API is added to the request Authorization header.
        //
        // You can read more here:
        // https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-auth-code-flow#use-the-access-token
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    });

var serviceProvider = services.BuildServiceProvider();

var todoService = serviceProvider.GetService<ITodoService>();
var postTodosService = serviceProvider.GetService<IPostTodosService>();
var dataDisplayService = serviceProvider.GetService<IDataDisplayService>();

try
{
    await dataDisplayService.DisplayAllTodosAsync();

    var uploadedTodoIds = await postTodosService.UploadSampleTodosAsync();

    Console.WriteLine("Id's of uploaded to-do's\n");

    foreach (var id in uploadedTodoIds)
    {
        Console.WriteLine(id);
    }

    await dataDisplayService.DisplayAllTodosAsync();

    var singleTodoId = uploadedTodoIds.FirstOrDefault();

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

    var aDifferentTodoId = uploadedTodoIds.LastOrDefault();

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