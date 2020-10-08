# Using Managed Identity and Azure Key Vault in a Daemon application that authenticates with the Microsoft Identity Platform

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
