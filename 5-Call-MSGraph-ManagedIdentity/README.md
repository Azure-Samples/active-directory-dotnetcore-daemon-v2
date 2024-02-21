---
topic: sample
languages:
  - csharp
products:
  - azure-active-directory
  - dotnet-core
  - office-ms-graph
description: "Shows how a daemon console app uses a managed identity to get an access token and call a protected Web API with the Microsoft.Identity.Web library."
---

# A .NET Core daemon console application calling a protected Web API with its own identity

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/active-directory-dotnetcore-daemon-v2%20CI)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=695)

## About this sample

### Overview

This sample application shows how to use the [Microsoft identity platform](http://aka.ms/aadv2) to access the data from Microsoft Graph, in a non-interactive process. It uses the [OAuth 2 client credentials grant](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow) to acquire an [Access Token(s)](https://aka.ms/access-tokens), which is then used to call the protected Web API. Additionally, it also lays out all the steps developers need to take to secure their Web APIs with the [Microsoft identity platform](http://aka.ms/aadv2).

The app is a .NET Core console application that gets data from Microsoft Graph by using Microsoft Authentication Library for .NET ([MSAL.NET](https://aka.ms/msal-net)) to acquire an access token for using a managed identity.

## How to run this sample

To run this sample, you'll need:

- [Visual Studio](https://aka.ms/vsdownload) and the [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
- An Internet connection
- An Azure Active Directory (Azure AD) tenant. For more information on how to get an Azure AD tenant, see [How to get an Azure AD tenant](https://azure.microsoft.com/documentation/articles/active-directory-howto-tenant/)
- An Azure virtual machine (VM) with a configured managed identity. For information on how to set up a VM with a managed identity, follow the instructions in the [managed identity wiki](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/overview).

### Step 1:  Clone or download this repository into your VM

From the shell or command line in the VM type:

```Shell
git clone https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2.git
```

or download and extract the repository .zip file.

> Given that the name of the sample is pretty long, and so are the name of the referenced NuGet packages, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

Navigate to the `"5-Call-Graph-ManagedIdentity"` folder

```Shell
cd "5-Call-Graph-ManagedIdentity"
```

### Step 2:  Register the sample with your Azure Active Directory tenant

#### Choose the Azure AD tenant where you want to create your applications

As a first step you'll need to:

1. Sign in to the [Azure portal](https://portal.azure.com) using either a work or school account or a personal Microsoft account.
1. If your account is present in more than one Azure AD tenant, select `Directory + Subscription` at the top right corner in the menu on top of the page, and switch your portal session to the desired Azure AD tenant.
1. In the left-hand navigation pane, select the **Azure Active Directory** service, and then select **App registrations**.

#### Register the client app (daemon-console)

1. Navigate to the Microsoft identity platform for developers [App registrations](https://go.microsoft.com/fwlink/?linkid=2083908) page.
1. Select **New registration**.
   - In the **Name** section, enter a meaningful application name that will be displayed to users of the app, for example `daemon-console-v2`.
   - In the **Supported account types** section, select **Accounts in this organizational directory only ({tenant name})**.
   - Select **Register** to create the application.

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

#### Configure the client project

1. Open the `Daemon-Console\appsettings.json` file
1. If you are connecting to a national cloud, change the instance to the correct Microsoft Graph endpoint. [See this reference for a list of Azure AD endpoints.](https://docs.microsoft.com/graph/deployments#microsoft-graph-and-graph-explorer-service-root-endpoints)
1. If using a user-assigned managed identity, find the app key `UserAssignedClientId`, uncomment the line, and replace the existing value with the Client ID of your user-assigned managed identity.
1. Find the app key `Scopes` and replace the existing value with the **App ID URI** of your web API, followed by "/.default".  

### Step 4: Run the sample

In the VM's console, start the client app

```Console
cd daemon-console
dotnet run
```

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRy8G199fkJNDjJ9kJaxUJIhUNUJGSDU1UkxFMlRSWUxGVTlFVkpGT0tOTi4u)

## About the code

To learn more about the code specific to using managed identity please see the wiki [here](https://github.com/AzureAD/microsoft-identity-web/wiki/Calling-APIs-with-Managed-Identity).

### The code to protect the Web API

The relevant code for the Web API is in the `Startup.cs` class. We are using the method `AddMicrosoftWebApi` to configure the Web API to authenticate using bearer tokens, validate them and protect the API from non authorized calls. These are the steps:

1. Configuring the API to authenticate using bearer tokens

    ```CSharp
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(Configuration);
    ```

2. Protecting the Web API

    Only apps that have added the **application role** created on **Azure Portal** for the `TodoList-webapi-daemon-v2`, will contain the claim `roles` on their tokens. This is also taken care by [Microsoft Identity Web](https://github.com/AzureAD/microsoft-identity-web)

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
to the application. See step 4 of [Register the client app (daemon-console-v2)](#register-the-client-app-daemon-console) above.

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
