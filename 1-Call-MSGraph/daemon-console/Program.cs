// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates; //Only import this if you are using certificate
using System.Threading.Tasks;

namespace daemon_console
{
    /// <summary>
    /// This sample shows how to query the Microsoft Graph from a daemon application
    /// which uses application permissions.
    /// For more information see https://aka.ms/msal-net-client-credentials
    /// </summary>
    class Program
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

            if (isUsingClientSecret)
            {
                app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                    .WithClientSecret(config.ClientSecret)
                    .WithAuthority(new Uri(config.Authority))
                    .Build();
            }

            else
            {

                // Check if the certificate is being loaded from a key vault. Consult the '3-Using-KeyVault' read me for more information.
                if (IsCertificateStoredInKeyVault(config))
                {
                    // Load the certificate
                    ICertificateLoader certificateLoader = new DefaultCertificateLoader();
                    certificateLoader.LoadIfNeeded(config.Certificate);

                    // Even if this is a console application here, a daemon application is a confidential client application
                    app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                        .WithCertificate(config.Certificate.Certificate)
                        .WithAuthority(new Uri(config.Authority))
                        .Build();
                }
                else
                {
                    X509Certificate2 certificate = ReadCertificate(config.CertificateName);
                    app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                        .WithCertificate(certificate)
                        .WithAuthority(new Uri(config.Authority))
                        .Build();
                }
            }

            app.AddInMemoryTokenCache();

            // With client credentials flows the scopes is ALWAYS of the shape "resource/.default", as the 
            // application permissions need to be set statically (in the portal or by PowerShell), and then granted by
            // a tenant administrator. 
            string[] scopes = new string[] { $"{config.ApiUrl}.default" }; // Generates a scope -> "https://graph.microsoft.com/.default"

            // Call MS Graph REST API directly
            await CallMSGraph(config, app, scopes);

