# A .NET Core daemon console application calling a web API using a certificate stored in an Azure Key Vault

## Overview

In this chapter, we explain how the [Call Microsoft Graph](../1-Call-MSGraph/README.md) or [Call Own API](../2-Call-OwnApi/README.md) samples can be configured to use credentials stored in the Key Vault instead of using a certificate or secret from a configuration file or a local machine certificate store.

## Scenario

- Acquire a certificate stored in an Azure Key Vault
- Use the certificate to acquire a token from Microsoft Identity Platform
- Use the retrieved token from the Microsoft Identity Platform to call a protected API. Either the Microsoft Graph `/users` endpoint to get the list of users in the [Call Microsoft Graph](../1-Call-MSGraph/README.md) sample or a protected API of **TODO** objects in the [Call Own API](../2-Call-OwnApi/README.md) sample.

## Prerequisites

To carry out these steps, you'd also need the following apart from the pre-requisites mentioned in the parent tutorial.  

- An [Azure subscription](https://azure.microsoft.com/free/).

## How to run samples using credentials from Key Vault

You'll need:

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

or download and exact the repository .zip file.

If you want to build a sample that makes a call to the **Graph API** follow the [setup instructions]("1-Call-MSGraph") and return after you have successfully registered your application.

If you want to build a sample that makes a call to a locally running API follow the [setup instructions]("2-Call-OwnApi") and return after you have successfully registered your application.

### Step 2:  Create an Azure Key Vault with a certificate on your tenant

In this step you'll need to create a Key Vault on your Azure tenant and then create store a certificate within that Key Vault.

You can find the instructions for creating a Key Vault [here](https://docs.microsoft.com/azure/key-vault/general/quick-create-portal).

After the Key Vault is created [upload your own certificate or create a new certificate entirely](https://docs.microsoft.com/azure/key-vault/certificates/tutorial-import-certificate) and store it in the Key Vault. To generate a certificate in the Azure portal select **Generate** as the **Method of Certificate Creation** instead of **Import** and fill in the configuration as appropriate.

If you create a new certificate you should download a **CER** format copy of the certificate. You'll need it to [register the certificate with your application](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app#add-credentials).

If you decide to create the application using the scripts found in the `AppCreationScripts-withCert` directory in the [Call Microsoft Graph](../1-Call-MSGraph/README.md) or the [Call Own API](../2-Call-OwnApi/README.md) sample a certificate will be generated and registered with the application along with a **PFX** file that can be [uploaded to your Key Vault](https://docs.microsoft.com/azure/key-vault/certificates/tutorial-import-certificate#import-a-certificate-to-key-vault).

### Step 3:  Update the appsettings.json file to use the certificate information in your Key Vault

In the `appsettings.json` file contained in the `daemon-console` directory of either app, replace the content of `ClientCredentials` with the following. Replace `<VaultUri>` with the Vault URI value for your Key Vault and `<CertificateName>` with the name of the certificate stored in your Key Vault.

```json
		"ClientCredentials": [
			{
				"SourceType": "KeyVault",
				"KeyVaultUrl": "<VaultUri>",
				"KeyVaultCertificateName": "<CertificateName>"
			}
		]
```

### Step 4: Run the sample

Start the application.

If you're using Visual Studio run the app by cleaning the solution, rebuilding and then running it.

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRy8G199fkJNDjJ9kJaxUJIhUNUJGSDU1UkxFMlRSWUxGVTlFVkpGT0tOTi4u)

**NOTE:** The MicrosoftIdentityWeb library uses the [DefaultAzureCredential Class](https://docs.microsoft.com/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet) to access the certificates stored in your Key Vault. If you have multiple credentials being used on your machine it is possible that the incorrect credential will be used to access the Key Vault causing an error. See the [EnvironmentCredential Class](https://docs.microsoft.com/dotnet/api/azure.identity.environmentcredential?view=azure-dotnet) for the list of environment variables to change to use the proper credentials when accessing the Key Vault. If you have Visual Studio installed, you can set the Azure credentials used by this application by [following this guide](https://docs.microsoft.com/dotnet/azure/configure-visual-studio) and setting the **Azure Service Authentication** account as appropriate.

## About the code

The relevant code for this sample is in the `Program.cs` file, in the `RunAsync()` method. The steps are:

1. Get the information for the relevant Key Vault and certificate from the `appsettings.json` file then use the MSAL library to retrieve the certificate from the Key Vault on your tenant.

This sample makes use of the **DefaultCertificateLoader** from the [Microsoft Identity Web.Certificate](https://docs.microsoft.com/azure/active-directory/develop/microsoft-identity-web) library. If you've configured your application using the steps above the loader will retrieve the certificate from the Key Vault as specified and store it within the `config` object. Accessing the certificate is discussed in more detail below.

Important note: The MicrosoftIdentityWeb library uses the [DefaultAzureCredential Class](https://docs.microsoft.com/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet) to access the certificates stored in your Key Vault. If you have multiple credentials being used on your machine it is possible that the incorrect credential will be used to access the Key Vault causing an error. See the [EnvironmentCredential Class](https://docs.microsoft.com/dotnet/api/azure.identity.environmentcredential?view=azure-dotnet) for the list of environment variables to change to use the proper credentials when accessing the Key Vault. If you have Visual Studio installed, you can set the Azure credentials used by [following this guide](https://docs.microsoft.com/dotnet/azure/configure-visual-studio) and setting the **Azure Service Authentication** account to one with the appropriate permissions for the target tenant.

```CSharp
ICertificateLoader certificateLoader = new DefaultCertificateLoader();
certificateLoader.LoadIfNeeded(config.Certificate);
```

1. Create the MSAL confidential client application and use the retrieved certificate to authorize the request. The certificate loaded by the `DefaultCertificateLoader` is made available as a [X509Certificate2](https://docs.microsoft.com/dotnet/api/system.security.cryptography.x509certificates.x509certificate2?view=net-6.0) by accessing the `config` object as `config.Certificate.Certificate`. By default this value is null but it is set after the call to `LoadIfNeeded` is run successfully.

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

1. Acquire the token

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

1. Call the API

    In that case calling "https://graph.microsoft.com/v1.0/users" with the access token as a bearer token.

## Getting a secret from the Key Vault

It's possible to store and retrieve secrets directly from the Azure Key Vault. You can follow the [Azure Key Vault quick start guide](https://docs.microsoft.com/azure/key-vault/secrets/quick-create-portal#:~:text=%20To%20add%20a%20secret%20to%20the%20vault%2C,APIs%20accept%20and%20return%20secret%20values...%20More%20) to create a Key Vault on your tenant with a secret.

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

## More information

### About Azure Key Vault

Cloud applications and services use cryptographic keys and secrets to help keep information secure. [Azure Key Vault](https://azure.microsoft.com/services/key-vault/) safeguards these keys and secrets. When you use Key Vault, you can encrypt authentication keys, storage account keys, data encryption keys, .pfx files, and passwords by using keys that are protected by hardware security modules (HSMs).

### About Managed Identities for Azure Resources

Azure Key Vault provides a way to securely store credentials, secrets, and other keys, but your code has to authenticate to Key Vault to retrieve them. The [managed identities for Azure resources](https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/overview) feature in Azure Active Directory (Azure AD) solves this problem. The feature provides Azure services with an automatically managed identity in Azure AD. You can use the identity to authenticate to any service that supports Azure AD authentication, including Key Vault, without any credentials in your code.

In a daemon application scenario, Managed Identity will work if you have it deployed it in an [Azure Virtual Machine](https://azure.microsoft.com/services/virtual-machines/) or [Azure Web Job](https://docs.microsoft.com/azure/app-service/webjobs-create). Please, read [this documentation](https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/overview) to understand how Managed Identity works with an Azure VM.

#### Configure Managed identity on Azure VM to access Key Vault

To authenticate to Key Vault using your Azure VM, you must first grant it permissions to Key Vault using the **Key Vault Access Policies**. To do that, follow the steps:

1. On Azure Portal, note the name of the Azure VM where you deployed the daemon application.
1. [Enable managed identity on the virtual machine](https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/qs-configure-portal-windows-vm).
1. On Azure Portal, navigate to **Key Vaults** and select the one that you want the daemon application's VM to access.
1. Then click on **Access policies** menu and click on **+Add Access Policy**.
1. Select an adequate template from the dropdown "Configure from template" (ie "Secret & Certificate Management") or set the permissions manually (this sample requires the permission **GET** for Secret and Certificate to be checked).
1. For **Select principal**, search for the Azure VM *name* or *ObjectId*, select it and click on **Select** button.
1. Click on **Add**.
1. Then, **Save**.

For more information about Key Vault, take a look at these links:

- [Key Vault documentation](https://docs.microsoft.com/azure/key-vault/)
- [Managed Identity Key Vault sample for dotnet](https://github.com/Azure-Samples/app-service-msi-keyvault-dotnet)

For more information about AzureVM and Managed Identities for Azure Resources, take a look at these links:

- [Managed Identity documentation](https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/overview)
- [AzureVM documentation](https://azure.microsoft.com/services/virtual-machines/)

For more information about the underlying protocol:

- [Microsoft identity platform and the OAuth 2.0 client credentials flow](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow)

For a more complex multi-tenant Web app daemon application, see [active-directory-dotnet-daemon-v2](https://github.com/Azure-Samples/active-directory-dotnet-daemon-v2)
