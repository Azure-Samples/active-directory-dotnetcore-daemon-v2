// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using System;
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
            // file if it exists in the same folder as the app (make sure that the 
            // "Copy to Output Directory" property of the appsettings.json file is "Copy if newer").
            TokenAcquirerFactory tokenAcquirerFactory = TokenAcquirerFactory.GetDefaultInstance();

            // Configure the application options to be read from the configuration
            // and add the services you need (Graph, token cache)
            IServiceCollection services = tokenAcquirerFactory.Services;
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
                    .GetAsync(r => r.Options.WithAppOnly());
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
                // System.ArgumentNullException: Value cannot be null. (Parameter 'tenantId')
                //    Provide the TenantId in the configuration
                // Microsoft.Identity.Client.MsalClientException: No ClientId was specified.
                //    Provide the ClientId in the configuration
                // ErrorCode: Client_Credentials_Required_In_Confidential_Client_Application
                //    Provide a ClientCredentials section containing either a client secret, or a certificate
                //    or workload identity federation for Kubernates if your app runs in AKS
            }
        }
    }
}
