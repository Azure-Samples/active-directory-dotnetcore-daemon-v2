---
topic: sample
languages:
  - csharp
products:
  - microsoft-entra-id
  - dotnet-core
  - office-ms-graph
description: "Shows how a daemon console app uses MSAL.NET to get an access token and call a protected Web API."
---

# A .NET Core daemon console application calling a protected Web API with its own identity

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/aad%20Samples/.NET%20client%20samples/active-directory-dotnetcore-daemon-v2%20CI)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=695)

## About this sample

### Overview

This sample application shows how to use the [Microsoft identity platform](http://aka.ms/aadv2) to access the data from a protected Web API, in a non-interactive process.  It uses the [OAuth 2 client credentials grant](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow) to acquire an [Access Tokens](https://aka.ms/access-tokens), which is then used to call the protected Web API. Additionally, it also lays out all the steps developers need to take to secure their Web APIs with the [Microsoft identity platform](http://aka.ms/aadv2).

The app is a .NET Core console application that gets the list of "ToDos" from `TodoList-WebApi` project by using Microsoft Authentication Library for .NET ([MSAL.NET](https://aka.ms/msal-net)) to acquire an access token for `TodoList-WebApi`.

## Scenario

The console application:

- acquires an access token from Microsoft Entra ID by authenticating as an application (no user interaction)
- and then calls the Web API  `TodoList-WebApi` protected using [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web) to get the a list of ToDo's, and displays the result

![Topology](./ReadmeFiles/daemon-with-secret.svg)

For more information on the concepts used in this sample, be sure to read the [Scenario: Daemon application that calls web APIs](https://docs.microsoft.com/azure/active-directory/develop/scenario-daemon-overview).

> ### Daemon applications can use two forms of credentials to authenticate themselves with Microsoft Entra ID:
>
> - **Client secrets** (also called application password).
> - **Certificates**.
>
> The first type (Client secret) is covered first the next paragraphs.
> A variation of this sample that uses a **certificate**, is also discussed at the end of this article in [Variation: daemon application using client credentials with certificates](#Variation-daemon-application-using-client-credentials-with-certificates)

## How to run this sample

To run this sample, you'll need:

- [Visual Studio](https://aka.ms/vsdownload) and the [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
- An Internet connection
- A Windows machine (necessary if you want to run the app on Windows)
- An OS X machine (necessary if you want to run the app on Mac)
- A Linux machine (necessary if you want to run the app on Linux)
- a Microsoft Entra tenant. For more information on how to get a Microsoft Entra tenant, see [How to get a Microsoft Entra tenant](https://azure.microsoft.com/documentation/articles/active-directory-howto-tenant/)

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

### Step 2:  Register the sample with your Microsoft Entra tenant

There is one project in this sample. To register it, you can:

- either follow the steps [Step 2: Register the sample with your Microsoft Entra tenant](#step-2-register-the-sample-with-your-azure-active-directory-tenant) and [Step 3:  Configure the sample to use your Microsoft Entra tenant](#choose-the-azure-ad-tenant-where-you-want-to-create-your-applications)
- or use PowerShell scripts that:
  - **automatically** creates the Microsoft Entra applications and related objects (passwords, permissions, dependencies) for you
  - modify the Visual Studio projects' configuration files.

If you want to use this automation:

1. On Windows run PowerShell and navigate to the root of the cloned directory
1. In PowerShell run:

   ```PowerShell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
   ```

1. Run the script to create your Microsoft Entra application and configure the code of the sample application accordingly.

   ```PowerShell
   .\AppCreationScripts\Configure.ps1
   ```

   > Other ways of running the scripts are described in [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md)

1. Open the Visual Studio solution and click start

If you don't want to use this automation, follow the steps below

#### Choose the Microsoft Entra tenant where you want to create your applications

As a first step you'll need to:

1. Sign in to the [Microsoft Entra admin center](https://entra.microsoft.com) using either a work or school account or a personal Microsoft account.
1. If your account is present in more than one Microsoft Entra tenant, select `Directory + Subscription` at the top right corner in the menu on top of the page, and switch your portal session to the desired Microsoft Entra tenant.
1. In the left-hand navigation pane, select the **Microsoft Entra ID** service, and then select **App registrations**.

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
            "description": "Daemon apps in this role can consume the web api.",
            "displayName": "DaemonAppRole",
            "id": "7489c77e-0f34-4fe9-bf84-0ce8b74a03c4",
            "isEnabled": true,
            "lang": null,
            "origin": "Application",
            "value": "DaemonAppRole"
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
     so record it as soon as it is visible from the Microsoft Entra admin center.
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
   You need to be a Microsoft Entra tenant admin to do this.

### Step 3:  Configure the sample to use your Microsoft Entra tenant

In the steps below, "ClientID" is the same as "Application ID" or "AppId".

Open the solution in Visual Studio to configure the projects

#### Configure the service project

> Note: if you used the setup scripts, the changes below will have been applied for you

1. Open the `TodoList-WebApi\appsettings.json` file
1. Find the app key `Domain` and replace the existing value with your Microsoft Entra tenant name.
1. Find the app key `TenantId` and replace the existing value with your Microsoft Entra tenant ID.
1. Find the app key `ClientId` and replace the existing value with the application ID (clientId) of the `TodoList-webapi-daemon-v2` application copied from the Microsoft Entra admin center.

#### Configure the client project

1. Open the `Daemon-Console\appsettings.json` file
1. If you are connecting to a national cloud, change the instance to the correct Microsoft Entra ID endpoint. [See this reference for a list of Microsoft Entra ID endpoints.](https://docs.microsoft.com/graph/deployments#app-registration-and-token-service-root-endpoints)
1. Find the app key `Tenant` and replace the existing value with your Microsoft Entra tenant name.
1. Find the app key `ClientId` and replace the existing value with the application ID (clientId) of the `daemon-console-v2` application copied from the Microsoft Entra admin center.
1. Find the app key `ClientSecret` and replace the existing value with the key you saved during the creation of the `daemon-console-v2` app, in the Microsoft Entra admin center.
1. Find the app key `TodoListBaseAddress` and set to `https://localhost:44372`
1. Find the app key `TodoListScope` and replace the existing value with the **App ID URI** of your web API, followed by "/.default".  

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

## About the code

The relevant code for this sample is in the `Program.cs` file:

1. Configure your application

    Important note: even if we are building a console application, since it is a daemon, and therefore a confidential client application, as it does not access the Web APIs on behalf of a user, but on its own (application).

    ```CSharp
    // Get the Token acquirer factory instance. By default it reads an appsettings.json
    // file if it exists in the same folder as the app (make sure that the 
    // "Copy to Output Directory" property of the appsettings.json file is "Copy if newer").
    var tokenAcquirerFactory = TokenAcquirerFactory.GetDefaultInstance();
    
    // Create a downstream API service named 'MyApi' which comes loaded with several
    // utility methods to make HTTP calls to the DownstreamApi configurations found
    // in the "MyWebApi" section of your appsettings.json file.
    tokenAcquirerFactory.Services.AddDownstreamApi("MyApi",
        tokenAcquirerFactory.Configuration.GetSection("MyWebApi"));
    var sp = tokenAcquirerFactory.Build();
    ```

   Here is an example of configuration (appsettings.json file)
  ```json
  {
  	"AzureAd": {
  		"Instance": "https://login.microsoftonline.com/",
  		"TenantId": "[Enter here the tenantID or domain name for your Microsoft Entra tenant]",
  		"ClientId": "[Enter here the ClientId for your application]",
  		"ClientCredentials": [
  			{
  				"SourceType": "ClientSecret",
  				"ClientSecret": "[Enter here a client secret for your application]"
  			}
  		]
  	},

  	"MyWebApi": {
  		"BaseUrl": "https://localhost:44372/",
  		"RelativePath": "api/TodoList",
  		"RequestAppToken": true,
  		"Scopes": [ "[Enter here the scopes for your web API]" ] // . E.g. 'api://<API_APPLICATION_ID>/.default'
  	}
  }
  ```

2. Call your to-do list API
 
   The `MyApi` downstream API service comes preloaded with various utility methods to make **HTTP** calls like **GET** and **POST** and will also handle serialization and deserialization of data like **JSON** for you. Each call will automatically retrieve an access token from **Azure** which will then be cached and re-used in later calls against your protected API.

   You can read more about the `IDownstreamApi` [here](https://github.com/AzureAD/microsoft-identity-web/wiki/v2.0#idownstreamapi).

   ```CSharp
   // Extract the downstream API service from the 'tokenAcquirerFactory' service provider.
   var api = sp.GetRequiredService<IDownstreamApi>();
   
   // You can use the API service to make direct HTTP calls to your API. Token
   // acquisition is handled automatically based on the configurations in your
   // appsettings.json file.
   var result = await api.GetForAppAsync<IEnumerable<TodoItem>>("MyApi");
   Console.WriteLine($"result = {result?.Count()}");
   ```

    Note that:
    - You don't need to define the scopes here. Applications that authenticate as themselves, using client credentials, cannot specify, in the code, the individual scopes that they want to access. The scopes (app permissions) have to be statically declared during the application registration step and are done so within the `MyWebApi:Scopes` section of the `appsettings.json` file.

### The code to protect the Web API

The relevant code for the Web API is in the `Startup.cs` class. We are using the method `AddMicrosoftWebApi` to configure the Web API to authenticate using bearer tokens, validate them and protect the API from non authorized calls. These are the steps:

1. Configuring the API to authenticate using bearer tokens

    ```CSharp
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(Configuration);
    ```

2. Protecting the Web API

    Only apps that have added the **application role** created on **Microsoft Entra admin center** for the `TodoList-webapi-daemon-v2`, will contain the claim `roles` on their tokens. This is also taken care by [Microsoft Identity Web](https://github.com/AzureAD/microsoft-identity-web)

    The protection can also be done on the `Controller` level, using the `Authorize` attribute and `Policy`. Read more about [policy based authorization](https://docs.microsoft.com/aspnet/core/security/authorization/policies?view=aspnetcore-6.0):

    ```csharp
    [HttpGet]
    [Authorize(Policy = "DaemonAppRole")]
    public IActionResult Get()
    {
        ...
    }
    ```

## Troubleshooting

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

## Variation: daemon application using client credentials with certificates

Daemon applications can use two forms of secrets to authenticate themselves with Microsoft Entra ID:

- **application secrets** (also named application password). This is what we've seen so far.
- **certificates**. This is the object of this paragraph.

![Topology](./ReadmeFiles/daemon-with-certificate.svg)

To [use client credentials protocol flow with certificates](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow#second-case-access-token-request-with-a-certificate) instead of an application secret, you will need to do make a few changes to what you have done so far:

- (optionally) generate a certificate and export it, if you don't have one already
- register the certificate with your application in the application registration portal
- enable the sample code to use certificates instead of app secret.

### (Optional) Create a self-signed certificate

To complete this step, you will use the `New-SelfSignedCertificate` Powershell command. You can find more information about the New-SelfSignedCertificate command [here](https://docs.microsoft.com/powershell/module/pkiclient/new-selfsignedcertificate).

1. Open PowerShell and run `New-SelfSignedCertificate` with the following parameters to create a self-signed certificate in the user certificate store on your computer:

    ```PowerShell
    $cert=New-SelfSignedCertificate -Subject "CN=DaemonConsoleCert" -CertStoreLocation "Cert:\CurrentUser\My"  -KeyExportPolicy Exportable -KeySpec Signature
    ```

1. Export this certificate using the "Manage User Certificate" MMC snap-in accessible from the Windows Control Panel. You can also add other options to generate the certificate in a different
store such as the Computer or service store (See [How to: View Certificates with the MMC Snap-in](https://docs.microsoft.com/dotnet/framework/wcf/feature-details/how-to-view-certificates-with-the-mmc-snap-in)).

Alternatively you can use an existing certificate if you have one (just be sure to record its name for the next steps)

### Add the certificate for the daemon-console-v2 application in Microsoft Entra ID

In the application registration blade for your application, in the **Certificates & secrets** page, in the **Certificates** section:

1. Click on **Upload certificate** and, in click the browse button on the right to select the certificate you just exported (or your existing certificate)
1. Click **Add**

### Configure the Visual Studio project

To change the visual studio project to enable certificates you need to:

1. Open the `daemon-console\appsettings.json` file
2. Find the app key `ClientCredentials` and insert the `CertificateDescription` properties of your certificate within the array of credentials. You can see some examples below and read more about how to configure specific certificate descriptions [here](https://github.com/AzureAD/microsoft-identity-web/wiki/Certificates#specifying-certificates).

### Get certificate from certificate store

You can retrieve a certificate from your local store by adding the configuration below to the `ClientCredentials` array in the `appsettings.json` file in the `daemon-console` directory replacing **<CERTIFICATE_STORE_PATH>** with the store path to your certificate and **<CERTIFICATE_STORE_PATH>** with the distinguished name of your certificate. You can read more about certificate stores [here](https://docs.microsoft.com/windows-hardware/drivers/install/certificate-stores).

```json
{
  // ... 
  "ClientCredentials": [
    {
      "SourceType":  "StoreWithDistinguishedName",
      "CertificateStorePath":  "<CERTIFICATE_STORE_PATH>",
      "CertificateDistinguishedName":  "<CERTIFICATE_DISTINGUISHED_NAME>"
    }
  ]
}
```

#### Get certificate from file path

It's possible to get a certificate file, such as a **pfx** file, directly from a file path on your machine and load it into the application by using the configuration as shown below. Add a certificate descriptor in the `ClientCredentials` array of the `appsettings.json` file in the `daemon-console` directory with the snippet shown below replacing `<PATH_TO_YOUR_CERTIFICATE_FILE>` with the path to your certificate file and `<PATH_TO_YOUR_CERTIFICATE_FILE>` with that certificates password. If you like, you can use configure the `Certificate` property to reference this file and use it as a credential.

```json
{
  // ... 
  "ClientCredentials": [
    {
      "SourceType":  "Path",
      "CertificateDiskPath":  "<PATH_TO_YOUR_CERTIFICATE_FILE>",
      "CertificatePassword":  "<CERTIFICATE_PASSWORD>"
    }
  ]
}
```

#### Get certificate from Key Vault

It's also possible to get certificates from an [Azure Key Vault](https://docs.microsoft.com/azure/key-vault/general/overview). Add a certificate descriptor in the `ClientCredentials` array of the `appsettings.json` file in the `daemon-console` directory with the snippet shown below replacing `<YOUR_KEY_VAULT_URL>` with the URL of the Key Vault holding your certificate and `<YOUR_KEY_VAULT_CERTIFICATE_NAME>` with the name of that certificate as shown in your Key Vault. See the [chapter 3 readme](../3-Using-KeyVault/README.md) for more information.

```json
{
  // ... 
  "ClientCredentials": [
    {
      "SourceType":  "KeyVault",
      "KeyVaultUrl":  "<YOUR_KEY_VAULT_URL>",
      "KeyVaultCertificateName":  "<YOUR_KEY_VAULT_CERTIFICATE_NAME>"
    }
  ]
}
```

#### Build and run

Build and run your project. You have the same output, but this time, your application is authenticated with Microsoft Entra ID with the certificate instead of the application secret.

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
