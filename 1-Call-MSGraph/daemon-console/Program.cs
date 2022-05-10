// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using daemon_console.Options;
using daemon_console.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AzureAdOptions>(
    builder.Configuration.GetSection(AzureAdOptions.AzureAd));

builder.Services.Configure<DownstreamApiOptions>(
    builder.Configuration.GetSection(DownstreamApiOptions.DownstreamApi));

builder.Services.AddSingleton<IConfidentialClientApplicationService, ConfidentialClientApplicationService>();
builder.Services.AddSingleton<IGraphUserService, GraphUserService>();
builder.Services.AddSingleton<IDataDisplayService, DataDisplayService>();

var app = builder.Build();

var dataDisplayService = app.Services.GetService<IDataDisplayService>();

if (dataDisplayService is null)
{
    throw new Exception("DataDisplayService not registered with services.");
}

await dataDisplayService.DisplayAllUsersAsync();

Console.WriteLine("Press any key to exit");
Console.ReadKey();
