using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using System.Collections.Generic;
using System;
using System.Linq;
using TodoList_WebApi.Models;
using Microsoft.Extensions.Logging;

// Get the Token acquirer factory instance. By default it reads an appsettings.json
// file if it exists in the same folder as the app (make sure that the 
// "Copy to Output Directory" property of the appsettings.json file is "Copy if newer").
var tokenAcquirerFactory = TokenAcquirerFactory.GetDefaultInstance();

// Add console logging or other services if you wish
tokenAcquirerFactory.Services.AddLogging(
    (loggingBuilder) => loggingBuilder.SetMinimumLevel(LogLevel.Warning)
                                      .AddConsole()
                                        );

// Create a downstream API service named 'MyApi' which comes loaded with several
// utility methods to make HTTP calls to the DownstreamApi configurations found
// in the "MyWebApi" section of your appsettings.json file.
tokenAcquirerFactory.Services.AddDownstreamApi("MyApi",
    tokenAcquirerFactory.Configuration.GetSection("MyWebApi"));
var sp = tokenAcquirerFactory.Build();

// Extract the downstream API service from the 'tokenAcquirerFactory' service provider.
var api = sp.GetRequiredService<IDownstreamApi>();

// You can use the API service to make direct HTTP calls to your API. Token
// acquisition is handled automatically based on the configurations in your
// appsettings.json file.
var result = await api.GetForAppAsync<IEnumerable<TodoItem>>("MyApi");
Console.WriteLine($"result = {result?.Count()}");
