// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Graph;
using System.Net.Http.Headers;
using System.Collections.Generic;

namespace daemon_console
{
    /// <summary>
    /// This sample shows how to query the Microsoft Graph from a daemon application
    /// which uses application permissions.
    /// For more information see https://aka.ms/msal-net-client-credentials
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                RunAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static async Task RunAsync()
        {
            AuthenticationConfig config = AuthenticationConfig.ReadFromJsonFile("appsettings.json");

            // You can run this sample using ClientSecret or Certificate. The code will differ only when instantiating the IConfidentialClientApplication
            bool isUsingClientSecret = IsAppUsingClientSecret(config);

            // Even if this is a console application here, a daemon application is a confidential client application
            IConfidentialClientApplication app;
            IEnumerable<User> users = Array.Empty<User>();

            if (isUsingClientSecret)
            {
                // Even if this is a console application here, a daemon application is a confidential client application
                app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                    .WithClientSecret(config.ClientSecret)
                    .WithAuthority(new Uri(config.Authority))
                    .Build();

                // Call MS graph using the Graph SDK
                users = await CallMSGraphUsingGraphSDK(app, new[] { "https://graph.microsoft.com/.default" });

                foreach (User user in users)
                {
                    Console.WriteLine($"{user.Id}, {user.DisplayName}, {user.UserPrincipalName}");
                }
            }
            else
            {
                ICertificateLoader certificateLoader = new DefaultCertificateLoader();
                certificateLoader.LoadIfNeeded(config.Certificate);

                app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                    .WithCertificate(config.Certificate.Certificate)
                    .WithAuthority(new Uri(config.Authority))
                    .Build();
            }

            app.AddInMemoryTokenCache();

            // With client credentials flows the scopes is ALWAYS of the shape "resource/.default", as the
            // application permissions need to be set statically (in the portal or by PowerShell), and then granted by
            // a tenant administrator
            string[] scopes = new string[] { config.TodoListScope };
            AuthenticationResult result = null;

            try
            {
                result = await app.AcquireTokenForClient(scopes)
                    .ExecuteAsync();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Token acquired for TodoList API\n");
                Console.ResetColor();
            }
            catch (MsalServiceException ex) when (ex.Message.Contains("AADSTS70011"))
            {
                // Invalid scope. The scope has to be of the form "https://resourceurl/.default"
                // Mitigation: change the scope to be as expected
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Scope provided is not supported");
                Console.ResetColor();
            }

            if (result != null)
            {
                var httpClient = new HttpClient();
                var apiCaller = new ProtectedApiCallHelper(httpClient);

                IEnumerable<daemon_console.Models.Todo> todosToUpload = users
                    .Select(user => new daemon_console.Models.Todo()
                    {
                        Owner = user.Id,
                        Task = "Sample task",
                    });

                await apiCaller.GenerateUserTodos($"{config.TodoListBaseAddress}/api/todolist", result.AccessToken, todosToUpload, DisplayTwo);

                //await apiCaller.CallWebApiAndProcessResultASync($"{config.TodoListBaseAddress}/api/date", result.AccessToken, Display);
                await apiCaller.CallWebApiAndProcessResultASync($"{config.TodoListBaseAddress}/api/todolist", result.AccessToken, Display);
            }
        }

        /// <summary>
        /// Display the result of the Web API call
        /// </summary>
        /// <param name="result">Object to display</param>
        private static void Display(JsonNode result)
        {
            Console.WriteLine("Web Api result: \n");

            JsonArray nodes = result.AsArray();

            foreach (JsonObject aNode in nodes.ToArray())
            {
                foreach (var property in aNode.ToArray())
                {
                    Console.WriteLine($"{property.Key} = {property.Value?.ToString()}");
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Display the result of the Web API call
        /// </summary>
        /// <param name="result">Object to display</param>
        private static void DisplayTwo(JsonNode result)
        {
            Console.WriteLine("Web Api result: \n");

            JsonObject todo = result.AsObject();

            foreach (var property in todo.ToArray())
            {
                Console.WriteLine($"{property.Key} = {property.Value?.ToString()}");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Checks if the sample is configured for using ClientSecret or Certificate. This method is just for the sake of this sample.
        /// You won't need this verification in your production application since you will be authenticating in AAD using one mechanism only.
        /// </summary>
        /// <param name="config">Configuration from appsettings.json</param>
        /// <returns></returns>
        private static bool IsAppUsingClientSecret(AuthenticationConfig config)
        {
            string clientSecretPlaceholderValue = "[Enter here a client secret for your application]";

            if (!String.IsNullOrWhiteSpace(config.ClientSecret) && config.ClientSecret != clientSecretPlaceholderValue)
            {
                return true;
            }
            else if (config.Certificate != null)
            {
                return false;
            }
            else
                throw new Exception("You must choose between using client secret or certificate. Please update appsettings.json file.");
        }

        /// <summary>
        /// The following example shows how to initialize the MS Graph SDK
        /// </summary>
        /// <param name="app"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        private static async Task<List<User>> CallMSGraphUsingGraphSDK(IConfidentialClientApplication app, string[] scopes)
        {
            // Prepare an authenticated MS Graph SDK client
            GraphServiceClient graphServiceClient = GetAuthenticatedGraphClient(app, scopes);
            string userSelectFields = "id,displayName,givenName,surname,mail,mailNickname,userPrincipalName,userType,jobTitle,accountEnabled,country,createdDateTime";

            List<User> allUsers = new List<User>();

            try
            {
                IGraphServiceUsersCollectionPage usersPage = await graphServiceClient.Users.Request().Select(userSelectFields).Top(20).GetAsync();

                allUsers = await CollectionProcessor<User>.ProcessGraphCollectionPageAsync(graphServiceClient, usersPage);
                Console.WriteLine($"Found {allUsers.Count()} users in the tenant");
            }
            catch (ServiceException e)
            {
                Console.WriteLine("We could not retrieve the user's list: " + $"{e}");
            }
            return allUsers;
        }

        /// <summary>
        /// An example of how to authenticate the Microsoft Graph SDK using the MSAL library
        /// </summary>
        /// <returns></returns>
        private static GraphServiceClient GetAuthenticatedGraphClient(IConfidentialClientApplication app, string[] scopes)
        {
            GraphServiceClient graphServiceClient = new GraphServiceClient("https://graph.microsoft.com/v1.0/",
                new DelegateAuthenticationProvider(async requestMessage =>
                {
                    // Retrieve an access token for Microsoft Graph (gets a fresh token if needed).
                    var result = await app.AcquireTokenForClient(scopes)
                        .ExecuteAsync();

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Token acquired for Microsoft Graph\n");
                    Console.ResetColor();

                    // Add the access token in the Authorization header of the API request.
                    requestMessage.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", result.AccessToken);

                }));

            return graphServiceClient;
        }
    }
}