            // Call MS graph using the Graph SDK
            await CallMSGraphUsingGraphSDK(app, scopes);
        }

        /// <summary>
        /// Calls MS Graph REST API using an authenticated Http client
        /// </summary>
        /// <param name="config"></param>
        /// <param name="app"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        private static async Task CallMSGraph(AuthenticationConfig config, IConfidentialClientApplication app, string[] scopes)
        {
            AuthenticationResult result = null;
            try
            {
                result = await app.AcquireTokenForClient(scopes)
                    .ExecuteAsync();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Token acquired");
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

            // The following example uses a Raw Http call 
            if (result != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = (JwtSecurityToken)tokenHandler.ReadToken(result.AccessToken);
                var tid = jwt.Claims.FirstOrDefault(c => c.Type == "tid")?.Value;
                var appId = jwt.Claims.FirstOrDefault(c => c.Type == "appid")?.Value;

                Console.WriteLine($"The ID of the tenant the application is hosted on: {tid}");
                Console.WriteLine($"The ID of the application this token is intended for: {appId}\n");

                var roles = jwt.Claims
                    .Where(c => c.Type == "roles")
                    .Select(c => c.Value);

                var tokenContainsAllRequiredRoles = config.RequiredRoles.All(r => roles.Contains(r));

                if (!tokenContainsAllRequiredRoles)
                {
                    throw new UnauthorizedAccessException("Token was issued with incorrect roles for application.\n\n" +
                    $"Expected Roles: {String.Join(", ", config.RequiredRoles)}\n" +
                    $"Roles on token: {String.Join(", ", roles)}");
                }

                var httpClient = new HttpClient();
                var apiCaller = new ProtectedApiCallHelper(httpClient);
                await apiCaller.CallWebApiAndProcessResultASync($"{config.ApiUrl}v1.0/users", result.AccessToken, Display);

            }
        }

        /// <summary>
        /// The following example shows how to initialize the MS Graph SDK
        /// </summary>
        /// <param name="app"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        private static async Task CallMSGraphUsingGraphSDK(IConfidentialClientApplication app, string[] scopes)
        {
            // Prepare an authenticated MS Graph SDK client
            GraphServiceClient graphServiceClient = GetAuthenticatedGraphClient(app, scopes);


            List<User> allUsers = new List<User>();

            try
            {

                IGraphServiceUsersCollectionPage users = await graphServiceClient.Users.Request().GetAsync();
                Console.WriteLine($"Found {users.Count()} users in the tenant"); 
            }
            catch (ServiceException e)
            {
                Console.WriteLine("We could not retrieve the user's list: " + $"{e}");
            }

        }

  
        /// <summary>
        /// An example of how to authenticate the Microsoft Graph SDK using the MSAL library
        /// </summary>
        /// <returns></returns>
        private static GraphServiceClient GetAuthenticatedGraphClient(IConfidentialClientApplication app, string[] scopes)
        {            

            GraphServiceClient graphServiceClient =
                    new GraphServiceClient("https://graph.microsoft.com/V1.0/", new DelegateAuthenticationProvider(async (requestMessage) =>
                    {
                        // Retrieve an access token for Microsoft Graph (gets a fresh token if needed).
                        AuthenticationResult result = await app.AcquireTokenForClient(scopes)
                            .ExecuteAsync();

                        // Add the access token in the Authorization header of the API request.
                        requestMessage.Headers.Authorization =
                            new AuthenticationHeaderValue("Bearer", result.AccessToken);
                    }));

            return graphServiceClient;
        }


        /// <summary>
        /// Display the result of the Web API call
        /// </summary>
        /// <param name="result">Object to display</param>
        private static void Display(JObject result)
        {
            foreach (JProperty child in result.Properties().Where(p => !p.Name.StartsWith("@")))
            {
                Console.WriteLine($"{child.Name} = {child.Value}");
            }
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
            string certificatePlaceholderValue = "[Or instead of client secret: Enter here the name of a certificate (from the user cert store) as registered with your application]";

            if (!String.IsNullOrWhiteSpace(config.ClientSecret) && config.ClientSecret != clientSecretPlaceholderValue)
            {
                return true;
            }

            else if ((!String.IsNullOrWhiteSpace(config.CertificateName) &&
                     config.CertificateName != certificatePlaceholderValue) ||
                     IsCertificateStoredInKeyVault(config))
            {
                return false;
            }

            else
                throw new Exception("You must choose between using client secret or certificate. Please update appsettings.json file.");
        }

        /// <summary>
        /// Checks if the application uses a certificate stored in a key vault.
        /// </summary>
        /// <param name="config">Configuration from appsettings.json</param>
        /// <returns></returns>
        private static bool IsCertificateStoredInKeyVault(AuthenticationConfig config)
        {
            string keyVaultUrlPlaceHolderText = "<VaultUri>";
            string keyVaultCertificateNamePlaceHolderText = "<CertificateName>";

            var keyVaultUrlIsSet = !String.IsNullOrWhiteSpace(config.Certificate.KeyVaultUrl) &&
                config.Certificate.KeyVaultUrl != keyVaultUrlPlaceHolderText;

            var keyVaultCertificateIsSet = !String.IsNullOrWhiteSpace(config.Certificate.KeyVaultCertificateName) &&
                config.Certificate.KeyVaultCertificateName != keyVaultCertificateNamePlaceHolderText;

            return keyVaultUrlIsSet && keyVaultCertificateIsSet;
        }

        private static X509Certificate2 ReadCertificate(string certificateName)
        {
            if (string.IsNullOrWhiteSpace(certificateName))
            {
                throw new ArgumentException("certificateName should not be empty. Please set the CertificateName setting in the appsettings.json", "certificateName");
            }

            CertificateDescription certificateDescription = CertificateDescription.FromStoreWithDistinguishedName(certificateName);
            DefaultCertificateLoader defaultCertificateLoader = new DefaultCertificateLoader();
            defaultCertificateLoader.LoadIfNeeded(certificateDescription);
            return certificateDescription.Certificate;
        }


    }
}
