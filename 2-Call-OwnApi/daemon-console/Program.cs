// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using daemon_console.Options;
using daemon_console.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


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
services.AddSingleton<IDataDisplayService, DataDisplayService>();

var serviceProvider = services.BuildServiceProvider();

var dataDisplayService = serviceProvider.GetService<IDataDisplayService>();

await dataDisplayService.DisplayAllTodosAsync();
