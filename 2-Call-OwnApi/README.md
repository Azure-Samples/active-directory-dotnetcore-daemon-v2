---
page_type: sample
name: A .NET Core daemon console application authenticating as itself and calling a custom protected Web API
description: This sample demonstrates a .NET core console application obtaining an Access Token using client credentials (app-only flow) for a custom Web API protected with Microsoft Identity Platform and Microsoft Graph.
languages:
 - dotnet-core
 - dotnet-csharp
 - aspnetcore
 - csharp
products:
 - azure-active-directory
 - ms-graph
 - microsoft-identity-web
urlFragment: active-directory-dotnetcore-daemon-v2
extensions:
- services: ms-identity
- platform: DotNet
- endpoint: AAD v2.0
- level: 200
- client: .NET Core (Console)
- service: .NET Core Web API
---

# A .NET Core daemon console application authenticating as itself and calling a custom protected Web API

* [Overview](#overview)
* [Scenario](#scenario)
* [Prerequisites](#prerequisites)
* [Setup the sample](#setup-the-sample)
* [Explore the sample](#explore-the-sample)
* [Troubleshooting](#troubleshooting)
* [About the code](#about-the-code)
* [How to deploy this sample to Azure](#how-to-deploy-this-sample-to-azure)
* [Next Steps](#next-steps)
* [Contributing](#contributing)
* [Learn More](#learn-more)

## Overview

This sample demonstrates a .NET Core (Console) authenticating with Azure AD using the [OAuth 2\.0 client credentials flow](https://learn.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow) and calling a protected .NET Core Web API that is also secured with Azure AD.

## Scenario

1. The client .NET Core (Console) uses [MSAL.NET](https://aka.ms/msal-net) to authenticate with Azure AD and obtain a JWT [Access Token](https://aka.ms/access-tokens) from **Azure AD** using its own identity for Microsoft Graph.
1. The **access token** is used as a *bearer* token to call the Microsoft Graph API and retrieve a list of users in the tenant.
1. The client app then proceeds to get another Access token for the .NET Core Web API.
1. The service uses the [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web) to protect the Web api, check permissions and validate tokens.
1. The **access token** is used as a *bearer* token to call the .NET Core Web API and to generate a set of random To-Do list items for users in the tenant.
1. The console app then proceeds to fetch the To-Do list items from the API and displays them on the console.

![Scenario Image](./ReadmeFiles/topology.png)

## Prerequisites

* Either [Visual Studio](https://visualstudio.microsoft.com/downloads/) or [Visual Studio Code](https://code.visualstudio.com/download) and [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
* An **Azure AD** tenant. For more information, see: [How to get an Azure AD tenant](https://docs.microsoft.com/azure/active-directory/develop/test-setup-environment#get-a-test-tenant)
* A user account in your **Azure AD** tenant.

## Setup the sample

### Step 1: Clone or download this repository

From your shell or command line:

```console
git clone https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2.git
```

or download and extract the repository *.zip* file.

> :warning: To avoid path length limitations on Windows, we recommend cloning into a directory near the root of your drive.

### Step 2: Navigate to project folder

You don't have to change current folder.

### Step 3: Register the sample application(s) in your tenant

There are two projects in this sample. Each needs to be separately registered in your Azure AD tenant. To register these projects, you can:

- follow the steps below for manually register your apps
- or use PowerShell scripts that:
  - **automatically** creates the Azure AD applications and related objects (passwords, permissions, dependencies) for you.
  - modify the projects' configuration files.

<details>
   <summary>Expand this section if you want to use this automation:</summary>

> :warning: If you have never used **Microsoft Graph PowerShell** before, we recommend you go through the [App Creation Scripts Guide](./AppCreationScripts/AppCreationScripts.md) once to ensure that your environment is prepared correctly for this step.
  
  1. On Windows, run PowerShell as **Administrator** and navigate to the root of the cloned directory
  2. In PowerShell run:

     ```PowerShell
     Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
     ```

  3. Run the script to create your Azure AD application and configure the code of the sample application accordingly.
  4. For interactive process -in PowerShell, run:

     ```PowerShell
     cd .\AppCreationScripts\
     .\Configure.ps1 -TenantId "[Optional] - your tenant id" -AzureEnvironmentName "[Optional] - Azure environment, defaults to 'Global'"
     ```

> Other ways of running the scripts are described in [App Creation Scripts guide](./AppCreationScripts/AppCreationScripts.md). The scripts also provide a guide to automated application registration, configuration and removal which can help in your CI/CD scenarios.

</details>

#### Choose the Azure AD tenant where you want to create your applications

To manually register the apps, as a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com).
1. If your account is present in more than one Azure AD tenant, select your profile at the top right corner in the menu on top of the page, and then **switch directory** to change your portal session to the desired Azure AD tenant.

#### Register the service app (TodoList-webapi-daemon-v2)

1. Navigate to the [Azure portal](https://portal.azure.com) and select the **Azure Active Directory** service.
1. Select the **App Registrations** blade on the left, then select **New registration**.
1. In the **Register an application page** that appears, enter your application's registration information:
    1. In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `TodoList-webapi-daemon-v2`.
    1. Under **Supported account types**, select **Accounts in this organizational directory only**
    1. Select **Register** to create the application.
1. In the **Overview** blade, find and note the **Application (client) ID**. You use this value in your app's configuration file(s) later in your code.
1. In the app's registration screen, select the **Expose an API** blade to the left to open the page where you can publish the permission as an API for which client applications can obtain [access tokens](https://aka.ms/access-tokens) for. The first thing that we need to do is to declare the unique [resource](https://learn.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow) URI that the clients will be using to obtain access tokens for this API. To declare an resource URI(Application ID URI), follow the following steps:
    1. Select **Set** next to the **Application ID URI** to generate a URI that is unique for this app.
    1. For this sample, accept the proposed Application ID URI (`api://{clientId}`) by selecting **Save**.
        > :information_source: Read more about Application ID URI at [Validation differences by supported account types (signInAudience)](https://docs.microsoft.com/azure/active-directory/develop/supported-accounts-validation).

##### Publish Delegated Permissions

1. All APIs must publish a minimum of one [scope](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-auth-code-flow#request-an-authorization-code), also called [Delegated Permission](https://docs.microsoft.com/azure/active-directory/develop/v2-permissions-and-consent#permission-types), for the client apps to obtain an access token for a *user* successfully. To publish a scope, follow these steps:
1. Select **Add a scope** button open the **Add a scope** screen and Enter the values as indicated below:
    1. For **Scope name**, use `ToDoList.Read`.
    1. Select **Admins and users** options for **Who can consent?**.
    1. For **Admin consent display name** type in *Read users ToDo list using the 'TodoList-webapi-daemon-v2'*.
    1. For **Admin consent description** type in *Allow the app to read the user's ToDo list using the 'TodoList-webapi-daemon-v2'*.
    1. For **User consent display name** type in *Read your ToDo list items via the 'TodoList-webapi-daemon-v2'*.
    1. For **User consent description** type in *Allow the app to read your ToDo list items via the 'TodoList-webapi-daemon-v2'*.
    1. Keep **State** as **Enabled**.
    1. Select the **Add scope** button on the bottom to save this scope.
    > Repeat the steps above for another scope named **ToDoList.ReadWrite**
1. Select the **Manifest** blade on the left.
    1. Set `accessTokenAcceptedVersion` property to **2**.
    1. Select on **Save**.

> :information_source:  Follow [the principle of least privilege when publishing permissions](https://learn.microsoft.com/security/zero-trust/develop/protected-api-example) for a web API.

##### Publish Application Permissions

1. All APIs should publish a minimum of one [App role for applications](https://docs.microsoft.com/azure/active-directory/develop/howto-add-app-roles-in-azure-ad-apps#assign-app-roles-to-applications), also called [Application Permission](https://docs.microsoft.com/azure/active-directory/develop/v2-permissions-and-consent#permission-types), for the client apps to obtain an access token as *themselves*, i.e. when they are not signing-in a user. **Application permissions** are the type of permissions that APIs should publish when they want to enable client applications to successfully authenticate as themselves and not need to sign-in users. To publish an application permission, follow these steps:
1. Still on the same app registration, select the **App roles** blade to the left.
1. Select **Create app role**:
    1. For **Display name**, enter a suitable name for your application permission, for instance **ToDoList.Read.All**.
    1. For **Allowed member types**, choose **Application** to ensure other applications can be granted this permission.
    1. For **Value**, enter **ToDoList.Read.All**.
    1. For **Description**, enter *Allow the app to read every user's ToDo list using the 'TodoList-webapi-daemon-v2'*.
    1. Select **Apply** to save your changes.

    > Repeat the steps above for another app permission named **ToDoList.ReadWrite.All**

##### Configure Optional Claims

1. Still on the same app registration, select the **Token configuration** blade to the left.
1. Select **Add optional claim**:
    1. Select **optional claim type**, then choose **Access**.
     1. Select the optional claim **idtyp**.
    > Indicates token type. This claim is the most accurate way for an API to determine if a token is an app token or an app+user token. This is not issued in tokens issued to users.
    1. Select **Add** to save your changes.

##### Configure the service app (TodoList-webapi-daemon-v2) to use your app registration

Open the project in your IDE (like Visual Studio or Visual Studio Code) to configure the code.

> In the steps below, "ClientID" is the same as "Application ID" or "AppId".

1. Open the `TodoList-WebApi\appsettings.json` file.
1. Find the key `Domain` and replace the existing value with your Azure AD tenant domain, ex. `contoso.onmicrosoft.com`.
1. Find the key `TenantId` and replace the existing value with your Azure AD tenant/directory ID.
1. Find the key `ClientId` and replace the existing value with the application ID (clientId) of `TodoList-webapi-daemon-v2` app copied from the Azure portal.

#### Register the client app (daemon-console-v2)

1. Navigate to the [Azure portal](https://portal.azure.com) and select the **Azure Active Directory** service.
1. Select the **App Registrations** blade on the left, then select **New registration**.
1. In the **Register an application page** that appears, enter your application's registration information:
    1. In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `daemon-console-v2`.
    1. Under **Supported account types**, select **Accounts in this organizational directory only**
    1. Select **Register** to create the application.
1. In the **Overview** blade, find and note the **Application (client) ID**. You use this value in your app's configuration file(s) later in your code.
1. In the app's registration screen, select the **Certificates & secrets** blade in the left to open the page where you can generate secrets and upload certificates.
1. In the **Client secrets** section, select **New client secret**:
    1. Type a key description (for instance `app secret`).
    1. Select one of the available key durations (**6 months**, **12 months** or **Custom**) as per your security posture.
    1. The generated key value will be displayed when you select the **Add** button. Copy and save the generated value for use in later steps.
    1. You'll need this key later in your code's configuration files. This key value will not be displayed again, and is not retrievable by any other means, so make sure to note it from the Azure portal before navigating to any other screen or blade.
    1. For enhanced security, instead of using client secrets, we'd be [using certificates](./README-use-certificate.md).
    1. Since this app signs-in as itself using the [OAuth 2\.0 client credentials flow](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow), we will now proceed to select **application permissions**, which is required by apps authenticating as themselves.
    1. In the app's registration screen, select the **API permissions** blade in the left to open the page where we add access to the APIs that your application needs:
    1. Select the **Add a permission** button and then:
    1. Ensure that the **My APIs** tab is selected.
    1. In the list of APIs, select the API `TodoList-webapi-daemon-v2`.
        1. We will select “Application permissions”, which should be the type of permissions that apps should use when they are authenticating just as themselves and not signing-in users.
   1. In the **Application permissions** section, select the **ToDoList.Read.All|ToDoList.ReadWrite.All** in the list. Use the search box if necessary.
    1. Select the **Add permissions** button at the bottom.
    1. Select the **Add a permission** button and then:
    1. Ensure that the **Microsoft APIs** tab is selected.
    1. In the *Commonly used Microsoft APIs* section, select **Microsoft Graph**
        1. We will select “Application permissions”, which should be the type of permissions that apps should use when they are authenticating just as themselves and not signing-in users.
   1. In the **Application permissions** section, select the **User.Read.All** in the list. Use the search box if necessary.
    1. Select the **Add permissions** button at the bottom.
1. At this stage, the permissions are assigned correctly but since the client app does not allow users to interact, the users' themselves cannot consent to these permissions. To get around this problem, we'd let the [tenant administrator consent on behalf of all users in the tenant](https://docs.microsoft.com/azure/active-directory/develop/v2-admin-consent). Select the **Grant admin consent for {tenant}** button, and then select **Yes** when you are asked if you want to grant consent for the requested permissions for all accounts in the tenant. You need to be a tenant admin to be able to carry out this operation.

##### Configure the client app (daemon-console-v2) to use your app registration

Open the project in your IDE (like Visual Studio or Visual Studio Code) to configure the code.

> In the steps below, "ClientID" is the same as "Application ID" or "AppId".

1. Open the `Daemon-Console\appsettings.json` file.
1. Find the key `Tenant` and replace the existing value with your Azure AD tenant domain, ex. `contoso.onmicrosoft.com`.
1. Find the key `ClientId` and replace the existing value with the application ID (clientId) of `daemon-console-v2` app copied from the Azure portal.
1. Find the key `CertificateName` and replace the existing value with Certificate.
1. Find the key `TodoListScope` and replace the existing value with ScopeDefault.
1. Find the key `TodoListBaseAddress` and replace the existing value with the base address of `TodoList-webapi-daemon-v2` (by default `https://localhost:44372`).
1. Find the key `Scopes` and replace the existing value with ScopeDefault.
1. **[OPTIONAL]** Find the app key `ClientCredentials` and add the keys as displayed below if you want to directly reference a certificate in a file path:

```JSON
    "ClientCredentials": [
        {
            "SourceType": "Path",
            "CertificateDiskPath": "<PATH_TO_CERTIFICATE>",
            "CertificatePassword": "<CERTIFICATE_PASSWORD>"
        }
    ]
```

1. Update values of the keys:
    1 .`SourceType` to `Path`.
    1. `CertificateDiskPath` to the path where certificate exported with private key (CN=daemon-console-v2.pfx) is stored. For example, `C:\\AppCreationScripts\CN=daemon-console-v2.pfx`
    1. `CertificatePassword` add the password used while exporting the certificate.
1. If you had set `ClientSecret` previously, set its value to empty string, `""`.

You can also reference certificates in your machine's local stores like so:

```JSON
    "ClientCredentials": [
        {
            "SourceType": "StoreWithDistinguishedName",
            "CertificateDiskPath": "<CERTIFICATE_STORE_PATH>", // Store path or your certificate. E.g. 'CurrentUser/My'",
            "CertificatePassword": "CertificateDistinguishedName": "<CERTIFICATE_DISTINGUISHED_NAME>" // Distinguished name of your certificate. E.g. 'CN=MyAppCertificate'
        }
    ]
```

> :information_source: For other alternatives, see: [Using certificates with Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web/wiki/Certificates#specifying-certificates)

### Variation: Using certificates instead of client secrets 

Follow [README-use-certificate.md](README-use-certificate.md) to know how to use this option.

### Step 4: Running the sample

From your shell or command line, execute the following commands:

```console
    # You don't have to change to current folder.
    dotnet run
```

Then, open a separate command terminal and run:

```console
    # You don't have to change to current folder.
    dotnet run
```

### (Optional) Create a self-signed certificate

Microsoft identity platform supports two types of authentication for [confidential client applications](https://learn.microsoft.com/azure/active-directory/develop/msal-client-applications): password-based authentication (i.e. client secret) and certificate-based authentication. For a higher level of security, we recommend using a certificate (instead of a client secret) as a credential in your confidential client applications.

In production, you should purchase a certificate signed by a well-known certificate authority, and use [Azure Key Vault](https://azure.microsoft.com/services/key-vault/) to manage certificate access and lifetime for you. For testing purposes, follow the steps below to create a self-signed certificate and configure your apps to authenticate with certificates.

### Using certificates

- **Step 1: [Create a self-signed certificate](#create-a-self-signed-certificate)**
  > :information_source: if you already have a valid certificate, skip to step 2
  - Option 1: [create self-signed certificate on local machine](#create-self-signed-certificate-on-local-machine)
  - Option 2: [create self-signed certificate on Key Vault](#create-self-signed-certificate-on-key-vault)
- **Step 2: [Configure an Azure AD app registration to use a certificate](#configure-an-azure-ad-app-registration-to-use-a-certificate)**
- **Step 3: [Configure your app(s) to use a certificate](#configure-your-apps-to-use-a-certificate)**
  - Option 1: [using an existing certificate from local machine](#using-an-existing-certificate-from-local-machine)
  - Option 2: [using an existing certificate from Key Vault](#using-an-existing-certificate-from-key-vault)

If you plan to deploy your app(s) to [Azure App Service](https://learn.microsoft.com/azure/app-service/overview) afterwards, we recommend [Azure Managed Identity](https://learn.microsoft.com/azure/active-directory/managed-identities-azure-resources/overview) to completely eliminate secrets, certificates, connection strings and etc. from your source code. See [Using Managed Identity](#using-managed-identity) below for more.

### Create a self-signed certificate

You can skip this step if you already have a valid self-signed certificate at hand.

#### Create self-signed certificate on local machine

If you wish to generate a new self-signed certificate yourself, follow the steps below.

<details>
<summary>Click here to use Powershell</summary>

To generate a new self-signed certificate, we will use the [New-SelfSignedCertificate](https://docs.microsoft.com/powershell/module/pkiclient/new-selfsignedcertificate) Powershell command.

Open PowerShell and run the command with the following parameters to create a new self-signed certificate that will be stored in the **current user** certificate store on your computer:

```PowerShell
$cert = New-SelfSignedCertificate -Subject "CN=<CertificateName>" -CertStoreLocation "Cert:\CurrentUser\My" -KeyExportPolicy Exportable -KeySpec Signature -KeyLength 2048 -KeyAlgorithm RSA -HashAlgorithm SHA256
```

You can now export a public key (*.cer* file) and a public + private key combination (*.pfx* file) to use in your app:

```Powershell
Export-Certificate -Cert $cert -FilePath "C:\Users\admin\Desktop\<CertificateName>.cer" ## Specify your preferred location

$mypwd = ConvertTo-SecureString -String "{myPassword}" -Force -AsPlainText  ## Replace {myPassword}
Export-PfxCertificate -Cert $cert -FilePath "C:\Users\admin\Desktop\<CertificateName>.pfx" -Password $mypwd ## Specify your preferred location
```

Proceed to [Step 2](#configure-an-azure-ad-app-registration-to-use-a-certificate).

> :information_source: For more details, follow the guide: [Create a self-signed public certificate to authenticate your application](https://learn.microsoft.com//azure/active-directory/develop/howto-create-self-signed-certificate)

</details>

<details>
<summary>Click here to use OpenSSL</summary>

Download and build **OpenSSL** for your **OS** following the guide at [github.com/openssl](https://github.com/openssl/openssl#build-and-install). If you like to skip building and get a binary distributable from the community instead, check the [OpenSSL Wiki: Binaries](https://wiki.openssl.org/index.php/Binaries) page. Afterwards, add the path to **OpenSSL** to your **environment variables** so that you can call it from anywhere.

Type the following in a terminal. The files will be generated in the terminals current directory.

```bash
openssl req -x509 -newkey rsa:2048 -keyout <CertificateName>.key -out <CertificateName>.cer -subj "/CN=<CertificateName>" -nodes

Generating a RSA private key
.........................................................
writing new private key to '<CertificateName>.key'
```

The following files should be generated: *<CertificateName>.key*, *<CertificateName>.cer*

You can generate a <CertificateName>.pfx (certificate + private key combination) with the command below:

```bash
openssl pkcs12 -export -out CertificateName.pfx -inkey <CertificateName>.key -in <CertificateName>.cer
```

Enter an export password when prompted and make a note of it. The following file should be generated: *CertificateName.pfx*.

Proceed to [Step 2](#configure-an-azure-ad-app-registration-to-use-a-certificate).

</details>

> :information_source: If you wish so, you can upload your locally generated self-signed certificate to Azure Key Vault later on. See: [Import a certificate in Azure Key Vault](https://learn.microsoft.com/azure/key-vault/certificates/tutorial-import-certificate)

#### Create self-signed certificate on Key Vault

You can use Azure Key Vault to generate a self-signed certificate for you. Doing so will have the additional benefits of assigning a partner Certificate Authority (CA) and automating certificate rotation.

<details>
<summary>Click here to use Azure Portal</summary>

Follow the guide: [Set and retrieve a certificate from Azure Key Vault using the Azure portal](https://learn.microsoft.com/azure/key-vault/certificates/quick-create-portal)

Afterwards, proceed to [Step 2](#configure-an-azure-ad-app-registration-to-use-a-certificate).

</details>

<details>
<summary>Click here to use Powershell</summary>

Follow the guide: [Set and retrieve a certificate from Azure Key Vault using Azure PowerShell](https://learn.microsoft.com/azure/key-vault/certificates/quick-create-powershell)

Afterwards, proceed to [Step 2](#configure-an-azure-ad-app-registration-to-use-a-certificate).

</details>

### Configure an Azure AD app registration to use a certificate

Now you must associate your Azure AD app registration with the certificate you will use in your application.

> information_source: If you have the certificate locally available, you can follow the steps below. If your certificate(s) is on Azure Key Vault, you must first export and download them to your computer, and delete the local copy after following the steps below. See: [Export certificates from Azure Key Vault](https://learn.microsoft.com/azure/key-vault/certificates/how-to-export-certificate)

1. Navigate to [Azure portal](https://portal.azure.com) and select your Azure AD app registration.
1. Select **Certificates & secrets** blade on the left.
1. Click on **Upload** certificate and select the certificate file to upload (e.g. *example.crt* or *example.cer*).
1. Click **Add**. Once the certificate is uploaded, the *thumbprint*, *start date*, and *expiration* values are displayed. Record the *thumbprint* value as you will make use of it later in your app's configuration file.

> For more information, see: [Register your certificate with the Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/active-directory-certificate-credentials#register-your-certificate-with-microsoft-identity-platform)

Proceed to [Step 3](#configure-your-apps-to-use-a-certificate)

### Configure your app(s) to use a certificate

Finally, you need to modify the app's configuration files.

#### Using an existing certificate from local machine

> Perform the steps below for the client app (daemon-console-v2)

1. Open the `appsettings.json` file.
2. *Comment out* the following lines:

```json
// "SourceType": "ClientSecret",
// "ClientSecret": "[Enter here a client secret for your application]"
```

3. *Un-comment* the following lines and replace the default values with the storepath and distinguised name of your certificate:

```json
"SourceType": "StoreWithDistinguishedName",
"CertificateStorePath": "<CERTIFICATE_STORE_PATH>", // Store path or your certificate. E.g. 'CurrentUser/My'
"CertificateDistinguishedName": "<CERTIFICATE_DISTINGUISHED_NAME>"  // Distinguished name of your certificate. E.g. 'CN=daemon-console-v2'
```

4. You can now start the application as instructed in the [README](./README#setup-the-sample).

> :information_source: For other alternatives, see: [Using certificates with Microsoft.Identity.Web](https://github.com/AzureAD/microsoft-identity-web/wiki/Certificates#specifying-certificates)

                    
#### Using an existing certificate from Key Vault

Comment out the **ClientSecret** sections like above and add the following object in the `ClientCredentials` array:

```json
{
  "SourceType":  "KeyVault",
  "KeyVaultUrl":  "<YOUR_KEY_VAULT_URL>",
  "KeyVaultCertificateName":  "<YOUR_KEY_VAULT_CERTIFICATE_NAME>"
}
```

## Using Managed Identity

Once you deploy your app(s) to Azure App Service, you can assign a managed identity to it for accessing Azure Key Vault using its own identity. This allows you to eliminate the all secrets, certificates, connection strings and etc. from your source code.

### Create a system-assigned identity

1. Navigate to [Azure portal](https://portal.azure.com) and select the **Azure App Service**.
1. Find and select the App Service instance you've created previously.
1. On App Service portal, select **Identity**.
1. Within the **System assigned** tab, switch **Status** to **On**. Click **Save**.

For more information, see [Add a system-assigned identity](https://docs.microsoft.com/azure/app-service/overview-managed-identity?tabs=dotnet#add-a-system-assigned-identity)

### Grant access to Key Vault

Now that your app deployed to App Service has a managed identity, in this step you grant it access to your key vault.

1. Go to the [Azure portal](https://portal.azure.com) and search for your Key Vault.
1. Select **Overview** > **Access policies** blade on the left.
1. Click on **Add Access Policy** > **Certificate permissions** > **Get**
1. Click on **Add Access Policy** > **Secret permissions** > **Get**
1. Click on **Select Principal**, add your account and pre-created **system-assigned** identity.
1. Click on **OK** to add the new Access Policy, then click **Save** to save the Access Policy.

For more information, see [Use Key Vault from App Service with Azure Managed Identity](https://docs.microsoft.com/samples/azure-samples/app-service-msi-keyvault-dotnet/keyvault-msi-appservice-sample/)

### Add environment variables

Finally, you need to add environment variables to the App Service where you deployed your app.

1. In the [Azure portal](https://portal.azure.com), search for and select **App Service**, and then select your app.
1. Select **Configuration** blade on the left, then select **New Application Settings**.
1. Add the following variables (key-value pairs):
    1. **KEY_VAULT_NAME**: the name of the key vault you've created, e.g. `msal-test-vault`
    1. **CERTIFICATE_NAME**: the name of the certificate you specified when importing it to key vault, e.g. `ExampleCert`

Wait for a few minutes for your changes on **App Service** to take effect. You should then be able to visit your published website and sign-in accordingly.

## More information

- [Microsoft identity platform application authentication certificate credentials](https://docs.microsoft.com/azure/active-directory/develop/active-directory-certificate-credentials)
- [Create a self-signed public certificate to authenticate your application](https://docs.microsoft.com/azure/active-directory/develop/howto-create-self-signed-certificate)
- [Various SSL/TLS certificate file types/extensions](https://docs.microsoft.com/archive/blogs/kaushal/various-ssltls-certificate-file-typesextensions)
- [Azure Key Vault Developer's Guide](https://docs.microsoft.com/azure/key-vault/general/developers-guide)
- [Managed identities for Azure resources](https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/overview)

### Build and run

Build and run your project. You have the same output, but this time, your application is authenticated with Azure AD with the certificate instead of the application secret.

## We'd love your feedback!

Were we successful in addressing your learning objective? Consider taking a moment to [share your experience with us](Enter_Survey_Form_Link).

## Troubleshooting

<details>
	<summary>Expand for troubleshooting info</summary>

ASP.NET core applications create session cookies that represent the identity of the caller. Some Safari users using iOS 12 had issues which are described in ASP.NET Core #4467 and the Web kit bugs database Bug 188165 - iOS 12 Safari breaks ASP.NET Core 2.1 OIDC authentication.

If your web site needs to be accessed from users using iOS 12, you probably want to disable the SameSite protection, but also ensure that state changes are protected with CSRF anti-forgery mechanism. See the how to fix section of Microsoft Security Advisory: iOS12 breaks social, WSFed and OIDC logins #4647
</details>

## About the code

The relevant code for this sample is in the `Program.cs` file.

1. Inject the relevant services from the `Microsoft.Identity.Web` library.

    Important note: even if we are building a console application, it is a daemon, and therefore a confidential client application, as it does not access Web APIs on behalf of a user but as an application.

    ```CSharp
    // Get the Token acquirer factory instance. By default it reads an appsettings.json
    // file if it exists in the project.
    TokenAcquirerFactory tokenAcquirerFactory = TokenAcquirerFactory.GetDefaultInstance();

    // Configure the authentication options, add the services you need (Graph, token cache)
    IServiceCollection services = tokenAcquirerFactory.Services;
    services.Configure<MicrosoftAuthenticationOptions>(
              option => tokenAcquirerFactory.Configuration.GetSection("AzureAd").Bind(option))
            .AddMicrosoftGraph()
            .AddInMemoryTokenCaches();

    // For more token cache serialization options, see https://aka.ms/msal-net-token-cache-serialization

    // Resolve the dependency injection.
    var serviceProvider = tokenAcquirerFactory.Build();
    ```

2. Get data for users to attribute their data to generated API's using the `Microsoft.Identity.Web.MicrosoftGraph` library. The `GraphServiceClient` will already be populated with the relevant configurations, including credentials, from you `appsettings.json` file from the previous step and allow you to make requests to the **Microsoft Graph API** as shown below.

    ```CSharp
    GraphServiceClient graphServiceClient = serviceProvider.GetRequiredService<GraphServiceClient>();

    string userSelectFields = "id,displayName,userPrincipalName";
    IGraphServiceUsersCollectionPage usersPage = await graphServiceClient
        .Users
        .Request()
        .WithAppOnly()
        .Select(userSelectFields)
        .GetAsync();

    var users = await CollectionProcessor<User>.ProcessGraphCollectionPageAsync(graphServiceClient, usersPage, 20);

    foreach (User user in users)
    {
        Console.WriteLine($"{user.Id}, {user.DisplayName}, {user.UserPrincipalName}");
    }
    ```

3. Acquire an access token and request **TO-DO's** from your API with the code shown below. The token requested will have the same scopes set in the `AzureAd:DownstreamApis` section of your `appsettings.json` file. You can

    ```CSharp
    var todoListApiOptions = tokenAcquirerFactory.Configuration.GetSection("AzureAd:DownstreamApis")
        .Get<MicrosoftGraphOptions>();

    var tokenAcquirer = tokenAcquirerFactory.GetTokenAcquirer();
    var acquireTokenResult = await tokenAcquirer.GetTokenForAppAsync(todoListApiOptions.Scopes);

    // We got a list of users from the tenant
    // Now that we also have an Access token for the ToDoList API,  we proceed to generate random ToDos 
    if (acquireTokenResult != null)
    {
        var httpClient = new HttpClient();
        var apiCaller = new ProtectedApiCallHelper(httpClient);

        IEnumerable<Models.Todo> todosToUpload = users
            .Select(user => new Models.Todo()
            {
                Owner = user.Id,
                Task = $"A To-Do for: {user.DisplayName}.",
            });

        await apiCaller.PostToDoForUserAsync($"{todoListApiOptions.BaseUrl}/api/todolist", acquireTokenResult.AccessToken, todosToUpload);

        // Fetch the list of ToDos and print them
        await apiCaller.GetAllTodosFromApiAndProcessResultAsync($"{todoListApiOptions.BaseUrl}/api/todolist", acquireTokenResult.AccessToken);
    }
    ```

### The code to protect the Web API

The relevant code for the Web API is in the `Startup.cs` class. We are using the method `AddMicrosoftWebApi` to configure the Web API to authenticate using bearer tokens, validate them and protect the API from non authorized calls. These are the steps:

1. Configuring the API to authenticate using bearer tokens

    ```CSharp
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(Configuration);
    ```

1. Protecting the Web API

    Only apps that have added the **application role** created on **Azure Portal** for the `TodoList-webapi-daemon-v2`, will contain the claim `roles` on their tokens. This is also taken care by [Microsoft Identity Web](https://github.com/AzureAD/microsoft-identity-web)

    The protection can also be done on the `Controller` level, using the [RequiredScopeOrAppPermission](https://github.com/AzureAD/microsoft-identity-web/wiki/web-apis#checking-for-scopes-or-app-permissions=) attribute and `Policy`. Read more about [policy based authorization](https://docs.microsoft.com/aspnet/core/security/authorization/policies?view=aspnetcore-6.0):

    ```csharp
    [HttpGet]
    [RequiredScopeOrAppPermission(
        AcceptedScope = new string[] { _todoListReadScope, _todoListReadWriteScope },
        AcceptedAppPermission = new string[] { _todoListReadAllPermission, _todoListReadWriteAllPermission }
        )]
    public IEnumerable<Todo> Get()
    {
        ...
    }
    ```

1. Determining if it's an application or user calling your API

    If you followed the instructions to setup the [idtyp](https://docs.microsoft.com/azure/active-directory/develop/active-directory-optional-claims) optional claim for this application all tokens issued for applications will contain a claim called `idtyp` which will have a value of `app`. This makes it possible to determnine if an incoming request is coming from a user or an application. The method *IsAppOnlyToken()* shows how to check if this value is set.

    ```csharp
    private bool IsAppOnlyToken()
    {
        // Add in the optional 'idtyp' claim to check if the access token is coming from an application or user.
        //
        // See: https://docs.microsoft.com/azure/active-directory/develop/active-directory-optional-claims
        return HttpContext.User.Claims.Any(c => c.Type == "idtyp" && c.Value == "app");
    }
    ```

    This can be used too add behaviors to end-points based on whether the request is coming from a client that is logged in as a user or as an application as shown below.

    ```csharp
    [HttpGet()]
    [RequiredScopeOrAppPermission(
        AcceptedScope = new string[] { _todoListReadScope, _todoListReadWriteScope },
        AcceptedAppPermission = new string[] { _todoListReadAllPermission, _todoListReadWriteAllPermission }
        )]
    public IEnumerable<Todo> Get()
    {
        if (!IsAppOnlyToken())
        {
            // this is a request for all ToDo list items of a certain user.
            return TodoStore.Values.Where(x => x.Owner == _currentLoggedUser);
        }
        else
        {
            // Its an app calling with app permissions, so return all items across all users
            return TodoStore.Values;
        }
    }
    ```

    This API is compatible with[active-directory-aspnetcore-webapp-openidconnect-v2 sample 4-1](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/4-WebApp-your-API/4-1-MyOrg) if you would like a quick sample to experiment with user based flows in this API.

## How to deploy this sample to Azure

### Deploying web API to Azure App Services

There is one web API in this sample. To deploy it to **Azure App Services**, you'll need to:

- create an **Azure App Service**
- publish the projects to the **App Services**

> :warning: Please make sure that you have not switched on the *[Automatic authentication provided by App Service](https://docs.microsoft.com/en-us/azure/app-service/scenario-secure-app-authentication-app-service)*. It interferes the authentication code used in this code example.


#### Publish your files (TodoList-webapi-daemon-v2)

##### Publish using Visual Studio

Follow the link to [Publish with Visual Studio](https://docs.microsoft.com/visualstudio/deployment/quickstart-deploy-to-azure).

##### Publish using Visual Studio Code

1. Install the Visual Studio Code extension [Azure App Service](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azureappservice).
1. Follow the link to [Publish with Visual Studio Code](https://docs.microsoft.com/aspnet/core/tutorials/publish-to-azure-webapp-using-vscode)

#### Enable cross-origin resource sharing (CORS) (TodoList-webapi-daemon-v2)

> :warning: the following steps are required only if you want your web API to be consumed by a single-page application (SPA). Learn more on [cross-origin resource sharing](https://developer.mozilla.org/docs/Web/HTTP/CORS).

1. Go to [Azure portal](https://portal.azure.com), and locate your project there.
    - On the API tab, select **CORS**. Check the box **Enable Access-Control-Allow-Credentials**.
    - Under **Allowed origins**, add the site URL of your published website **that will call this web API**.

## Next Steps

Learn how to:

* [Change your app to sign-in users from any organization or Microsoft accounts](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/1-WebApp-OIDC/1-3-AnyOrgOrPersonal)
* [Enable users from National clouds to sign-in to your application](https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/1-WebApp-OIDC/1-4-Sovereign)
* [Enable your web app to call a web API on behalf of the signed-in user](https://github.com/Azure-Samples/ms-identity-dotnetcore-ca-auth-context-app)

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Learn More

* [Microsoft identity platform (Azure Active Directory for developers)](https://docs.microsoft.com/azure/active-directory/develop/)
* [Overview of Microsoft Authentication Library (MSAL)](https://docs.microsoft.com/azure/active-directory/develop/msal-overview)
* [Register an application with the Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
* [Configure a client application to access web APIs](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-access-web-apis)
* [Understanding Azure AD application consent experiences](https://docs.microsoft.com/azure/active-directory/develop/application-consent-experience)
* [Understand user and admin consent](https://docs.microsoft.com/azure/active-directory/develop/howto-convert-app-to-be-multi-tenant#understand-user-and-admin-consent)
* [Application and service principal objects in Azure Active Directory](https://docs.microsoft.com/azure/active-directory/develop/app-objects-and-service-principals)
* [Authentication Scenarios for Azure AD](https://docs.microsoft.com/azure/active-directory/develop/authentication-flows-app-scenarios)
* [Building Zero Trust ready apps](https://aka.ms/ztdevsession)
* [National Clouds](https://docs.microsoft.com/azure/active-directory/develop/authentication-national-cloud#app-registration-endpoints)
* [Azure AD code samples](https://docs.microsoft.com/azure/active-directory/develop/sample-v2-code)
* [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web)
* [Validating Access Tokens](https://docs.microsoft.com/azure/active-directory/develop/access-tokens#validating-tokens)
* [User and application tokens](https://docs.microsoft.com/azure/active-directory/develop/access-tokens#user-and-application-tokens)
* [Validation differences by supported account types](https://docs.microsoft.com/azure/active-directory/develop/supported-accounts-validation)
* [How to manually validate a JWT access token using the Microsoft identity platform](https://github.com/Azure-Samples/active-directory-dotnet-webapi-manual-jwt-validation/blob/master/README.md)