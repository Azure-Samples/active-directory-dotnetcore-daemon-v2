using Microsoft.AspNetCore.Mvc;
using ms_activedirectory_managedidentity.Models;
using System.Diagnostics;
using System.Net.Http.Headers;
using Microsoft.Identity.Client;
using System;

namespace ms_activedirectory_managedidentity.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory? _httpClientFactory;

        /// <summary>
        /// Home Controller
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="httpClientFactory"></param>
        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Gets secret from the Azure Key Vault
        /// </summary>
        /// <param name="userAssignedId">Optional parameter if you want to get a token for a user assigned mananaged identity</param>
        /// <returns></returns>
        public async Task<ActionResult> GetSecret([FromQuery(Name = "userAssignedId")] string? userAssignedId = null)
        {
            try
            {
                string resource = "https://vault.azure.net";
                var kvUri = "https://<your-key-vault-name>.vault.azure.net/";
                var secretName = "<secret name>"; 

                //Get a managed identity token using Microsoft Identity Client
                IManagedIdentityApplication mi = CreateManagedIdentityApplication(userAssignedId);
                var result = await mi.AcquireTokenForManagedIdentity(resource).ExecuteAsync().ConfigureAwait(false);
                var accessToken = result.AccessToken;

                //create an HttpClient using IHttpClientFactory
                HttpClient httpClient = _httpClientFactory.CreateClient();

                //Use the access token to read secrets from the key vault 
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                var response = await httpClient.GetAsync($"{kvUri}/secrets/{secretName}?api-version=7.2");
                var secretValue = await response.Content.ReadAsStringAsync();

                ViewBag.Message = secretValue;
                return View();
            }
            catch (MsalException ex)
            {
                ViewBag.Title = "MsalException Thrown!!!";
                ViewBag.Error = "MsalException";
                ViewBag.Message = ex.Message;
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Title = "Exception Thrown!!!";
                ViewBag.Error = "Exception";
                ViewBag.Message = ex.Message;
                return View();
            }
        }

        private static IManagedIdentityApplication CreateManagedIdentityApplication(string? userAssignedId)
        {
            if (userAssignedId == null)
            {
                return ManagedIdentityApplicationBuilder.Create()
                    .WithExperimentalFeatures()
                    .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
                    .Build();
            }
            else
            {
                return ManagedIdentityApplicationBuilder.Create(userAssignedId)
                    .WithExperimentalFeatures()
                    .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
                    .Build();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}