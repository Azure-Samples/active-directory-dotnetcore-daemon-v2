---
topic: sample
languages:
  - csharp
products:
  - microsoft-entra-id
  - dotnet
  - office-ms-graph
description: "Shows how a daemon console app uses MSAL.NET to get an access token and call Microsoft Graph."
---

# A .NET Core daemon console application calling Microsoft Graph with its own identity

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/aad%20Samples/.NET%20client%20samples/active-directory-dotnetcore-daemon-v2%20CI)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=695)

## About this sample

### Overview

This sample application shows how to use the [Microsoft identity platform](https://aka.ms/identityplatform) to access the data of Microsoft business customers in [Microsoft Graph](https://aka.ms/msgraph) in a long-running, non-interactive process.  It uses the [OAuth 2 client credentials grant](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow) to acquire an [Access Tokens](https://aka.ms/access-tokens), which is then used to call the [Microsoft Graph](https://graph.microsoft.io) API and access organizational data.

The app is a .NET Core Console application. It gets the list of users in a Microsoft Entra tenant by using the Microsoft Authentication Library for .NET ([MSAL.NET](https://aka.ms/msal-net)) to authenticate and acquire a token.

## Scenario

The console application:

- gets a token from Microsoft Entra ID for itself (without a user)
- and then calls the Microsoft Graph `/users` endpoint to get the list of users, which it then displays on the screen

![Topology](./ReadmeFiles/topology.png)

For more information on the concepts used in this sample, be sure to read the [Scenario: Daemon application that calls web APIs](https://docs.microsoft.com/azure/active-directory/develop/scenario-daemon-overview).

- Developers who wish to gain good familiarity of programming with Microsoft Graph are advised to go through the [An introduction to Microsoft Graph for developers](https://www.youtube.com/watch?v=EBbnpFdB92A) recorded session.

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

Navigate to the `"1-Call-MSGraph"` folder

```Shell
cd "1-Call-MSGraph"
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
   cd AppCreationScripts
   .\Configure.ps1
   ```

   > Other ways of running the scripts are described in [App Creation Scripts](./AppCreationScripts/AppCreationScripts.md)

1. Open the Visual Studio solution and click start

If you don't want to use this automation, follow the steps below

#### Choose the Microsoft Entra tenant where you want to create your applications

As a first step you'll need to:

1. Sign in to the [Microsoft Entra admin center](https://entra.microsoft.com) using either a work or school account or a personal Microsoft account.
1. If your account is present in more than one Microsoft Entra tenant, select `Directory + Subscription` at the top right corner in the menu on top of the page, and switch your portal session to the desired Microsoft Entra tenant.
1. In the left-hand navigation pane, select the **Microsoft Entra ID** service, and then select **App registrations**.

#### Register the client app (daemon-console)

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `daemon-console`.
   - In the **Supported account types** section, select **Accounts in this organizational directory only ({tenant name})**.
   - Select **Register** to create the application.
1. On the app **Overview** page, find the **Application (client) ID** value and record it for later. You'll need it to configure the Visual Studio configuration file for this project.
1. From the **Certificates & secrets** page, in the **Client secrets** section, choose **New client secret**:

   - Type a key description (for instance `app secret`),
   - Select a key duration of either **In 1 year**, **In 2 years**, or **Never Expires**.
   - When you press the **Add** button, the key value will be displayed, copy, and save the value in a safe location.
   - You'll need this key later to configure the project in Visual Studio. This key value will not be displayed again, nor retrievable by any other means,
     so record it as soon as it is visible from the Microsoft Entra admin center.
1. In the list of pages for the app, select **API permissions**
   - Click the **Add a permission** button and then,
   - Ensure that the **Microsoft APIs** tab is selected
   - In the *Commonly used Microsoft APIs* section, click on **Microsoft Graph**
   - In the **Application permissions** section, ensure that the right permissions are checked: **User.Read.All**
   - Select the **Add permissions** button

1. At this stage permissions are assigned correctly but a daemon client app has no user interaction. Therefore no consent can be presented via a UI when the application is running. So the consent will need to be provided in the portal itself.
   Click the **Grant/revoke admin consent for {tenant}** button, and then select **Yes** when you are asked if you want to grant consent for the  requested permission.
   You need to be a Microsoft Entra tenant admin to do this.

### Step 3:  Configure the sample to use your Microsoft Entra tenant

In the steps below, "ClientID" is the same as "Application ID" or "AppId".

Open the solution in Visual Studio to configure the project

#### Configure the client project

> Note: if you used the setup scripts, the changes below will have been applied for you, with the exception of the national cloud specific steps.

1. Open the `daemon-console\appsettings.json` file.
1. Find the app key `TenantId` and replace the existing value with your Microsoft Entra tenant name.
1. Find the app key `ClientId` and replace the existing value with the application ID (clientId) of the `daemon-console` application copied from the Microsoft Entra admin center.
1. Find the app key `ClientSecret` and replace the existing value with the key you saved during the creation of the `daemon-console` app, in the Microsoft Entra admin center.

##### If you are connecting to a national cloud, then:

1. Change the instance to the correct Microsoft Entra ID endpoint. [See this reference for a list of Microsoft Entra ID endpoints.](https://docs.microsoft.com/graph/deployments#app-registration-and-token-service-root-endpoints).
1. open the 'daemon-console\Program.cs' file and change the graph endpoint on lines in which there is a "graph.microsoft.com" reference. [See this reference for more info on which graph endpoint to use.](https://docs.microsoft.com/graph/deployments#microsoft-graph-and-graph-explorer-service-root-endpoints)

### Step 4: Run the sample

Clean the solution, rebuild the solution, and run it.

Start the application, it will display the users in the tenant.

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRy8G199fkJNDjJ9kJaxUJIhUNUJGSDU1UkxFMlRSWUxGVTlFVkpGT0tOTi4u)

## About the code

The relevant code for this sample is in the `Program.cs` file, in the `Main` method. The steps are:

1. Configure your application

    Important note: even if we are building a console application, since it is a daemon, and therefore a confidential client application, as it does not access the Web APIs on behalf of a user, but on its own (application).

    ```CSharp
    // Get the Token acquirer factory instance. By default it reads the configuration in an appsettings.json file
    // if it exists in the project.
    TokenAcquirerFactory tokenAcquirerFactory = TokenAcquirerFactory.GetDefaultInstance();

    // Configure the authentication options, add the services you need (Microsoft Graph, token cache)
    IServiceCollection services = tokenAcquirerFactory.Services;
    services.Configure<MicrosoftIdentityApplicationOptions>(
              option => tokenAcquirerFactory.Configuration.GetSection("AzureAd").Bind(option))
            .AddMicrosoftGraph()
            .AddInMemoryTokenCaches();
    // For more token cache serialization options, see https://aka.ms/msal-net-token-cache-serialization

    // Resolve the dependency injection.
    var serviceProvider = tokenAcquirerFactory.Build();
    ```

   Here is an example of configuration (appsettings.json file)
   ```json
   {
    "AzureAd": {
      "Instance": "https://login.microsoftonline.com/",
      "TenantId": "yourdomain.onmicrosoft.com",
      "ClientId": "1b4649ec-1111-2222-9821-bf5efe85ffdb",
      "ClientCredentials": [
      {
        "SourceType": "ClientSecret",
        "ClientSecret": "your client secret here"
      }
     ]
    }
   }
   ```

2. Call the Microsoft Graph API using the Graph SDK
 
   In that case calling "https://graph.microsoft.com/v1.0/users" with the access token as a bearer token.
   You get the GraphServiceClient as a required service, and use it to request the list of users in the tenant. 
   Given you are writing a daemon application, use `.WithAppOnly()` to get an app-only token.

   ```CSharp
     try
    {
      GraphServiceClient graphServiceClient = serviceProvider.GetRequiredService<GraphServiceClient>();
      var users = await graphServiceClient.Users
          .GetAsync(r => r.Options.WithAppOnly());
    }
    catch (ServiceException e)
    {
        Console.WriteLine("We could not retrieve the user's list: " + $"{e}");
    }
   ```

    Note that:
    - You don't need to define the scopes here. Applications that authenticate as themselves, using client credentials, cannot specify, in the code, the individual scopes that they want to access. The scopes (app permissions) have to be statically declared during the application registration step. Therefore the only possible scope that can be specified in the code is "resource/.default" (here, "https://graph.microsoft.com/.default"). When you use the GraphServiceClient using `WithAppOnly`, the scopes are automatically set
    to "https://graph.microsoft.com/.default" for you.
    - You don't need either to acquire a token. Microsoft.Identity.Web takes care of acquiring a token for you, and add it
      to the request made by Microsoft Graph

## Troubleshooting

### Did you forget to provide admin consent? This is needed for daemon apps

If you get an error `Insufficient privileges to complete the operation.` when calling the API, this is because the tenant administrator has not consent for the static permissions in the app registration portal. See step 6 of [Register the client app (daemon-console)](#register-the-client-app-daemon-console) above.

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

### Did you get the configuration right?

| Exception | How to fix ?|
| --------- | ---------- |
| IDW10503: Cannot determine the cloud Instance | Provide the configuration (appsettings.json with an "AzureAd" section, and "Instance" set) |
| System.ArgumentNullException: Value cannot be null. (Parameter 'tenantId') | Provide the TenantId in the configuration
| Microsoft.Identity.Client.MsalClientException: No ClientId was specified. |  Provide the ClientId in the configuration
| ErrorCode: Client_Credentials_Required_In_Confidential_Client_Application | Provide a ClientCredentials section containing either a client secret, or a certificate or Pod identity if you run in AKS
  
## Variation: daemon application using client credentials with certificates

As we had explained earlier, daemon applications can use two types of credentials to authenticate themselves with Microsoft Entra ID. In the following section we will discuss how to use a certificate instead of a client secret.

![Topology](./ReadmeFiles/topology-certificates.png)

To use certificates instead of an application secret you will need to do the following changes to what you have done so far:

- (optionally) generate a certificate and export it, i.e. if you don't have a certificate already.
- register the certificate with your application in the application registration portal
- enable the sample code to use certificates instead of the app secret.

### (Optional) use the automation script to carry out these steps

If you want to use the automation script:

1. On Windows run PowerShell and navigate to the root of the cloned directory
1. In PowerShell run:

   ```PowerShell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
   ```

1. Run the script to create your Microsoft Entra application and configure the code of the sample application accordingly. 

   ```PowerShell
   .\AppCreationScripts-withCert\Configure.ps1
   ```

   > Other ways of running the scripts are described in [App Creation Scripts](./AppCreationScripts-WithCert/AppCreationScripts.md)

If you don't want to use this automation, follow the following steps:

### (Optional) Create a self-signed certificate

To complete this step, you will use the [New-SelfSignedCertificate]((https://docs.microsoft.com/powershell/module/pkiclient/new-selfsignedcertificate)) Powershell command.

1. Open PowerShell and run `New-SelfSignedCertificate` with the following parameters to create a self-signed certificate in the user certificate store on your computer:

    ```PowerShell
    $cert=New-SelfSignedCertificate -Subject "CN=DaemonConsoleCert" -CertStoreLocation "Cert:\CurrentUser\My"  -KeyExportPolicy Exportable -KeySpec Signature
    ```

1. Export this certificate using the "Manage User Certificate" MMC snap-in accessible from the Windows Control Panel. You can also add other options to generate the certificate in a different store such as the Computer or service store (See [How to: View Certificates with the MMC Snap-in](https://docs.microsoft.com/dotnet/framework/wcf/feature-details/how-to-view-certificates-with-the-mmc-snap-in)).

Alternatively you can use an existing certificate if you have one (just be sure to record its name for the next steps)

### Add the certificate for the daemon-console application in Microsoft Entra ID

In the application registration blade for your application, in the **Certificates & secrets** page, in the **Certificates** section:

1. Click on **Upload certificate** and, in click the browse button on the right to select the certificate you just exported (or your existing certificate)
1. Click **Add**

### Configure the Visual Studio project

To change the visual studio project to enable certificates you need to:

1. Open the `daemon-console\appsettings.json` file
2. Find the app key `Certificate` and insert the `CertificateDescription` properties of your certificate. You can see some examples below and read more about how to configure certificate descriptions [here](https://github.com/AzureAD/microsoft-identity-web/wiki/Certificates#specifying-certificates).

#### Get certificate from certificate store

You can retrieve a certificate from your local store by adding the configuration below to the `Certificate` property in the `daemon-console\appsettings.json` file replacing **<CERTIFICATE_STORE_PATH>** with the store path to your certificate and **<CERTIFICATE_DISTINGUISHED_NAME>** with the distinguished name of your certificate. If you used the configuration scripts to generate the application this will be done for you using a sample self-signed certificate. You can read more about certificate stores [here](https://docs.microsoft.com/windows-hardware/drivers/install/certificate-stores).

  ```json
   {
    "AzureAd": {
      "Instance": "https://login.microsoftonline.com/",
      "TenantId": "yourdomain.onmicrosoft.com",
      "ClientId": "1b4649ec-1111-2222-9821-bf5efe85ffdb",
      "ClientCredentials": [
      {
       "SourceType":  "StoreWithDistinguishedName",
       "CertificateStorePath":  "<CERTIFICATE_STORE_PATH>",
       "CertificateDistinguishedName":  "<CERTIFICATE_DISTINGUISHED_NAME>"
      }
     ]
    }
   }
   ```  

#### Get certificate from file path

It's possible to get a certificate file, such as a **pfx** file, directly from a file path on your machine and load it into the application by using the configuration as shown below. Replace the values in the `Certificate` key of the `daemon-console\appsettings.json` file with the snippet shown below also replacing `<PATH_TO_YOUR_CERTIFICATE_FILE>` with the path to your certificate file and `<PATH_TO_YOUR_CERTIFICATE_FILE>` with that certificates password. If you created the application with the `Configure.ps1` script found in the `AppCreationScripts-withCert` a **pfx** file called **DaemonConsoleCert.pfx** will be generated that is associated with certificate used as a credential for your app. If you like, you can use configure the `Certificate` property to reference this file and use it as a credential.

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

It's also possible to get certificates from an [Azure Key Vault](https://docs.microsoft.com/azure/key-vault/general/overview). Replace the values in the `Certificate` key of the `daemon-console\appsettings.json` file with the snippet shown below also replacing `<YOUR_KEY_VAULT_URL>` with the URL of the Key Vault holding your certificate and `<YOUR_KEY_VAULT_CERTIFICATE_NAME>` with the name of that certificate as shown in your Key Vault. If you created the application with the `Configure.ps1` script found in the `AppCreationScripts-withCert` a **pfx** file called **DaemonConsoleCert.pfx** will be generated that is associated with certificate used as a credential for your app. If you like, you can load that certificate into a Key Vault and then access that Key Vault to use as a credential for your application. See the [chapter 3 readme](../3-Using-KeyVault/README.md) for more information.

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

#### About the alternate code

This application makes use of the [Microsoft Identity Web Library](https://docs.microsoft.com/azure/active-directory/develop/microsoft-identity-web) to load the certificate based on the configurations in the `daemon-console/appsettings.json` for the `ClientCredentials` property settings. 

Only the configuration changes. The rest of the application remains the same.

## Next Steps

Learn how to:

- [Create a daemon app that calls a Web API](https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2/tree/master/2-Call-OwnApi)
- [Integrate a daemon app with Key Vault and MSI](https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2/tree/master/3-Using-KeyVault)

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`msal` `dotnet`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](../../issues).

If you find a bug in msal.Net, please raise the issue on [MSAL.NET GitHub Issues](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues).

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
