---
page_type: sample
name: A .NET Core daemon console application calling a custom Web API with its own identity
services: active-directory
platforms: dotnet
urlFragment: active-directory-dotnetcore-daemon-v2
description: 
---

# A .NET Core daemon console application calling a custom Web API with its own identity

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/ASP.NET%20Core%20Web%20App%20tutorial)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=819)

Table Of Contents

* [Scenario](#Scenario)
* [Variation: daemon application using client credentials with certificates](#Variation:-daemon-application-using-client-credentials-with-certificates)
* [Prerequisites](#Prerequisites)
* [Setup the sample](#Setup-the-sample)
* [Next Steps](#Next-Steps)
* [Contributing](#Contributing)
* [Learn More](#Learn-More)
* [Next Steps](#Next-Steps)
* [Community Help and Support](#Community-Help-and-Support)
* [Contributing](#Contributing)
* [More information](#More-information)

## Scenario

### Overview

This sample application shows how to use the [Microsoft identity platform](http://aka.ms/aadv2) to access the data from a protected Web API, in a non-interactive process.  It uses the [OAuth 2 client credentials grant](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow) to acquire an [Access Tokens](https://aka.ms/access-tokens), which is then used to call the protected Web API. Additionally, it also lays out all the steps developers need to take to secure their Web APIs with the [Microsoft identity platform](http://aka.ms/aadv2).

The app is a .NET Core console application that gets the list of "ToDos" from `TodoList-WebApi` project by using Microsoft Authentication Library for .NET ([MSAL.NET](https://aka.ms/msal-net)) to acquire an access token for `TodoList-WebApi`.

> ### Daemon applications can use two forms of credentials to authenticate themselves with Azure AD:
>
> - **Client secrets** (also called application password).
> - **Certificates**.
>
> The first type (Client secret) is covered first the next paragraphs.
> A variation of this sample that uses a **certificate**, is also discussed at the end of this article in [Variation: daemon application using client credentials with certificates](#Variation-daemon-application-using-client-credentials-with-certificates)

The console application:

- acquires an access token from Azure AD by authenticating as an application (no user interaction)
- and then calls the Web API  `TodoList-WebApi` protected using [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web) to get the a list of ToDo's, and displays the result

![Topology](./ReadmeFiles/daemon-with-secret.svg)


## Variation: daemon application using client credentials with certificates

Daemon applications can use two forms of secrets to authenticate themselves with Azure AD:

- **application secrets** (also named application password). This is what we've seen so far.
- **certificates**. This is the object of this paragraph.

![Topology](./ReadmeFiles/daemon-with-certificate.svg)

To [use client credentials protocol flow with certificates](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow#second-case-access-token-request-with-a-certificate) instead of an application secret, you will need to do little changes to what you have done so far:

- (optionally) generate a certificate and export it, if you don't have one already
- register the certificate with your application in the application registration portal
- enable the sample code to use certificates instead of app secret.

For more information on the concepts used in this sample, be sure to read the [Scenario: Daemon application that calls web APIs](https://docs.microsoft.com/azure/active-directory/develop/scenario-daemon-overview).


![Scenario Image](./ReadmeFiles/topology.png)
## Prerequisites

* Either [Visual Studio](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/download) and [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
* An **Azure AD** tenant. For more information, see: [How to get an Azure AD tenant](https://docs.microsoft.com/azure/active-directory/develop/test-setup-environment#get-a-test-tenant)
* A user account in your **Azure AD** tenant. This sample will not work with a **personal Microsoft account**.  If you're signed in to the [Azure portal](https://portal.azure.com) with a personal Microsoft account and have not created a user account in your directory before, you will need to create one before proceeding.
## Setup the sample

### (Optional) use the automation script

1. On Windows run PowerShell and navigate to the root of the cloned directory
2. In PowerShell run:

   ```PowerShell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
   ```

3. Run the script to create your Azure AD application and configure the code of the sample application accordingly.

   ```PowerShell
   .\AppCreationScripts-WtihCert\Configure.ps1
   ```

   > Other ways of running the scripts are described in [App Creation Scripts](./AppCreationScripts-WithCert/AppCreationScripts.md)

If you don't want to use this automation, follow the following steps:

#### (Optional) Create a self-signed certificate

  To complete this step, you will use the `New-SelfSignedCertificate` Powershell command. You can find more information about the New-SelfSignedCertificate command [here](https://docs.microsoft.com/powershell/module/pkiclient/new-selfsignedcertificate).
  
  1. Open PowerShell and run `New-SelfSignedCertificate` with the following parameters to create a self-signed certificate in the user certificate store on your computer:
  
      ```PowerShell
      $cert=New-SelfSignedCertificate -Subject "CN=daemon-console-v2" -CertStoreLocation "Cert:\CurrentUser\My"  -KeyExportPolicy Exportable -KeySpec Signature
      ```
  
  1. Export this certificate using the "Manage User Certificate" MMC snap-in accessible from the Windows Control Panel. You can also add other options to generate the certificate in a different
  store such as the Computer or service store (See [How to: View Certificates with the MMC Snap-in](https://docs.microsoft.com/dotnet/framework/wcf/feature-details/how-to-view-certificates-with-the-mmc-snap-in)).
  
  Alternatively you can use an existing certificate if you have one (just be sure to record its name for the next steps)
  
##### Add the certificate for the application in Azure AD
  
  In the application registration blade for your application, in the **Certificates & secrets** page, in the **Certificates** section:
  
  1. Click on **Upload certificate** and, in click the browse button on the right to select the certificate you just exported (or your existing certificate)
  1. Click **Add**
  
##### Configure the Visual Studio project
  
  To change the visual studio project to enable certificates you need to:
  
  1. Open the `appsettings.json` file
  2. Find the app key `Clientertificates` in the `AzureAd` section and insert the `CertificateDescription` properties of your certificate within an array. You can see some examples below and read more about how to configure certificate descriptions [here](https://github.com/AzureAD/microsoft-identity-web/wiki/Certificates#specifying-certificates).
  
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
  
  1. If you had set `ClientSecret` previously, change its value to an empty string, `""`.

#### Build and run

Build and run your project. You have the same output, but this time, your application is authenticated with Azure AD with the certificate instead of the application secret.

#### About the alternate code

This application makes use of the [Microsoft Identity Web Library](https://docs.microsoft.com/azure/active-directory/develop/microsoft-identity-web) to load the certificate based on the configurations in the `daemon-console/appsettings.json` for the `ClientCertificates` property setting. The `DefaultCertificateLoader` class contains the logic needed to load a certificate into your application and can store it into a `CertificateDescription` object as a [X509Certificate2](https://docs.microsoft.com/dotnet/api/system.security.cryptography.x509certificates.x509certificate2?view=net-6.0) object.

The application uses a `DefaultCertificateLoader` instance to load a `X509Certificate2` into the `config.Certificate` object. After this is done the certificate becomes accessible as in the `config` object as shown below by calling `config.Certificate.Certificate`. Instead of using the `WithClientSecret` to add a client secret as a credential `WithCertificate` is used associate a certificate as the credential.

You can find this logic in the `ConfidentialClientApplicationService.cs` file.

```CSharp
ICertificateLoader certificateLoader = new DefaultCertificateLoader();
certificateLoader.LoadIfNeeded(_azureAdOptions.ClientCertificates.First());

_confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(_azureAdOptions.ClientId)
    .WithAuthority(new Uri(_azureAdOptions.Authority))
    .WithCertificate(_azureAdOptions.ClientCertificates.First().Certificate)
    .Build();
```

The rest of the application remains the same.

### Did you forget to provide admin consent? This is needed for daemon apps

If you get an error when calling the API `Insufficient privileges to complete the operation.`, this is because the tenant administrator has not granted permissions
to the application. See step 6 of [Register the client app (daemon-console-v2)](#register-the-client-app-daemon-console) above.

You will typically see, on the output window, something like the following:

```Json
Failed to call the Web Api: Forbidden
Content: {
  "error": {
    "code": "Authorization_RequestDenied",
    "message": "Insufficient privileges to complete the operation.",
    "innerError": {
      "request-id": "<a guid>",
      "date": "<date>"
    }
  }
}
```

To run this sample, you'll need:

- [Visual Studio](https://aka.ms/vsdownload) and the [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
- An Internet connection
- A Windows machine (necessary if you want to run the app on Windows)
- An OS X machine (necessary if you want to run the app on Mac)
- A Linux machine (necessary if you want to run the app on Linux)
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, see [How to get an Azure AD tenant](https://azure.microsoft.com/documentation/articles/active-directory-howto-tenant/)

### Step 1:  Clone or download this repository

From your shell or command line:

```Shell
git clone https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2.git
```

or download and extract the repository .zip file.

> Given that the name of the sample is pretty long, and so are the name of the referenced NuGet packages, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

Navigate to the `"2-Call-OwnApi"` folder

```Shell
cd "2-Call-OwnApi"
```

### Step 2:  Register the sample with your Azure Active Directory tenant

There is one project in this sample. To register it, you can:

- either follow the steps [Step 2: Register the sample with your Azure Active Directory tenant](#step-2-register-the-sample-with-your-azure-active-directory-tenant) and [Step 3:  Configure the sample to use your Azure AD tenant](#choose-the-azure-ad-tenant-where-you-want-to-create-your-applications)
- or use PowerShell scripts that:
  - **automatically** creates the Azure AD applications and related objects (passwords, permissions, dependencies) for you
  - modify the Visual Studio projects' configuration files.

If you want to use this automation:

1. On Windows run PowerShell and navigate to the root of the cloned directory
1. In PowerShell run:

   ```PowerShell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
   ```

1. Run the script to create your Azure AD application and configure the code of the sample application accordingly.

   ```PowerShell
   .\AppCreationScripts\Configure.ps1
   ```

   > Other ways of running the scripts are described in [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md)

1. Open the Visual Studio solution and click start

If you don't want to use this automation, follow the steps below

#### Choose the Azure AD tenant where you want to create your applications

As a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com) using either a work or school account or a personal Microsoft account.
1. If your account is present in more than one Azure AD tenant, select `Directory + Subscription` at the top right corner in the menu on top of the page, and switch your portal session to the desired Azure AD tenant.
1. In the left-hand navigation pane, select the **Azure Active Directory** service, and then select **App registrations**.

#### Register the service app (TodoList-webapi-daemon-v2)

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
1. When the **Register an application page** appears, enter your application's registration information:
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `TodoList-webapi-daemon-v2`.
   - Leave **Supported account types** on the default setting of **Accounts in this organizational directory only**.
1. Select **Register** to create the application.
1. On the app **Overview** page, find the **Application (client) ID** value and record it for later. You'll need it to configure the Visual Studio configuration file for this project.
1. Select the **Expose an API** section:
    - Use the 'Set' button to generate the default AppID URI in the form of `api://<web api client id>`
   > If your tenant went through [domain verification](https://docs.microsoft.com/azure/active-directory/develop/howto-configure-publisher-domain) and you have verified domains available, you can use an AppID URI in the form of `https://<yourdomain>` or `https://<yourdomain>/<myAPI name>`  as well.
   - Click **Save**
1. Select the **Manifest** section, and:
   - Edit the manifest by locating the `appRoles`.  The role definition is provided in the JSON code block below. Leave the `allowedMemberTypes` to **Application** only. Each role definition in this manifest must have a different valid **Guid** for the "id" property.
   - Save the manifest.

The content of `appRoles` should be the following (the `id` can be any unique **Guid**)

```Json
{
  ...
    "appRoles": [
    {
        "allowedMemberTypes": [
        	"Application"
        ],
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
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `daemon-console-v2`.
   - In the **Supported account types** section, select **Accounts in this organizational directory only ({tenant name})**.
   - Select **Register** to create the application.
1. On the app **Overview** page, find the **Application (client) ID** value and record it for later. You'll need it to configure the Visual Studio configuration file for this project.
1. From the **Certificates & secrets** page, in the **Client secrets** section, choose **New client secret**:

   - Type a key description (of instance `app secret`),
   - Select a key duration of either **In 1 year**, **In 2 years**, or **Never Expires**.
   - When you press the **Add** button, the key value will be displayed, copy, and save the value in a safe location.
   - You'll need this key later to configure the project in Visual Studio. This key value will not be displayed again, nor retrievable by any other means,
     so record it as soon as it is visible from the Azure portal.
1. In the list of pages for the app, select **API permissions**
   - Click the **Add a permission** button and then,
   - Ensure that the **My APIs** tab is selected
   - Select the API created in the previous step, for example `TodoList-webapi-daemon-v2`
   - In the **Application permissions** section, ensure that the right permissions are checked: **DaemonAppRole**
   - Select the **Add permissions** button
1. At this stage permissions are assigned correctly but the client app does not allow interaction.
   Therefore no consent can be presented via a UI and accepted to use the service app.
   Click the **Grant/revoke admin consent for {tenant}** button, and then select **Yes** when you are asked if you want to grant consent for the
   requested permissions for all account in the tenant.
   You need to be an Azure AD tenant admin to do this.

### Step 3:  Configure the sample to use your Azure AD tenant

In the steps below, "ClientID" is the same as "Application ID" or "AppId".

Open the solution in Visual Studio to configure the projects

#### Configure the service project

> Note: if you used the setup scripts, the changes below will have been applied for you

1. Open the `TodoList-WebApi\appsettings.json` file
1. Find the app key `Domain` in the `AzureAd` section and replace the existing value with your Azure AD tenant name.
1. Find the app key `TenantId` in the `AzureAd` section and replace the existing value with your Azure AD tenant ID.
1. Find the app key `ClientId` in the `AzureAd` section and replace the existing value with the application ID (clientId) of the `TodoList-webapi-daemon-v2` application copied from the Azure portal.

#### Configure the client project

1. Open the `Daemon-Console\appsettings.json` file
1. If you are connecting to a national cloud, change the instance to the correct Azure AD endpoint. [See this reference for a list of Azure AD endpoints.](https://docs.microsoft.com/graph/deployments#app-registration-and-token-service-root-endpoints)
1. Find the app key `TenantId`  in the `AzureAd` section and replace the existing value with your Azure AD tenant name.
1. Find the app key `ClientId` in the `AzureAd` section and replace the existing value with the application ID (clientId) of the `daemon-console-v2` application copied from the Azure portal.
1. Find the app key `ClientSecret` in the `AzureAd` section and replace the existing value with the key you saved during the creation of the `daemon-console-v2` app, in the Azure portal.
1. Find the app key `BaseUrl` in the `DownStreamApi` section and set to `https://localhost:44372/`
1. Find the app key `Scopes` in the `DownStreamApi` section and replace the existing value with the **App ID URI** of your web API, followed by "/.default".  

### Step 4: Run the sample

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

Once the client app is started, it will display the ToDos from the API.

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRy8G199fkJNDjJ9kJaxUJIhUNUJGSDU1UkxFMlRSWUxGVTlFVkpGT0tOTi4u)

### Code in Todo API

The Todo API is built on [ASP.NET Core](https://docs.microsoft.com/aspnet/core/introduction-to-aspnet-core) and is secured using the [Microsoft Identity Web](https://docs.microsoft.com/azure/active-directory/develop/microsoft-identity-web) library.

Within the `Program.cs` file you will see the [WebApplicationBuilder](https://docs.microsoft.com/dotnet/api/microsoft.aspnetcore.builder.webapplicationbuilder) **builder** which injects all dependencies into your application.

After the **builder** is created, its [services](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-6.0) are configured to add [JSON web token validation](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-6.0) and decorate the [controllers](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/actions?view=aspnetcore-6.0) with token validation according to the configurations in your `appsettings.json`. This is why the `TenantId` and `ClientId` values must be provided.

```CSharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration);
```

The `TodoService` is then injected which will be discussed in more detail below.

```CSharp
builder.Services.AddSingleton<ITodoService, TodoService>();
```

The `TodoController` is decorated with the [Authorize](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/simple?view=aspnetcore-6.0) attribute which will check that all incoming requests according to JwtBearer-based authentication. The (AddMicrosoftIdentityWebApi)[https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.web.microsoftidentitywebapiauthenticationbuilderextensions.addmicrosoftidentitywebapi?view=azure-dotnet] configures the [Authentication scheme](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-6.0) to validate incoming **JWT's** to ensure that they come from a trusted issuer and that the **TodoList-WebApi** is the intended audience along with checking the **JWT's** expiration date and a couple of other features.

```CSharp
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class TodoController : ControllerBase
{
    // Controller body...
}
```

The `TodoController` then must verify the claims contained within the request to ensure it has sufficient [privileges](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-permissions-and-consent).

It's often the case that developers will need to access both **delegated** and **application** permissions from the same API. The **Microsoft Identity Library** makes this easy with the [RequiredScopeOrAppPermission](https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.web.requiredscopeorapppermissionextensions.requirescopeorapppermission?view=azure-dotnet).

```CSharp
[HttpGet]
[RequiredScopeOrAppPermission(
    RequiredScopesConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredDelegatedTodoReadClaimsKey,
    RequiredAppPermissionsConfigurationKey = RequiredTodoAccessPermissionsOptions.RequiredApplicationTodoReadWriteClaimsKey)]
public IActionResult Get()
{
    if (!Guid.TryParse(HttpContext.User.GetObjectId(), out var userIdentifier))
    {
        return BadRequest();
    }

    return Ok(_todoService.GetTodos(IsAppMakingRequest(), userIdentifier));
}
```

When each **GET** request is routed to this endpoint the retrieved **access token** is first validated to ensure that it has sufficient claims to access this endpoint. These claims could either confer **application permissions** or **delegated permissions** defined in the **appsettings.json**. If neither of these claims exist the request is rejected with a `401` response code.

This API is programmed to execute different behaviors based on whether or not the token contains **application permissions** or **delegated permissions**. When the request has **application permissions** this endpoint will return all **todo's** found within its store. When the request has **delegated permissions** it will only return **todo's** associated with the user that's signed in.

To confirm that the access token was issued by an applicaiton, and *not* from a user login flow, the **idtyp** optional claim is added. When an access token is acquired from **Azure** *only* access tokens acquired via an application flow will contain this claim. All access token attained via a user sign in flow *will not* have this claim.

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

The `TodoService` is responsible for handling the data management of **Todo's**. **Todo's** are stored in a [thread-safe collection](https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/) to allow for multi-threaded access. You can see the [concurrent dictionary documentation](https://docs.microsoft.com/en-us/dotnet/standard/collections/thread-safe/how-to-add-and-remove-items) for further information.

```csharp
public class TodoService : ITodoService
{
    private ConcurrentDictionary<Guid, Todo> _todoStore = new ConcurrentDictionary<Guid, Todo>();

    // Rest of service...
}
```

Each method within the `TodoService` is designed to do a simple create, read, update or delete action on **Todo's** contained in the `_todoStore`. Each method also contains a `hasAppPermissions` parameter to check if the call has applicatoin permissions. If this is true the methods are written to allow access to all **Todo's** in the `_todoStore`. If this is false, the method will only interact with **Todo's** that have a `UserId` matching the `userIdentifier` parameter passed into the method.

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

Next, the initial scopes are extracted from the `appsettings.json` file from within the `DownstreamApi` object. These scopes will be used in the access token stored within a cache within your server and also be contained within the token which will be retrieved by the SPA after it exchanges its *access code*. The server-side token cache will be cleared out for users after they sign-out.

### Code in the daemon-console

The **daemon-console** app is a simple [.NET Core](https://docs.microsoft.com/en-us/dotnet/core/introduction) app that acquires access tokens from Azure to make simple HTTP calls to the **TodoList-WebApi** code sample to perform basic CRUD operations.

The application makes use of [dependency injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-6.0) to create some simple services to perform the authorization, HTTP requests and print out the data received from the API.

```csharp
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
services.AddSingleton<IUploadTodosService, UploadTodosService>();
services.AddSingleton<IDataDisplayService, DataDisplayService>();

var serviceProvider = services.BuildServiceProvider();

var todoService = serviceProvider.GetService<ITodoService>();
var uploadTodosService = serviceProvider.GetService<IUploadTodosService>();
var dataDisplayService = serviceProvider.GetService<IDataDisplayService>();
```

The `ConfidentialClientApplicationService` is responsible for creating the [ConfidentialClientApplication](https://docs.microsoft.com/en-us/python/api/msal/msal.application.confidentialclientapplication?view=azure-python) that will be used throughout the application. The primary function of the service is to extract the configuration settings from the `appsettings.json` file and pass them into the generated `ConfidentialClientApplication`.

```csharp
private AzureAdOptions _azureAdOptions;
private DownstreamApiOptions _downStreamApiOptions;

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

            var clientSecretPlaceholderValue = "[Enter here a client secret for your application]";

            if (!string.IsNullOrWhiteSpace(_azureAdOptions.ClientSecret) &&
                _azureAdOptions.ClientSecret != clientSecretPlaceholderValue)
            {
                _confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(_azureAdOptions.ClientId)
                    .WithAuthority(new Uri(_azureAdOptions.Authority))
                    .WithClientSecret(_azureAdOptions.ClientSecret)
                    .Build();
            }
            else if (_azureAdOptions.ClientCertificates.Any())
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
                throw new Exception("You must choose between using client secret or certificate. Please update appsettings.json file.");
            }

        }

        return _confidentialClientApplication;
    }
}
```

The `GetAuthenticationResultAsync` calls the `ConfidentialClientApplication` to make a request to Azure and retrieve an access token. The `ConfidentialClientApplication` is automatically configured to use a cache if a token has already been retrieved.

```csharp
public async Task<AuthenticationResult> GetAuthenticationResultAsync()
{
    if (string.IsNullOrEmpty(_downStreamApiOptions.Scopes))
    {
        throw new Exception("'Scopes' must be set in the 'DownStreamApi' of appsettings.json file.");
    }

    var scopes = _downStreamApiOptions.Scopes.Split(' ');

    var authenticationResult = await ConfidentialClientApplication
        .AcquireTokenForClient(scopes)
        .ExecuteAsync();

    return authenticationResult;
}
```

The `TodoService` shows how the `ConfidentialClientApplicationService` can be leveraged to retrieve tokens from the API after receiving an **access token**.

```csharp
private IConfidentialClientApplicationService _confidentialClientApplicationService;
private DownstreamApiOptions _downStreamApiOptions;

public TodoService(
    IConfidentialClientApplicationService confidentialClientApplicationService,
    IOptions<AzureAdOptions> azureAdOptions,
    IOptions<DownstreamApiOptions> downStreamApiOptions)
{
    _confidentialClientApplicationService = confidentialClientApplicationService;
    _downStreamApiOptions = downStreamApiOptions.Value;
}

public async Task<Guid> AddAsync(Todo todo)
{
    var httpClient = await PrepareHttpClientAsync();
    var response = await httpClient.PostAsJsonAsync($"{_downStreamApiOptions.BaseUrl}api/todo", todo);

    if (response.IsSuccessStatusCode)
    {
        var todoIdResponse = (await response.Content.ReadAsStringAsync()).Trim('"');

        if (Guid.TryParse(todoIdResponse, out var todoId))
        {
            return todoId;
        }
    }

    throw new HttpRequestException($"Request failed with status code: {response.StatusCode}\n");
}

// More code...

private async Task<HttpClient> PrepareHttpClientAsync()
{
    var authenticationResult = await _confidentialClientApplicationService.GetAuthenticationResultAsync();

    var httpClient = new HttpClient();
    var defaultRequestHeaders = httpClient.DefaultRequestHeaders;

    if (defaultRequestHeaders.Accept is null || !defaultRequestHeaders.Accept.Any(m => m.MediaType == "application/json"))
    {
        httpClient.DefaultRequestHeaders
            .Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    defaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

    return httpClient;
}
```


<details>
 <summary>Expand the section</summary>

This project has two WebApp / Web API projects. To deploy them to Azure Web Sites, you'll need, for each one, to:

 *create an Azure Web Site
 *publish the Web App / Web APIs to the web site, and
 *update its client(s) to call the web site instead of IIS Express.

### Create and publish the `TodoListService-aspnetcore-webapi` to an Azure Web Site

1. Sign in to the [Azure portal](https://portal.azure.com).
1. Click `Create a resource` in the top left-hand corner, select **Web** --> **Web App**, and give your web site a name, for example, `TodoListService-aspnetcore-webapi-contoso.azurewebsites.net`.
1. Thereafter select the `Subscription`, `Resource Group`, `App service plan and Location`. `OS` will be **Windows** and `Publish` will be **Code**.
1. Click `Create` and wait for the App Service to be created.
1. Once you get the `Deployment succeeded` notification, then click on `Go to resource` to navigate to the newly created App service.
1. Once the web site is created, locate it it in the **Dashboard** and click it to open **App Services** **Overview** screen.
1. From the **Overview** tab of the App Service, download the publish profile by clicking the **Get publish profile** link and save it.  Other deployment mechanisms, such as from source control, can also be used.
1. Switch to Visual Studio and go to the TodoListService-aspnetcore-webapi project.  Right click on the project in the Solution Explorer and select **Publish**.  Click **Import Profile** on the bottom bar, and import the publish profile that you downloaded earlier.
1. Click on **Configure** and in the `Connection tab`, update the Destination URL so that it is a `https` in the home page url, for example [https://TodoListService-aspnetcore-webapi-contoso.azurewebsites.net](https://TodoListService-aspnetcore-webapi-contoso.azurewebsites.net). Click **Next**.
1. On the Settings tab, make sure `Enable Organizational Authentication` is NOT selected.  Click **Save**. Click on **Publish** on the main screen.
1. Visual Studio will publish the project and automatically open a browser to the URL of the project.  If you see the default web page of the project, the publication was successful.

### Update the Active Directory tenant application registration for `TodoListService-aspnetcore-webapi`

1. Navigate back to to the [Azure portal](https://portal.azure.com).
In the left-hand navigation pane, select the **Azure Active Directory** service, and then select **App registrations (Preview)**.
1. In the resultant screen, select the `TodoListService-aspnetcore-webapi` application.
1. From the *Branding* menu, update the **Home page URL**, to the address of your service, for example [https://TodoListService-aspnetcore-webapi-contoso.azurewebsites.net](https://TodoListService-aspnetcore-webapi-contoso.azurewebsites.net). Save the configuration.
1. Add the same URL in the list of values of the *Authentication -> Redirect URIs* menu. If you have multiple redirect urls, make sure that there a new entry using the App service's Uri for each redirect url.

### Update the `TodoListClient-aspnetcore-webapi` to call the `TodoListService-aspnetcore-webapi` Running in Azure Web Sites

1. In Visual Studio, go to the `TodoListClient-aspnetcore-webapi` project.
2. Open `Client\appsettings.json`.  Only one change is needed - update the `todo:TodoListBaseAddress` key value to be the address of the website you published,
   for example, [https://TodoListService-aspnetcore-webapi-contoso.azurewebsites.net](https://TodoListService-aspnetcore-webapi-contoso.azurewebsites.net).
3. Run the client! If you are trying multiple different client types (for example, .Net, Windows Store, Android, iOS) you can have them all call this one published web API.

### Create and publish the `TodoListClient-aspnetcore-webapi` to an Azure Web Site

1. Sign in to the [Azure portal](https://portal.azure.com).
1. Click `Create a resource` in the top left-hand corner, select **Web** --> **Web App**, and give your web site a name, for example, `TodoListClient-aspnetcore-webapi-contoso.azurewebsites.net`.
1. Thereafter select the `Subscription`, `Resource Group`, `App service plan and Location`. `OS` will be **Windows** and `Publish` will be **Code**.
1. Click `Create` and wait for the App Service to be created.
1. Once you get the `Deployment succeeded` notification, then click on `Go to resource` to navigate to the newly created App service.
1. Once the web site is created, locate it in the **Dashboard** and click it to open **App Services** **Overview** screen.
1. From the **Overview** tab of the App Service, download the publish profile by clicking the **Get publish profile** link and save it.  Other deployment mechanisms, such as from source control, can also be used.
1. Switch to Visual Studio and go to the TodoListClient-aspnetcore-webapi project.  Right click on the project in the Solution Explorer and select **Publish**.  Click **Import Profile** on the bottom bar, and import the publish profile that you downloaded earlier.
1. Click on **Configure** and in the `Connection tab`, update the Destination URL so that it is a `https` in the home page url, for example [https://TodoListClient-aspnetcore-webapi-contoso.azurewebsites.net](https://TodoListClient-aspnetcore-webapi-contoso.azurewebsites.net). Click **Next**.
1. On the Settings tab, make sure `Enable Organizational Authentication` is NOT selected.  Click **Save**. Click on **Publish** on the main screen.
1. Visual Studio will publish the project and automatically open a browser to the URL of the project.  If you see the default web page of the project, the publication was successful.

### Update the Active Directory tenant application registration for `TodoListClient-aspnetcore-webapi`

1. Navigate back to to the [Azure portal](https://portal.azure.com).
In the left-hand navigation pane, select the **Azure Active Directory** service, and then select **App registrations (Preview)**.
1. In the resultant screen, select the `TodoListClient-aspnetcore-webapi` application.
1. In the **Authentication** | page for your application, update the Logout URL fields with the address of your service, for example [https://TodoListClient-aspnetcore-webapi-contoso.azurewebsites.net](https://TodoListClient-aspnetcore-webapi-contoso.azurewebsites.net)
1. From the *Branding* menu, update the **Home page URL**, to the address of your service, for example [https://TodoListClient-aspnetcore-webapi-contoso.azurewebsites.net](https://TodoListClient-aspnetcore-webapi-contoso.azurewebsites.net). Save the configuration.
1. Add the same URL in the list of values of the *Authentication -> Redirect URIs* menu. If you have multiple redirect urls, make sure that there a new entry using the App service's Uri for each redirect url.

> NOTE: Remember, the To Do list is stored in memory in this TodoListService sample. Azure Web Sites will spin down your web site if it is inactive, and your To Do list will get emptied.
Also, if you increase the instance count of the web site, requests will be distributed among the instances. To Do will, therefore, not be the same on each instance.

</details>


## Next Steps

Could not find file 'C:\GitHub\AzureSamples\active-directory-dotnetcore-daemon-v2\2-Call-OwnApi\ReadmeFiles\ReadmeNextSteps.md'.

## Contributing

Could not find file 'C:\GitHub\AzureSamples\active-directory-dotnetcore-daemon-v2\2-Call-OwnApi\ReadmeFiles\ReadmeContributing.md'.

## Learn More

* [Microsoft identity platform (Azure Active Directory for developers)](https://docs.microsoft.com/azure/active-directory/develop/)
* [Overview of Microsoft Authentication Library (MSAL)](https://docs.microsoft.com/azure/active-directory/develop/msal-overview)
* [Authentication Scenarios for Azure AD](https://docs.microsoft.com/azure/active-directory/develop/authentication-flows-app-scenarios)
* [Azure AD code samples](https://docs.microsoft.com/azure/active-directory/develop/sample-v2-code)
* [Register an application with the Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
* [Building Zero Trust ready apps](https://aka.ms/ztdevsession)

## Next Steps

Learn how to:

- [Integrate a daemon app with Key Vault and MSI](https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2/tree/master/3-Using-KeyVault)

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

- [Quickstart: Register an application with the Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
- [Quickstart: Configure a client application to access web APIs](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-access-web-apis)
- [Acquiring a token for an application with client credential flows](https://aka.ms/msal-net-client-credentials)

For more information about the underlying protocol:

- [Microsoft identity platform and the OAuth 2.0 client credentials flow](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow)

For a more complex multi-tenant Web app daemon application, see [active-directory-dotnet-daemon-v2](https://github.com/Azure-Samples/active-directory-dotnet-daemon-v2)
