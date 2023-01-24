// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.TokenCacheProviders.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace daemon_console
{
    /// <summary>
    /// This sample shows how to query the Microsoft Graph from a daemon application
    /// </summary>
    class Program
    {
        static async Task Main(string[] _)
        {
            // Get the Token acquirer factory instance. By default it reads an appsettings.json
            // file if it exists in the project.
            TokenAcquirerFactory tokenAcquirerFactory = TokenAcquirerFactory.GetDefaultInstance();

            // Configure the application options to be read from the configuration
            // and add the services you need (Graph, token cache)
            IServiceCollection services = tokenAcquirerFactory.Services;
            services.Configure<MicrosoftIdentityApplicationOptions>(
                      option => tokenAcquirerFactory.Configuration.GetSection("AzureAd").Bind(option))
                    .AddMicrosoftGraph();
            // By default, you get an in-memory token cache.
            // For more token cache serialization options, see https://aka.ms/msal-net-token-cache-serialization

            // Resolve the dependency injection.
            var serviceProvider = tokenAcquirerFactory.Build();

            // Call Microsoft Graph using the Graph SDK
            try
            {
                GraphServiceClient graphServiceClient = serviceProvider.GetRequiredService<GraphServiceClient>();

                string userSelectFields = "id,displayName,userPrincipalName";
                IGraphServiceUsersCollectionPage usersPage = await graphServiceClient
                    .Users
                    .Request()
                    .WithAppOnly()
                    .Select(userSelectFields)
                    .GetAsync();

                var users = await CollectionProcessor<User>.ProcessGraphCollectionPageAsync(graphServiceClient, usersPage, 20);

                foreach (User user in users)
                {
                    Console.WriteLine($"{user.Id}, {user.DisplayName}, {user.UserPrincipalName}");
                }

                var todoListApiOptions = tokenAcquirerFactory.Configuration.GetSection("AzureAd:DownstreamApis")
                    .Get<MicrosoftGraphOptions>();

                var tokenAcquirer = tokenAcquirerFactory.GetTokenAcquirer();
                var acquireTokenResult = await tokenAcquirer.GetTokenForAppAsync(todoListApiOptions.Scopes);


                // We got a list of users from the tenant
                // Now that we also have an Access token for the ToDoList API,  we proceed to generate random ToDos 
                if (acquireTokenResult != null)
                {
                    var httpClient = new HttpClient();
                    var apiCaller = new ProtectedApiCallHelper(httpClient);

                    IEnumerable<Models.Todo> todosToUpload = users
                        .Select(user => new Models.Todo()
                        {
                            Owner = user.Id,
                            Task = $"A To-Do for: {user.DisplayName}.",
                        });

                    await apiCaller.PostToDoForUserAsync($"{todoListApiOptions.BaseUrl}/api/todolist", acquireTokenResult.AccessToken, todosToUpload);

                    // Fetch the list of ToDos and print them
                    await apiCaller.GetAllTodosFromApiAndProcessResultAsync($"{todoListApiOptions.BaseUrl}/api/todolist", acquireTokenResult.AccessToken);
                }
            }
            catch (ServiceException e)
            {
                Console.WriteLine("We could not retrieve the user's list: " + $"{e}");

                // If you get the following exception, here is what you need to do
                // ---------------------------------------------------------------
                //  IDW10503: Cannot determine the cloud Instance.
                //    Provide the configuration (appsettings.json with an "AzureAd" section, and "Instance" set)
                // System.ArgumentNullException: Value cannot be null. (Parameter 'tenantId')
                //    Provide the TenantId in the configuration
                // Microsoft.Identity.Client.MsalClientException: No ClientId was specified.
                //    Provide the ClientId in the configuration
                // ErrorCode: Client_Credentials_Required_In_Confidential_Client_Application
                //    Provide a ClientCredentials section containing either a client secret, or a certificate
                //    or Pod identity if you run in AKS
            }
        }
    }
}
