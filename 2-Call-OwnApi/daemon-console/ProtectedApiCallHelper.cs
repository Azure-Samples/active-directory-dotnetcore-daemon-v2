// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using daemon_console.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace daemon_console
{
    /// <summary>
    /// Helper class to call a protected API and process its result
    /// </summary>
    public class ProtectedApiCallHelper
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClient">HttpClient used to call the protected API</param>
        public ProtectedApiCallHelper(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        protected HttpClient HttpClient { get; private set; }

        /// <summary>
        /// Calls the protected web API and processes the result
        /// </summary>
        /// <param name="webApiUrl">URL of the web API to call (supposed to return Json)</param>
        /// <param name="accessToken">Access token used as a bearer security token to call the web API</param>
        /// <param name="processResult">Callback used to process the result of the call to the web API</param>
        public async Task GetAllTodosFromApiAndProcessResultAsync(string webApiUrl, string accessToken)
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                PrepareAuthenticatedRequest(accessToken);

                HttpResponseMessage response = await HttpClient.GetAsync(webApiUrl);
                PrintServerResponse(response);
            }
        }

        /// <summary>
        /// Uploads user to-dos to the protected web API and processes the result
        /// </summary>
        /// <param name="webApiUrl">URL of the web API to call (supposed to return Json)</param>
        /// <param name="accessToken">Access token used as a bearer security token to call the web API</param>
        /// <param name="todos">ToDos to be loaded to the API</param>
        public async Task PostToDoForUser(string webApiUrl, string accessToken, IEnumerable<Todo> todos)
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                PrepareAuthenticatedRequest(accessToken);

                foreach (var todo in todos)
                {
                    Console.WriteLine($"Uploading ToDo with task '{todo.Task}' for owner '{todo.Owner}'");
                    HttpResponseMessage response = await HttpClient.PostAsJsonAsync(webApiUrl, todo);
                    PrintServerResponse(response);
                }
            }
        }

        /// <summary>
        /// Prepares the request headers to have the 'Bearer' value set to the provided access token
        /// </summary>
        /// <param name="accessToken">Access token used as a bearer security token to call the web API</param>
        private void PrepareAuthenticatedRequest(string accessToken)
        {
            var defaultRequestHeaders = HttpClient.DefaultRequestHeaders;
            if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
            {
                HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        /// <summary>
        /// Prints the server response to the console
        /// </summary>
        /// <param name="response">The response retrieved from the server</param>
        /// <param name="processResult">Callback used to process the result of the call to the web API</param>
        private async void PrintServerResponse(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();
                using var result = JsonDocument.Parse(json);
                Console.ForegroundColor = ConsoleColor.Gray;

                string formattedJson = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });

                Console.WriteLine("Web API call result:");
                Console.WriteLine(formattedJson);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to call the web API: {response.StatusCode}");
                string content = await response.Content.ReadAsStringAsync();

                // Note that if you got reponse.Code == 403 and reponse.content.code == "Authorization_RequestDenied"
                // this is because the tenant admin as not granted consent for the application to call the Web API
                Console.WriteLine($"Content: {content}");
            }

            Console.ResetColor();
        }
    }
}