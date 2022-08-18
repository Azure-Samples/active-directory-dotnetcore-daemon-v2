---
page_type: sample
name: A .NET Core daemon console application calling a custom protected Web API with its own identity
services: active-directory
platforms: dotnet
urlFragment: active-directory-dotnetcore-daemon-v2
description: 
---

# A .NET Core daemon console application calling a custom protected Web API with its own identity

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/active-directory-dotnetcore-daemon-v2%20CI)

Table Of Contents

* [Scenario](#Scenario)
* [Prerequisites](#Prerequisites)
* [Setup the sample](#Setup-the-sample)
* [Contributing](#Contributing)
* [Learn More](#Learn-More)
* [Next Steps](#Next-Steps)
* [Community Help and Support](#Community-Help-and-Support)
* [Contributing](#Contributing)
* [More information](#More-information)

## Scenario

### Overview

This sample application shows how to use the [Microsoft identity platform](https://aka.ms/aadv2) to access data from a protected Web API using a non-interactive process.  It uses the [OAuth 2 client credentials grant](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow) to acquire an [access token](https://aka.ms/access-tokens) which is then used to request data from a protected API. It also lays out all the steps a developer would need to secure their Web APIs with the [Microsoft identity platform](https://aka.ms/aadv2).

The app is a .NET Core console application that can perform create, read, update and delete (CRUD) operations on a simple to-do API. The to-do API is protected using the Microsoft Authentication Library for .NET ([MSAL.NET](https://aka.ms/msal-net)) and requires access tokens provided by **Azure AD** for all incoming requests.

### Accessing API with client secret

![Topology](./ReadmeFiles/daemon-with-secret.svg)

### Accessing API with certificate

![Topology](./ReadmeFiles/daemon-with-certificate.svg)

## Prerequisites

* Either [Visual Studio](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/download) and [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
* An **Azure AD** tenant. You can find more information for setting up an Azure AD tenant [here](https://docs.microsoft.com/azure/active-directory/develop/test-setup-environment#get-a-test-tenant)
* A user account in your **Azure AD** tenant. This sample will not work with a **personal Microsoft account**.  You can read more about creating and removing users from your **Azure AD** tenant [here](https://docs.microsoft.com/azure/active-directory/fundamentals/add-users-azure-active-directory).

## Setup the sample

### Step 1:  Clone or download this repository

From your shell or command line:

```Shell
git clone https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2.git
```

or [download and extract the repository .zip file](https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2/archive/refs/heads/master.zip).

> Given the length of the project name and referenced NuGet packages, it might be a good idea to clone the project in a folder close to the root of your hard drive to avoid file size limitations on Windows.

Navigate to the `2-Call-OwnApi` folder

```Shell
cd 2-Call-OwnApi
```

### Step 2:  Register the sample with your Azure Active Directory tenant

There are two projects in this sample that need to be registered with **Azure AD**. You can automate these steps by following the steps below but if you wish to manually register the application yourself skip ahead to [Step 3:  Configure the sample to use your Azure AD tenant](#choose-the-azure-ad-tenant-where-you-want-to-create-your-applications).

1. On Windows, run PowerShell and navigate to the root of the cloned directory
1. In PowerShell run:

   ```PowerShell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
   ```

1. Navigate to the `AppCreationScripts` folder and run the script to create your Azure AD application and configure the code of the sample application accordingly.

   ```PowerShell
   # If you want to register your application with a certificate instead of
   # a client secret navigate to the 'AppCreationScripts-withCert' folder 
   # instead
   #
   # AppCreationScripts-withCert
   cd AppCreationScripts
   .\Configure.ps1
   ```

   > Other ways of running the scripts are described in [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md)

1. [Run the sample](#step-4-run-the-sample).

#### Choose the Azure AD tenant where you want to create your applications

As a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com) using either a work or school account or a personal Microsoft account.
1. If your account is present in more than one Azure AD tenant, select `Directory + Subscription` at the top right corner in the menu on top of the page, and switch your portal session to the desired Azure AD tenant.
1. In the left-hand navigation pane, select the **Azure Active Directory** service, and then select **App registrations**.

#### Register the service app (TodoList-webapi-daemon-v2)

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
1. When the **Register an application page** appears, enter your application's registration information:
   * In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `TodoList-webapi-daemon-v2`.
   * Leave **Supported account types** on the default setting of **Accounts in this organizational directory only**.
1. Select **Register** to create the application.
1. On the app **Overview** page, find the **Application (client) ID** value and record it for later. You'll need it to configure the Visual Studio configuration file for this project.
1. Select the **Expose an API** section:
    * Use the 'Set' button to generate the default AppID URI in the form of `api://<web api client id>`
   > If your tenant went through [domain verification](https://docs.microsoft.com/azure/active-directory/develop/howto-configure-publisher-domain) and you have verified domains available, you can use an AppID URI in the form of `https://<yourdomain>` or `https://<yourdomain>/<myAPI name>`  as well.
   * Click **Save**
1. Select the **Manifest** section, and:
   * Edit the manifest by locating the `appRoles`.  The role definition is provided in the JSON code block below. Leave the `allowedMemberTypes` to **Application** only. Each role definition in this manifest must have a different valid **Guid** for the "id" property.
   * Save the manifest.

The content of `appRoles` should be the following (the `id` can be any unique **Guid**)

```Json
{
  ...
    "appRoles": [
      {
          "allowedMemberTypes": [ "Application" ],
          "description": "An application permissions that gives you read access for all to-do's",
          "displayName": "Todo.Read.All",
          "id": "713e491e-7433-4e4e-85b2-a910a196f935",
          "isEnabled": true,
          "lang": null,
          "origin": "Application",
          "value": "Todo.Read.All"
      },
      {
          "allowedMemberTypes": [ "Application" ],
          "description": "An application permissions that gives you read and write access for all to-do's",
          "displayName": "Todo.ReadWrite.All",
          "id": "2abb8697-1162-4459-92b7-235f0ff8743c",
          "isEnabled": true,
          "lang": null,
          "origin": "Application",
          "value": "Todo.ReadWrite.All"
      }
    ],
 ...
}
```

#### Register the client app (daemon-console)

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
   * In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `daemon-console-v2`.
   * In the **Supported account types** section, select **Accounts in this organizational directory only ({tenant name})**.
   * Select **Register** to create the application.
1. On the app **Overview** page, find the **Application (client) ID** value and record it for later. You'll need it to configure the Visual Studio configuration file for this project.
1. From the **Certificates & secrets** page, in the **Client secrets** section, choose **New client secret**:

   * Type a key description (of instance `app secret`),
   * Select a key duration of either **In 1 year**, **In 2 years**, or **Never Expires**.
   * When you press the **Add** button, the key value will be displayed, copy, and save the value in a safe location.
   * You'll need this key later to configure the project in Visual Studio. This key value will not be displayed again, nor retrievable by any other means,
     so record it as soon as it is visible from the Azure portal.
1. In the list of pages for the app, select **API permissions**
   * Click the **Add a permission** button and then,
   * Ensure that the **My APIs** tab is selected
   * Select the API created in the previous step, for example `TodoList-webapi-daemon-v2`
   * In the **Application permissions** section, ensure that the right permissions are checked: **Todo.Read.All**, **Todo.ReadWrite.All**
   * Select the **Add permissions** button
1. At this stage permissions are assigned correctly but the client app does not allow interaction.
   Therefore no consent can be presented via a UI and accepted to use the service app.
   Click the **Grant/revoke admin consent for {tenant}** button, and then select **Yes** when you are asked if you want to grant consent for the
   requested permissions for all account in the tenant.
   You need to be an Azure AD tenant admin to do this.

### Step 3:  Configure the sample to use your Azure AD tenant

#### Configure the service project

> Note: if you used the setup scripts, the changes below will have been applied for you

1. Open the `TodoList-WebApi\appsettings.json` file
1. Find the app key `Domain` in the `AzureAd` section and replace the existing value with your Azure AD tenant name.
1. Find the app key `TenantId` in the `AzureAd` section and replace the existing value with your Azure AD tenant ID.
1. Find the app key `ClientId` in the `AzureAd` section and replace the existing value with the application ID (client ID) of the `TodoList-webapi-daemon-v2` application copied from the Azure portal.

#### Configure the client project

1. Open the `daemon-console\appsettings.json` file
1. If you are connecting to a national cloud, change the instance to the correct **Azure AD** endpoint. [See this reference for a list of Azure AD endpoints.](https://docs.microsoft.com/graph/deployments#app-registration-and-token-service-root-endpoints)
1. Find the app key `TenantId`  in the `AzureAd` section and replace the existing value with your Azure AD tenant name.
1. Find the app key `ClientId` in the `AzureAd` section and replace the existing value with the application ID (clientId) of the `daemon-console-v2` application copied from the Azure portal.
1. Find the app key `ClientSecret` in the `AzureAd` section and replace the existing value with the key you saved during the creation of the `daemon-console-v2` app in the Azure portal.
   * If you wish to use a certificate skip see the [Add the certificate for the application in Azure AD](#add-the-certificate-for-the-application-in-azure-ad) section below.
1. Find the app key `BaseUrl` in the `DownStreamApi` section and set it to `https://localhost:44372/`
1. Find the app key `Scopes` in the `DownStreamApi` section and replace the existing value with the **App ID URI** of your web API. It should follow the format below replacing `YOUR_APP_ID` with the **App ID** of your `TodoList-webapi-daemon-v2` project:

    * `"api://{YOUR_APP_ID}/.default"`

#### (Optional) Create a self-signed certificate

  To complete this step, you will use the `New-SelfSignedCertificate` Powershell command. You can find more information about the New-SelfSignedCertificate command [here](https://docs.microsoft.com/powershell/module/pkiclient/new-selfsignedcertificate).
  
  1. Open PowerShell and run `New-SelfSignedCertificate` with the following parameters to create a self-signed certificate in the user certificate store on your computer:
  
      ```PowerShell
      $cert=New-SelfSignedCertificate -Subject "CN=daemon-console-v2" -CertStoreLocation "Cert:\CurrentUser\My"  -KeyExportPolicy Exportable -KeySpec Signature
      ```
  
  1. Export this certificate using the "Manage User Certificate" MMC snap-in accessible from the Windows Control Panel. You can also add other options to generate the certificate in a different
  store such as the Computer or service store (See [How to: View Certificates with the MMC Snap-in](https://docs.microsoft.com/dotnet/framework/wcf/feature-details/how-to-view-certificates-with-the-mmc-snap-in)).
  
  You can use an existing certificate if you have one. You'll need the certificate name for the next steps.
  
##### Add the certificate for the application in Azure AD
  
  In the application registration blade for your application, in the **Certificates & secrets** page, in the **Certificates** section:
  
  1. Click on **Upload certificate** and, in click the browse button on the right to select the certificate you just exported (or your existing certificate)
  1. Click **Add**
  
##### Configure the appsettings.json file
  
  1. Open the `appsettings.json` file
  1. Find the app key `Clientertificates` in the `AzureAd` section and insert the `CertificateDescription` properties of your certificate within an array. You can see some examples below and read more about how to configure certificate descriptions [here](https://github.com/AzureAD/microsoft-identity-web/wiki/Certificates#specifying-certificates).
  
##### Get certificate from certificate store
  
  You can retrieve a certificate from your local store by adding the configuration below to the `ClientCertificates` array in the `appsettings.json` file replacing **<CERTIFICATE_STORE_PATH>** with the store path to your certificate and **<CERTIFICATE_DISTINGUISHED_NAME>** with the distinguished name of your certificate. If you used the configuration scripts to generate the application this will be done for you using a sample self-signed certificate. You can read more about certificate stores [here](https://docs.microsoft.com/windows-hardware/drivers/install/certificate-stores).
  
  ```json
  {
    // ... 
    "AzureAd": {
      // ...
        "ClientCertificates":  [{
          "SourceType":  "StoreWithDistinguishedName",
          "CertificateStorePath":  "<CERTIFICATE_STORE_PATH>",
          "CertificateDistinguishedName":  "<CERTIFICATE_DISTINGUISHED_NAME>"
        }]
    }
  }
  ```

##### Get certificate from file path
  
  It's possible to get a certificate file, such as a **pfx** file, directly from a file path on your machine and load it into the application by using the configuration as shown below. Add the configuration below to the `ClientCertificates` array of the `appsettings.json` file. Replace `<PATH_TO_YOUR_CERTIFICATE_FILE>` with the path to your certificate file and `<CERTIFICATE_PASSWORD>` with that certificates password. If you created the application with the `Configure.ps1` script found in the `AppCreationScripts-withCert` a **pfx** file called **daemon-console-v2.pfx** will be generated with the certificate that is associated with  your app and can be used as a credential. If you like, you can use configure the `Certificate` property to reference this file and use it as a credential.
  
  ```json
  {
    // ... 
    "AzureAd": {
      // ... 
      "ClientCertificates": [{
        "SourceType":  "Path",
        "CertificateDiskPath":  "<PATH_TO_YOUR_CERTIFICATE_FILE>",
        "CertificatePassword":  "<CERTIFICATE_PASSWORD>"
      }]
    } 
  }
  ```
  
##### Get certificate from Key Vault
  
  It's also possible to get certificates from an [Azure Key Vault](https://docs.microsoft.com/azure/key-vault/general/overview). Add the configuration below to the `ClientCertificates` array of the `appsettings.json` file. Replace `<YOUR_KEY_VAULT_URL>` with the URL of the Key Vault holding your certificate and `<YOUR_KEY_VAULT_CERTIFICATE_NAME>` with the name of that certificate as shown in your Key Vault. If you created the application with the `Configure.ps1` script found in the `AppCreationScripts-withCert` a **pfx** file called **daemon-console-v2.pfx** will be generated that is associated with the certificate that can be used as a credential for your app. If you like, you can load that certificate into a Key Vault and then access that Key Vault to use as a credential for your application.

  ```json
  {
    // ... 
    "AzureAd": {
      // ... 
      "ClientCertificates":  [{
        "SourceType":  "KeyVault",
        "KeyVaultUrl":  "<YOUR_KEY_VAULT_URL>",
        "KeyVaultCertificateName":  "<YOUR_KEY_VAULT_CERTIFICATE_NAME>"
      }]
    }
  }
  ```

### Step 4: Run the sample

#### .NET CLI

In the console run the API first

```Console
cd TodoList-WebApi
dotnet run
```

In a separate console, start the client app

```Console
cd daemon-console
dotnet run
```

#### Visual Studio 2022

Open the `daemon-console.sln` file and run the project

## About the code

### Code in Todo API

The Todo API is built on [ASP.NET Core](https://docs.microsoft.com/aspnet/core/introduction-to-aspnet-core) and is secured using the [Microsoft Identity Web](https://docs.microsoft.com/azure/active-directory/develop/microsoft-identity-web) library.

Within the `Program.cs` file you will see the [WebApplicationBuilder](https://docs.microsoft.com/dotnet/api/microsoft.aspnetcore.builder.webapplicationbuilder) **builder** which injects all dependencies into your application.

After the **builder** is created its [services](https://docs.microsoft.com/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-6.0) are configured to add [JSON web token validation](https://docs.microsoft.com/aspnet/core/security/authentication/?view=aspnetcore-6.0) and decorate the [controllers](https://docs.microsoft.com/aspnet/core/mvc/controllers/actions?view=aspnetcore-6.0) with token validation according to the configurations in your `appsettings.json`. This is why the `TenantId` and `ClientId` values must be provided.

```CSharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration);
```

The `TodoService` is then injected which will be discussed in more detail below.

```CSharp
builder.Services.AddSingleton<ITodoService, TodoService>();
```

The `TodoController` is decorated with the [Authorize](https://docs.microsoft.com/aspnet/core/security/authorization/simple?view=aspnetcore-6.0) attribute which will check that all incoming requests according to JwtBearer based authentication. The [AddMicrosoftIdentityWebApi](https://docs.microsoft.com/dotnet/api/microsoft.identity.web.microsoftidentitywebapiauthenticationbuilderextensions.addmicrosoftidentitywebapi?view=azure-dotnet) configures the [Authentication scheme](https://docs.microsoft.com/aspnet/core/security/authentication/?view=aspnetcore-6.0) to validate incoming **JWT's** to ensure that they come from a trusted issuer and that the **TodoList-WebApi** is the intended audience along with checking the **JWT's** expiration date and a couple of other features.

```CSharp
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TodoController : ControllerBase
{
    // Controller body...
}
```

The `TodoController` will verify the claims contained within the access token of the request to ensure it has sufficient [privileges](https://docs.microsoft.com/azure/active-directory/develop/v2-permissions-and-consent) to access to-do data.

It's often the case that developers will need to access both **delegated** and **application** permissions from the same API. The **Microsoft Identity Library** makes this easy with the [RequiredScopeOrAppPermission](https://docs.microsoft.com/dotnet/api/microsoft.identity.web.requiredscopeorapppermissionextensions.requirescopeorapppermission?view=azure-dotnet).

```CSharp
[HttpGet]
[RequiredScopeOrAppPermission(
    RequiredScopesConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredDelegatedTodoReadClaimsKey,
    RequiredAppPermissionsConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredApplicationTodoReadClaimsKey)]
public IActionResult Get()
{
    if (!Guid.TryParse(HttpContext.User.GetObjectId(), out var userIdentifier))
    {
        return BadRequest();
    }

    return Ok(_todoService.GetTodos(IsAppMakingRequest(), userIdentifier));
}
```

When each **GET** request is routed to this endpoint the **access token** used for that request is first validated to ensure that it has sufficient claims to access this endpoint. These claims could either confer **application permissions** or **delegated permissions** defined in the **appsettings.json**. If neither of these claims exist the request is rejected with a `401` response code.

This API is programmed to execute different behaviors based on whether or not the token contains **application permissions** or **delegated permissions**. For example, when the request has **application permissions** this endpoint will return all **todo's** found within its store. When the request has **delegated permissions** it will only return **todo's** associated with the user that's signed in to the application making the request.

To confirm that the access token was issued by an application, and *not* from a user login flow, the **idtyp** optional claim is added. When an access token is acquired from **Azure** *only* access tokens acquired via an application flow will contain this claim. All access token attained via a user sign in flow *will not* have this claim.

The code below shows how the token is checked to confirm the claim exists.

```csharp
private bool IsAppMakingRequest()
{
    // Add in the optional 'idtyp' claim to check if the access token is coming from an application or user.
    //
    // See: https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-optional-claims
    return HttpContext.User
        .Claims.Any(c => c.Type == "idtyp" && c.Value == "app");
}
```

The `TodoService` is responsible for handling the data management of **Todo's**. **Todo's** are stored in a [thread-safe collection](https://docs.microsoft.com/dotnet/standard/collections/thread-safe/) to allow for multi-threaded access. You can see the [concurrent dictionary documentation](https://docs.microsoft.com/dotnet/standard/collections/thread-safe/how-to-add-and-remove-items) for further information.

```csharp
public class TodoService : ITodoService
{
    private ConcurrentDictionary<Guid, Todo> _todoStore = new ConcurrentDictionary<Guid, Todo>();

    // Rest of service...
}
```

Each method within the `TodoService` is designed to do a simple create, read, update or delete action on **Todo's** contained in the `_todoStore`. Each method also contains a `hasAppPermissions` parameter to check if the call has applicatoin permissions. If this is `true` the methods are written to allow access to all **Todo's** in the `_todoStore`. If this is `false`, the method will only interact with **Todo's** that have a `UserId` matching the `userIdentifier` parameter passed into the method.

The `GetTodo` method provides an illustrative example.

```csharp
public Todo GetTodo(bool hasAppPermissions, Guid id, Guid userIdentifier)
{
    if (hasAppPermissions)
    {
        _todoStore.TryGetValue(id, out var todo);
        return todo;
    }

    var usersTodos = _todoStore
        .Where(td => td.Value.UserId == userIdentifier)
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    usersTodos.TryGetValue(id, out var userTodo);

    return userTodo;

}
```

### Code in the daemon-console

The **daemon-console** app is a simple [.NET Core](https://docs.microsoft.com/dotnet/core/introduction) app that acquires access tokens from Azure to make simple HTTP calls to the **TodoList-WebApi** code sample to perform basic CRUD operations.

The application makes use of [dependency injection](https://docs.microsoft.com/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-6.0) to create some simple services to perform the authorization, HTTP requests and print out the data received from the API.

This section of the file shows how values stored in the `appsettings.json` file are extracted and passed into the other services within the `ServiceCollection`.

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var services = new ServiceCollection();

// These options are populated with data from the appsettings.json file and provided to the services.
services.AddOptions<AzureAdOptions>()
    .Configure(azureAdOptions =>
        configuration.GetSection(AzureAdOptions.AzureAd).Bind(azureAdOptions));

services.AddOptions<DownstreamApiOptions>()
    .Configure(downstreamApiOptions =>
        configuration.GetSection(DownstreamApiOptions.DownstreamApi).Bind(downstreamApiOptions));

services.AddTransient<AccessTokenHandler>();

services.AddSingleton<IConfidentialClientApplicationService, ConfidentialClientApplicationService>();
services.AddSingleton<IPostTodosService, PostTodosService>();
services.AddSingleton<IDataDisplayService, DataDisplayService>();
```

This sample makes use of the [IHttpClientFactory](https://docs.microsoft.com/aspnet/core/fundamentals/http-requests?view=aspnetcore-6.0) to inject a custom [HttpClient](https://docs.microsoft.com/dotnet/api/system.net.http.httpclient?view=net-6.0) into services that contains the logic for making requests to the `TodoList-WebApi` and for retrieving access tokens from **Azure** before each request.

```csharp
// Inject a custom HttpClient to the services using the API url set in the appsettings.json file and provide the
// AccessTokenHandler as a message handler to include access tokens with each request.
services.AddHttpClient<ITodoService, TodoService>((serviceProvider, httpClient) =>
    {
        httpClient.BaseAddress = new Uri(configuration["DownStreamApi:BaseUrl"]);
    })
    .AddHttpMessageHandler<AccessTokenHandler>();
```

The `AccessTokenHandler` is responsible for retrieving **access tokens** fom **Azure** before each request is sent and does this leveraging the `ConfidentialClientApplicationService` which contains an instance of the [ConfidentialClientApplication](https://docs.microsoft.com/dotnet/api/microsoft.identity.client.confidentialclientapplication?view=azure-dotnet) class.

```csharp
/*
 * Handler for retrieving including an access token with the authentication header before each request.
 *
 * Leverages the IConfidentialClientApplicationService to retrieve tokens either from Azure or the
 * ConfidentialClientApplication token cache.
 */
public class AccessTokenHandler : DelegatingHandler
{
    private IConfidentialClientApplicationService _confidentialClientApplicationService;

    public AccessTokenHandler(IConfidentialClientApplicationService confidentialClientApplicationService)
    {
        _confidentialClientApplicationService = confidentialClientApplicationService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Retrieves access tokens straight from the token cache in the ConfidentialClientApplication. If a token has
        // not yet been cached it will retrieve a token directly from Azure and then cache it.
        var accessToken = await _confidentialClientApplicationService.GetAccessTokenAsync();

        request.Headers.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, accessToken);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
```

The `ConfidentialClientApplicationService` is responsible for creating the [ConfidentialClientApplication](https://docs.microsoft.com/python/api/msal/msal.application.confidentialclientapplication?view=azure-python) that will be used throughout the application. The primary function of the service is to extract the configuration settings from the `appsettings.json` file and pass them into the generated `ConfidentialClientApplication`.

```csharp
/*
 * This service contains the main ConfidentialClientApplication that is used throughout the app. This service is
 * responsible for acquiring access tokens form Azure using the credentials provided in your appsettings.json file.
 *
 * You can find a more detailed reading here:
 *
 * https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-net-instantiate-confidential-client-config-options
 */
public class ConfidentialClientApplicationService : IConfidentialClientApplicationService
{
    private AzureAdOptions _azureAdOptions;
    private DownstreamApiOptions _downStreamApiOptions;

    /*
     * The 'AzureAdOptions' and 'DownStreamApiOptions' are provided from the 'AzureAd' and 'DownStreamApi' sections of
     * the appsettings.json file respectively.
     */
    public ConfidentialClientApplicationService(IOptions<AzureAdOptions> azureAdOptions, IOptions<DownstreamApiOptions> downStreamApiOptions)
    {
        _azureAdOptions = azureAdOptions.Value;
        _downStreamApiOptions = downStreamApiOptions.Value;
    }

    private IConfidentialClientApplication _confidentialClientApplication;
    private IConfidentialClientApplication ConfidentialClientApplication
    {
        get
        {
            if (_confidentialClientApplication is null)
            {
                // Create a new ConfidentialClientApplication based on whether or not your application is configured to
                // use a client secret or certificate in your appsettings.json file.
                if (!string.IsNullOrWhiteSpace(_azureAdOptions.ClientSecret))
                {
                    _confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(_azureAdOptions.ClientId)
                        .WithAuthority(new Uri(_azureAdOptions.Authority))
                        .WithClientSecret(_azureAdOptions.ClientSecret)
                        .Build();
                }
                else if (_azureAdOptions.ClientCertificates is not null && _azureAdOptions.ClientCertificates.Any())
                {

                    ICertificateLoader certificateLoader = new DefaultCertificateLoader();
                    certificateLoader.LoadIfNeeded(_azureAdOptions.ClientCertificates.First());

                    _confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(_azureAdOptions.ClientId)
                        .WithAuthority(new Uri(_azureAdOptions.Authority))
                        .WithCertificate(_azureAdOptions.ClientCertificates.First().Certificate)
                        .Build();
                }
                else
                {
                    throw new Exception("You must choose between using a client secret or certificate. Please update the appsettings.json file.");
                }

            }

            return _confidentialClientApplication;
        }

        // More code...
    }
}

```

The `GetAccessTokenAsync` calls the `ConfidentialClientApplication` to make a request to Azure and retrieve an access token. The `ConfidentialClientApplication` is automatically configured to use a cache if a token has already been retrieved.

```csharp
public async Task<string> GetAccessTokenAsync()
{
    if (string.IsNullOrEmpty(_downStreamApiOptions.Scopes))
    {
        throw new Exception("'Scopes' must be set in the 'DownStreamApi' of appsettings.json file.");
    }

    // Scopes provide the information for what the application has access to.
    var scopes = _downStreamApiOptions.Scopes.Split(' ');

    // The authenticationResult contains the access token acquired from Azure by the ConfidentialClientApplication.
    // When the ConfidentialClientApplication is created in the code above it uses the credentials provided from
    // within the appsettings.json file to request the token. The scopes provided set what actions and information
    // are available when using that token.
    var authenticationResult = await ConfidentialClientApplication
        .AcquireTokenForClient(scopes)
        .ExecuteAsync();

    return authenticationResult.AccessToken;
}
```

The `TodoService` shows how the injected `HttpClient` can be used to retrieve data from the `TodoList-WebApi`. The authentication for the `HttpClient` is completed through the steps discussed above.

```csharp
/*
 * This class provides the basic create, read, update, delete (CRUD) actions available to this application from the
 * TodoList-WebApi. Authentication is handled automatically before each request by the custom HttpClient created
 * in Program.cs.
 */
public class TodoService : ITodoService
{
    private IConfidentialClientApplicationService _confidentialClientApplicationService;
    private HttpClient _httpClient;

    public TodoService(
        IConfidentialClientApplicationService confidentialClientApplicationService,
        HttpClient httpClient)
    {
        _confidentialClientApplicationService = confidentialClientApplicationService;
        _httpClient = httpClient;
    }

    public async Task<Guid> CreateTodoAsync(Todo todo)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/todo", todo);

        if (response.IsSuccessStatusCode)
        {
            var todoIdResponse = await response.Content.ReadAsStringAsync();

            // Need to remove the excess '"' characters from the raw response
            if (Guid.TryParse(todoIdResponse.Trim('"'), out var todoId))
            {
                return todoId;
            }
        }

        throw new HttpRequestException($"Request failed with status code: {response.StatusCode}\n");
    }
    
    // More code...
}
```

## Learn More

* [Microsoft identity platform (Azure Active Directory for developers)](https://docs.microsoft.com/azure/active-directory/develop/)
* [Overview of Microsoft Authentication Library (MSAL)](https://docs.microsoft.com/azure/active-directory/develop/msal-overview)
* [Authentication Scenarios for Azure AD](https://docs.microsoft.com/azure/active-directory/develop/authentication-flows-app-scenarios)
* [Azure AD code samples](https://docs.microsoft.com/azure/active-directory/develop/sample-v2-code)
* [Register an application with the Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
* [Building Zero Trust ready apps](https://aka.ms/ztdevsession)

## Next Steps

Learn how to:

* [Integrate a daemon app with Key Vault and MSI](https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2/tree/master/3-Using-KeyVault)

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`msal` `dotnet`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../issues).

If you find a bug in Msal.Net, please raise the issue on [MSAL.NET GitHub Issues](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## More information

For more information, see MSAL.NET's conceptual documentation:

* [Quickstart: Register an application with the Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
* [Quickstart: Configure a client application to access web APIs](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-access-web-apis)
* [Acquiring a token for an application with client credential flows](https://aka.ms/msal-net-client-credentials)

For more information about the underlying protocol:

* [Microsoft identity platform and the OAuth 2.0 client credentials flow](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow)

For a more complex multi-tenant Web app daemon application, see [active-directory-dotnet-daemon-v2](https://github.com/Azure-Samples/active-directory-dotnet-daemon-v2)
