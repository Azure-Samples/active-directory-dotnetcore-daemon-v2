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
> 
> A variation of this sample using a **secret** stored in a **key vault** instead is described at the end of this article in [Variation: daemon application using client credentials with certificates](#Variation-daemon-application-using-client-credentials-with-certificates)

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

Go to the `"3-Call-MSGraph"` folder

```Shell
cd "3-Call-MSGraph"
```

or download and extract the repository .zip file.

> Given that the name of the sample is pretty long, and so are the name of the referenced NuGet packages, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.


### Step 2:  Create an Azure Key Vault with a certificate on your tenant

In this step you'll need to create a Key Vault on your Azure tenant and then create store a certificate within that Key Vault.

You can find the instructions for creating a Key Vault [here](https://docs.microsoft.com/en-us/azure/key-vault/general/quick-create-portal).

After the Key Vault is created [upload your own certificate or create a new certificate entirely](https://docs.microsoft.com/en-us/azure/key-vault/certificates/tutorial-import-certificate) and store it in the Key Vault. 

* NOTE: If you decided to create a new certificate you should download a CER format copy of the certificate. You'll need this in step 4.

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

- either follow the steps [Step 4: Register the sample with your Azure Active Directory tenant](#step-2-register-the-sample-with-your-azure-active-directory-tenant) and [Step 5:  Configure the sample to use your Azure AD tenant](#choose-the-azure-ad-tenant-where-you-want-to-create-your-applications)
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

### Step 4: Run the sample

Clean the solution, rebuild the solution, and run it.

Start the application, it will display the users in the tenant.

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRy8G199fkJNDjJ9kJaxUJIhUNUJGSDU1UkxFMlRSWUxGVTlFVkpGT0tOTi4u)

* **NOTE:** The MSAL library uses the [DefaultAzureCredential Class](https://docs.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet) to access the certificates stored in your Key Vault. If you have multiple credentials being used on your machine it is possible that the incorrect credential will be used to access the Key Vault causing an error. See the [EnvironmentCredential Class](https://docs.microsoft.com/en-us/dotnet/api/azure.identity.environmentcredential?view=azure-dotnet) for the list of environment variables to change to use the proper credentials when accessing the Key Vault.


## About Azure Key Vault

Cloud applications and services use cryptographic keys and secrets to help keep information secure. [Azure Key Vault](https://azure.microsoft.com/services/key-vault/) safeguards these keys and secrets. When you use Key Vault, you can encrypt authentication keys, storage account keys, data encryption keys, .pfx files, and passwords by using keys that are protected by hardware security modules (HSMs).

## About Managed Identities for Azure Resources

Azure Key Vault provides a way to securely store credentials, secrets, and other keys, but your code has to authenticate to Key Vault to retrieve them. The [managed identities for Azure resources](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview) feature in Azure Active Directory (Azure AD) solves this problem. The feature provides Azure services with an automatically managed identity in Azure AD. You can use the identity to authenticate to any service that supports Azure AD authentication, including Key Vault, without any credentials in your code.

## Authenticating to Key Vault with a deamon application

While there are multiple ways to authenticate to Azure Key Vault with a daemon application, this document will only discuss the following ways:

- **Managed Identities for Azure Resources** - for scenarios where the application is deployed on Azure, and the Azure resource supports Managed Identities.
- **[Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest) (for local development)** - Azure CLI version 2.0.12 and above supports the get-access-token option. AzureServiceTokenProvider uses this option to get an access token for local development environment.
- **AzureServicesAuthConnectionString (for local development)** - use for scenarios where all the previous ways are not possible, and exposing the connection string in the code is not a concern. Use it for local development only.

### **Managed Identities for Azure Resources**

In a daemon application scenario, Managed Identity will work if you have it deployed it in an [Azure Virtual Machine](https://azure.microsoft.com/services/virtual-machines/). Please, read [this documentation](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview#how-a-system-assigned-managed-identity-works-with-an-azure-vm) to understand how Managed Identity works with an Azure VM.

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

### Using Azure CLI (for local development only)

If you want to run the daemon application on your local machine and get an access token for Key Vault, you can use **Azure CLI** but some conditions must be met:

Azure CLI will work if the following conditions are met:

1. You have [Azure CLI 2.0](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest) installed. If you have an earlier version, please upgrade.
1. You are logged into Azure CLI. You can login using **az login** command.
1. The user account that you are signing in have permission to access the Key Vault.

#### Use the  AzureServicesAuthConnectionString (for local development only)

This is a method to enable development using keyvault and managed identities on your local machine. 

> Note: This method **exposes** a connection string and it is **not secure** to check in source code repositories.

To authenticate to Key Vault using a connection string, your app's service principal must have permissions on **Key Vault Access Policies**. To do that, follow the steps:

1. Note the name of your daemon application, or its objectId.
1. On Azure Portal, navigate to **Key Vaults** and select the one that you want the daemon application to access.
1. Then click on **Access policies** menu and click on **+Add Access Policy**.
1. Select an adequate template from the dropdown "Configure from template" (ie "Secret & Certificate Management") or set the permissions manually (this sample requires the permission **GET** for Secret and Certificate to be checked).
1. For **Select principal**, search for your daemon application's *name* or *ObjectId*, select it and click on **Select** button.
1. Then, click on **Add**.
1. Then, **Save**.

The authentication can be done on your local machine using your daemon application's **client secret** or **certificate**.

If you are using client secret:

- Set an environment variable named `AzureServicesAuthConnectionString` with the value, `RunAs=App;AppId=<DaemonAppId>;TenantId=<YourTenantId>;AppKey=<Secret>`. You need to replace `DaemonAppId`, `YourTenantId`, and `Secret` with actual values from your application.

If you are using certificate:

- Set an environment variable named `AzureServicesAuthConnectionString` with the value, `RunAs=App;AppId=<DaemonAppId>;TenantId=<YourTenantId>;CertificateThumbprint=<Thumbprint>;CertificateStoreLocation=CurrentUser`. You need to replace `DaemonAppId`, `YourTenantId`, and `Thumbprint` with actual values from your application.

## Getting an access token for Key Vault

To get an access token for [Azure Key Vault](https://azure.microsoft.com/services/key-vault/), you can install the following *Nuget* packages, which has helper methods to interact with Key Vault, and use the sample code:

- Microsoft.Azure.KeyVault
- Microsoft.Azure.Services.AppAuthentication

> Note: **Replace** `<keyvaultname>` and `<secretName>` with the appropriate values of your key vault and the secret to be returned

```csharp
AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
var keyVaultClient = new KeyVaultClient(
            new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

var secret = await keyVaultClient.GetSecretAsync("https://<keyvaultname>.vault.azure.net/secrets/<secretName>").ConfigureAwait(false);

Console.WriteLine($"The secret value is: {secret.Value}");

```

### Certificate

If you want to retrieve a **certificate** from KeyVault, you can read [this documentation](https://docs.microsoft.com/en-us/azure/key-vault/about-keys-secrets-and-certificates#key-vault-certificates) and use the following code sample:

> Note: **Replace** `<certificateSecretIdentifier>` with the appropriate value of your certificate secret identifier URL

```csharp
AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();
var keyVaultClient = new KeyVaultClient(
    new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

var secret = await keyVaultClient.GetSecretAsync("<certificateSecretIdentifier>").ConfigureAwait(false);
X509Certificate2 certificateWithPrivateKey = new X509Certificate2(Convert.FromBase64String(secret.Value));
```

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRy8G199fkJNDjJ9kJaxUJIhUNUJGSDU1UkxFMlRSWUxGVTlFVkpGT0tOTi4u)

## More information

For more information about Key Vault, take a look at these links:

- [Key Vault documentation](https://docs.microsoft.com/en-us/azure/key-vault/)
- [Managed Identity Key Vault sample for dotnet](https://github.com/Azure-Samples/app-service-msi-keyvault-dotnet)

For more information about AzureVM and Managed Identities for Azure Resources, take a look at these links:

- [Managed Identity documentation](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview)
- [AzureVM documentation](https://azure.microsoft.com/en-us/services/virtual-machines/)
