---
topic: sample
languages:
  - csharp
products:
  - microsoft-entra-id
  - dotnet
  - office-ms-graph
description: "Shows how a daemon console app uses a managed identity to get an access token and call Microsoft Graph using the Microsoft.Identity.Web library."
---

# A .NET Core daemon console application calling a protected Web API with its own identity

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/AAD%20Samples/.NET%20client%20samples/active-directory-dotnetcore-daemon-v2%20CI)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=695)

## About this sample

### Overview

This sample application shows how to use the [Microsoft identity platform](https://aka.ms/identityplatform) to access the data of Microsoft business customers in [Microsoft Graph](https://aka.ms/msgraph) in a long-running, non-interactive process.  It uses the [OAuth 2 client credentials grant](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow) to acquire an [Access Token(s)](https://aka.ms/access-tokens) using a managed identity, which is then used to call the [Microsoft Graph](https://graph.microsoft.io) API and access organizational data.

The app is a .NET Core Console application. It gets the list of users in a Microsoft Entra tenant by using the Microsoft Authentication Library for .NET ([MSAL.NET](https://aka.ms/msal-net)) to authenticate and acquire a token.

## Scenario

The console application:

1. Gets a token from Microsoft Entra ID for itself (without a user).
1. It then calls the Microsoft Graph `/users` endpoint to get the list of users, which it displays on the screen.

- Developers who wish to gain good familiarity of programming with Microsoft Graph are advised to go through the [An introduction to Microsoft Graph for developers](https://www.youtube.com/watch?v=EBbnpFdB92A) recorded session.


## How to run this sample

To run this sample, you'll need:

- [Visual Studio](https://aka.ms/vsdownload) and the [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
- An Internet connection
- An Entra ID tenant. For more information on how to create an Entra ID tenant, see [Get a Microsoft Entra ID tenant](https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-create-new-tenant).
- An Azure virtual machine (VM) with a configured managed identity. For information on how to set up a VM with a managed identity, follow the instructions in the [managed identity wiki](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/overview).

### Step 1:  Clone or download this repository into your VM

From the shell or command line in the VM type:

```Shell
git clone https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2.git
```

or download and extract the repository .zip file.

> Given that the name of the sample is pretty long, and so are the name of the referenced NuGet packages, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

Navigate to the `"5-Call-MSGraph-ManagedIdentity"` folder

```Shell
cd "5-Call-MSGraph-ManagedIdentity"
```

### Step 2:  Register the sample with your Entra ID tenant

#### Choose the Entra ID tenant where your VM is located

As a first step you'll need to:

1. Sign in to the [Microsoft Entra admin center](https://entra.microsoft.com) using either a work or school account or a personal Microsoft account.
1. If your account is present in more than one Microsoft Entra tenant, select `Directory + Subscription` at the top right corner in the menu on top of the page, and switch your portal session to the desired Microsoft Entra tenant.
1. In the left-hand navigation pane, select the **Microsoft Entra ID** service, and then select **App registrations**.

#### Register the client app (daemon-console)

1. In order to access Microsoft Graph you'll need to grant the correct permissions to your managed identity. To do this follow the instructions in the "Grant access to Microsoft Graph" section of [this wiki](https://learn.microsoft.com/en-us/azure/app-service/scenario-secure-app-access-microsoft-graph-as-app?tabs=azure-powershell#grant-access-to-microsoft-graph).

1. At this stage permissions are assigned correctly but the client app does not allow interaction.
   Therefore no consent can be presented via a UI and accepted to use the service app.
   Click the **Grant/revoke admin consent for {tenant}** button, and then select **Yes** when you are asked if you want to grant consent for the
   requested permissions for all account in the tenant.
   You need to be an Entra ID tenant admin to do this.

### Step 3:  Configure the sample to use your Entra ID tenant

In the steps below, "ClientID" is the same as "Application ID" or "AppId".

Open the solution in Visual Studio to configure the projects

#### Configure the client project

1. Open the `Daemon-Console\Program.cs` file
1. If you are connecting to a national cloud, change the scope to the correct Microsoft Graph endpoint. [See this reference for a list of Microsoft Graph endpoints.](https://docs.microsoft.com/graph/deployments#microsoft-graph-and-graph-explorer-service-root-endpoints)
1. If using a user-assigned managed identity, find the `UserAssignedClientId` member, uncomment the line, and replace the existing value with the Client ID of your user-assigned managed identity.

### Step 4: Run the sample

In the VM's console, start the client app

```Console
cd daemon-console
dotnet run
```

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRy8G199fkJNDjJ9kJaxUJIhUNUJGSDU1UkxFMlRSWUxGVTlFVkpGT0tOTi4u)

## About the code

1. To learn more about the code specific to using managed identity please see the wiki [here](https://github.com/AzureAD/microsoft-identity-web/wiki/Calling-APIs-with-Managed-Identity). The wiki includes an example of calling graph as well as an additional example of how to call your own API.

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
