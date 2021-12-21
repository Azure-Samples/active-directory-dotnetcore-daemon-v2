# A .NET Core daemon console application calling a web API using a certificate stored in an Azure Key Vault

## Overview

This sample application shows how to use the [Microsoft identity platform endpoint](http://aka.ms/aadv2) to access the data of Microsoft business customers in a long-running, non-interactive process using a certificate stored in an [Azure Key Vault](https://azure.microsoft.com/services/key-vault/).  It uses the [OAuth 2 client credentials grant](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow) to acquire an access token, which can be used to call the [Microsoft Graph](https://graph.microsoft.io) and access organizational data

## Scenario
- Acquires a certificate from an Azure Key Vault
- Uses certificate to acquire a token from Microsoft Identity Platform
- Calls the Microsoft Graph /users endpoint to get the list of user, which it then displays (as Json blob)

For more information on the concepts used in this sample, be sure to read the [Microsoft identity platform endpoint client credentials protocol documentation](https://azure.microsoft.com/documentation/articles/active-directory-v2-protocols-oauth-client-creds).

- Developers who wish to gain good familiarity of programming for Microsoft Graph are advised to go through the [An introduction to Microsoft Graph for developers](https://www.youtube.com/watch?v=EBbnpFdB92A) recorded session. 

> ### Daemon applications can use two forms of secrets to authenticate themselves with Azure AD:
>
> - **application secrets** (also named application password).
> - **certificates**.
>
> This a sample using a **certificate** stored in a **key vault** is treated over the next few paragraphs. 

## How to run this sample

To run this sample, you'll need:

- [Visual Studio](https://aka.ms/vsdownload) or just the [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
- An Internet connection
- A Windows machine (necessary if you want to run the app on Windows)
- An OS X machine (necessary if you want to run the app on Mac)
- A Linux machine (necessary if you want to run the app on Linux)
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, see [How to get an Azure AD tenant](https://azure.microsoft.com/en-us/documentation/articles/active-directory-howto-tenant/)
- A user account in your Azure AD tenant. This sample will not work with a Microsoft account (formerly Windows Live account). Therefore, if you signed in to the [Azure portal](https://portal.azure.com) with a Microsoft account and have never created a user account in your directory before, you need to do that now.

### Step 1:  Clone or download this repository

From your shell or command line:

```Shell
git clone https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2.git
```

Go to the `"3-Using-KeyVault"` folder

```Shell
cd "3-Using-KeyVault"
```

or download and extract the repository .zip file.

> Given that the name of the sample is pretty long, and so are the name of the referenced NuGet packages, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.


### Step 2:  Create an Azure Key Vault with a certificate on your tenant

In this step you'll need to create a Key Vault on your Azure tenant and then create store a certificate within that Key Vault.

You can find the instructions for creating a Key Vault [here](https://docs.microsoft.com/en-us/azure/key-vault/general/quick-create-portal).

After the Key Vault is created [upload your own certificate or create a new certificate entirely](https://docs.microsoft.com/en-us/azure/key-vault/certificates/tutorial-import-certificate) and store it in the Key Vault. To generate a certificate in the Azure portal select **Generate** as the **Method of Certificate Creation** instead of **Import** and fill in the configuration as appropriate.

If you create a new certificate you should download a CER format copy of the certificate. You'll need this in [step 4](#step-4-register-the-sample-with-your-azure-active-directory-tenant).

If you decide to create the application using the scripts found in the `AppCreationScripts-withCert` directory a certificate will be generated and registered with the application along with a **PFX** file that can be uploaded to your Key Vault. See [step 4](#step-4-register-the-sample-with-your-azure-active-directory-tenant) for more details.

### Step 3:  Update the appsettings.json file to use the certificate information in your Key Vault

In the `appsettings.json` file you'll see the `Certificate` property. Replace `<VaultUri>` with the Vault URI value for your Key Vault and `<CertificateName>` with the name of the certificate stored in your Key Vault.

```json
{
  // ...
  "Certificate": {
    "SourceType": "KeyVault",
    "KeyVaultUrl": "<VaultUri>",
    "KeyVaultCertificateName": "<CertificateName>"
  }
}
```

### Step 4:  Register the sample with your Azure Active Directory tenant

There is one project in this sample. To register it, you can:

- either follow the steps [Step 4: Register the sample with your Azure Active Directory tenant](#step-4-register-the-sample-with-your-azure-active-directory-tenant) and [Step 5:  Configure the sample to use your Azure AD tenant](#step-5-configure-the-sample-to-use-your-azure-ad-tenant)
- or use PowerShell scripts that:
  - **automatically** creates the Azure AD applications and related objects (passwords, permissions, dependencies) for you
  - modify the Visual Studio projects' configuration files.
  - creates a PFX file that can uploaded to your key vault which is already registered with your application

If you want to use this automation:
1. On Windows run PowerShell and navigate to the root of the cloned directory
1. In PowerShell run:
   ```PowerShell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
   ```
1. Run the script to create your Azure AD application and configure the code of the sample application accordingly. You will be asked for a password for the certificate PFX file that can then be uploaded to your Key Vault.
   ```cmd
   cd AppCreationScripts-withCert
   .\Configure.ps1
   ```
   > Other ways of running the scripts are described in [App Creation Scripts](./AppCreationScripts-withCert/AppCreationScripts.md)

1. Open the Visual Studio solution and click start

If you don't want to use this automation, follow the steps below

#### Choose the Azure AD tenant where you want to create your applications

As a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com) using either a work or school account or a personal Microsoft account.
1. If your account is present in more than one Azure AD tenant, select `Directory + Subscription` at the top right corner in the menu on top of the page, and switch your portal session to the desired Azure AD tenant.
1. In the left-hand navigation pane, select the **Azure Active Directory** service, and then select **App registrations**.

#### Register the client app (daemon-console)

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `daemon-console`.
   - In the **Supported account types** section, select **Accounts in this organizational directory only ({tenant name})**.
   - Select **Register** to create the application.
1. On the app **Overview** page, find the **Application (client) ID** value and record it for later. You'll need it to configure the Visual Studio configuration file for this project.

1. In the list of pages for the app, select **API permissions**
   - Click the **Add a permission** button and then,
   - Ensure that the **Microsoft APIs** tab is selected
   - In the *Commonly used Microsoft APIs* section, click on **Microsoft Graph**
   - In the **Application permissions** section, ensure that the right permissions are checked: **User.Read.All**
   - Select the **Add permissions** button

1. From the **Certificates & secrets** page, in the **Certificates** section, choose **Upload certificate** and either upload the certificate stored in the Key Vault from step 2.

1. At this stage permissions are assigned correctly but the client app does not allow interaction. 
   Therefore no consent can be presented via a UI and accepted to use the service app. 
   Click the **Grant/revoke admin consent for {tenant}** button, and then select **Yes** when you are asked if you want to grant consent for the
   requested permissions for all account in the tenant.
   You need to be an Azure AD tenant admin to do this.

### Step 5:  Configure the sample to use your Azure AD tenant

In the steps below, "ClientID" is the same as "Application ID" or "AppId".

Open the solution in Visual Studio to configure the project

#### Configure the client project

> Note: if you used the setup scripts, the changes below will have been applied for you, with the exception of the national cloud specific steps.

1. Open the `daemon-console\appsettings.json` file.
1. If you are connecting to a national cloud, change the instance to the correct Azure AD endpoint. [See this reference for a list of Azure AD endpoints.](https://docs.microsoft.com/graph/deployments#app-registration-and-token-service-root-endpoints)
1. Find the app key `Tenant` and replace the existing value with your Azure AD tenant name.
1. Find the app key `ClientId` and replace the existing value with the application ID (clientId) of the `daemon-console` application copied from the Azure portal.
1. If you are connecting to a national cloud, open the 'daemon-console\Program.cs' file.
1. Change the graph endpoint on lines in which there is a "graph.microsoft.com" reference. [See this reference for more info on which graph endpoint to use.](https://docs.microsoft.com/graph/deployments#microsoft-graph-and-graph-explorer-service-root-endpoints)

### Step 6: Run the sample

Start the application, it will display the users in the tenant.

If you're using Visual Studio run the app by cleaning the solution, rebuilding and then running it.

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRy8G199fkJNDjJ9kJaxUJIhUNUJGSDU1UkxFMlRSWUxGVTlFVkpGT0tOTi4u)

* **NOTE:** The MicrosoftIdentityWeb library uses the [DefaultAzureCredential Class](https://docs.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet) to access the certificates stored in your Key Vault. If you have multiple credentials being used on your machine it is possible that the incorrect credential will be used to access the Key Vault causing an error. See the [EnvironmentCredential Class](https://docs.microsoft.com/en-us/dotnet/api/azure.identity.environmentcredential?view=azure-dotnet) for the list of environment variables to change to use the proper credentials when accessing the Key Vault. If you have Visual Studio installed, you can set the Azure credentials used by this application by [following this guide](https://docs.microsoft.com/en-us/dotnet/azure/configure-visual-studio) and setting the **Azure Service Authentication** account as appropriate.

## About the code

The relevant code for this sample is in the `Program.cs` file, in the `RunAsync()` method. The steps are:

1. Get the information for the relevant Key Vault and certificate from the `appsettings.json` file then use the MSAL library to retrieve the certificate from the Key Vault on your tenant.

   ```CSharp
   ICertificateLoader certificateLoader = new DefaultCertificateLoader();
   certificateLoader.LoadIfNeeded(config.Certificate);
   ```

1. Create the MSAL confidential client application and use the retrieved certificate to authorize the request.

    Important note: even if we are building a console application, it is a daemon, and therefore a confidential client application, as it does not
    access Web APIs on behalf of a user, but on its own application behalf.

    ```CSharp
   IConfidentialClientApplication app;
   app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
            .WithCertificate(config.Certificate.Certificate)
            .WithAuthority(new Uri(config.Authority))
            .Build();
    ```

1. Define the scopes.

   Specific to client credentials, you don't specify, in the code, the individual scopes you want to access. You have statically declared
   them during the application registration step. Therefore the only possible scope is "resource/.default" (here "https://graph.microsoft.com/.default")
   which means "the static permissions defined in the application"

    ```CSharp
    // With client credentials flows the scopes is ALWAYS of the shape "resource/.default", as the 
    // application permissions need to be set statically (in the portal or by PowerShell), and then granted by
    // a tenant administrator
    string[] scopes = new string[] { $"{config.ApiUrl}.default" };
    ```

3. Acquire the token

    ```CSharp
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
    ```

4. Call the API

    In that case calling "https://graph.microsoft.com/v1.0/users" with the access token as a bearer token.

## Getting a secret from the Key Vault

It's possible to store and retrieve secrets directly from the Azure Key Vault. You can folow the [Azure Key Vault quick start guide](https://docs.microsoft.com/en-us/azure/key-vault/secrets/quick-create-portal#:~:text=%20To%20add%20a%20secret%20to%20the%20vault%2C,APIs%20accept%20and%20return%20secret%20values...%20More%20) to create a Key Vault on your tenant with a secret.

 You can install the following *Nuget* packages, which has helper methods to interact with Key Vault, and use the sample code:

- Microsoft.Azure.KeyVault
- Microsoft.Azure.Services.AppAuthentication

> Note: **Replace** `<keyvaultname>` and `<secretName>` with the appropriate values of your key vault and the secret to be returned

```csharp
var secret = await keyVaultClient.GetSecretAsync("https://<keyvaultname>.vault.azure.net/secrets/<secretName>")
   .ConfigureAwait(false);

Console.WriteLine($"The secret value is: {secret.Value}");
```

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


# More information

## About Azure Key Vault

Cloud applications and services use cryptographic keys and secrets to help keep information secure. [Azure Key Vault](https://azure.microsoft.com/services/key-vault/) safeguards these keys and secrets. When you use Key Vault, you can encrypt authentication keys, storage account keys, data encryption keys, .pfx files, and passwords by using keys that are protected by hardware security modules (HSMs).

## About Managed Identities for Azure Resources

Azure Key Vault provides a way to securely store credentials, secrets, and other keys, but your code has to authenticate to Key Vault to retrieve them. The [managed identities for Azure resources](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview) feature in Azure Active Directory (Azure AD) solves this problem. The feature provides Azure services with an automatically managed identity in Azure AD. You can use the identity to authenticate to any service that supports Azure AD authentication, including Key Vault, without any credentials in your code.

### **Managed Identities for Azure Resources**

In a daemon application scenario, Managed Identity will work if you have it deployed it in an [Azure Virtual Machine](https://azure.microsoft.com/services/virtual-machines/) or [Azure Web Job](https://docs.microsoft.com/en-us/azure/app-service/webjobs-create). Please, read [this documentation](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview) to understand how Managed Identity works with an Azure VM.

#### Configure Managed identity on Azure VM to access Key Vault

To authenticate to Key Vault using your Azure VM, you must first grant it permissions to Key Vault using the **Key Vault Access Policies**. To do that, follow the steps:

1. On Azure Portal, note the name of the Azure VM where you deployed the daemon application.
1. [Enable managed identity on the virtual machine](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/qs-configure-portal-windows-vm).
1. On Azure Portal, navigate to **Key Vaults** and select the one that you want the daemon application's VM to access.
1. Then click on **Access policies** menu and click on **+Add Access Policy**.
1. Select an adequate template from the dropdown "Configure from template" (ie "Secret & Certificate Management") or set the permissions manually (this sample requires the permission **GET** for Secret and Certificate to be checked).
1. For **Select principal**, search for the Azure VM *name* or *ObjectId*, select it and click on **Select** button.
1. Click on **Add**.
1. Then, **Save**.

For more information about Key Vault, take a look at these links:

- [Key Vault documentation](https://docs.microsoft.com/en-us/azure/key-vault/)
- [Managed Identity Key Vault sample for dotnet](https://github.com/Azure-Samples/app-service-msi-keyvault-dotnet)

For more information about AzureVM and Managed Identities for Azure Resources, take a look at these links:

- [Managed Identity documentation](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview)
- [AzureVM documentation](https://azure.microsoft.com/en-us/services/virtual-machines/)


For more information about the underlying protocol:

- [Microsoft identity platform and the OAuth 2.0 client credentials flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow)

For a more complex multi-tenant Web app daemon application, see [active-directory-dotnet-daemon-v2](https://github.com/Azure-Samples/active-directory-dotnet-daemon-v2)
