// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System;
using System.Threading.Tasks;

namespace daemon_console
{
    /// <summary>
    /// This sample shows how to query the Microsoft Graph from a daemon application using a managed identity.
    /// </summary>
    class Program
    {
        static async Task Main(string[] _)
        {
            // Get the Token acquirer factory instance. By default it reads an appsettings.json
            // file if it exists in the same folder as the app (make sure that the 
            // "Copy to Output Directory" property of the appsettings.json file is "Copy if newer").
            TokenAcquirerFactory tokenAcquirerFactory = TokenAcquirerFactory.GetDefaultInstance();

            // Configure the application options to be read from the configuration
            // and add the services you need (Graph, token cache)
            var services = tokenAcquirerFactory.Services;
            services.AddMicrosoftGraph();
            // By default, you get an in-memory token cache.
            // For more token cache serialization options, see https://aka.ms/msal-net-token-cache-serialization

            // Resolve the dependency injection.
            var serviceProvider = tokenAcquirerFactory.Build();

            // Call Microsoft Graph using the Graph SDK
            try
            {
                GraphServiceClient graphServiceClient = serviceProvider.GetRequiredService<GraphServiceClient>();
                var users = await graphServiceClient.Users
                    .GetAsync(r => r.Options.WithAppOnly()
                        .WithAuthenticationOptions(o =>
                            {
                                // Specify your target Microsoft Graph endpoint as the scope for the request.
                                // A list of Microoft Graph endpoints can be found at https://docs.microsoft.com/graph/deployments#microsoft-graph-and-graph-explorer-service-root-endpoints
                                o.Scopes = new string[] { "https://graph.microsoft.com/.default" };
                                // Tell the library to use a managed identity, by default it uses the system-assigned managed identity.
                                o.AcquireTokenOptions.ManagedIdentity = new()
                                {
                                // Uncomment the below line and edit the value to use a user-assigned managed identity.
                                // UserAssignedClientId = "ClientID-of-the-user-assigned-managed-identity"
                                };
                        })                   
                    );
                Console.WriteLine($"{users.Value.Count} users");
            }
            catch (ServiceException e)
            {
                Console.WriteLine("We could not retrieve the user's list: " + $"{e}");

                // If you get the following exception, here is what you need to do
                // ---------------------------------------------------------------
                //  IDW10503: Cannot determine the cloud Instance.
                //    Provide the configuration (appsettings.json with an "AzureAd" section, and "Instance" set,
                //    the project needs to be this way)
                // <ItemGroup>
                //  < None Update = "appsettings.json" >
                //    < CopyToOutputDirectory > PreserveNewest </ CopyToOutputDirectory >
                //  </ None >
                // </ ItemGroup >
            }
        }
    }
}