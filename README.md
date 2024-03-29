---
page_type: sample
languages:
- csharp
- powershell
products:
- microsoft-entra-id
description: "Daemon or unattended application consuming Microsoft Graph or your own Web Apis using Microsoft Identity Platform to acquire tokens."
urlFragment: ms-identity-daemon
---

# A .NET Core daemon console application using MSAL.NET to acquire tokens for resources

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/aad%20Samples/.NET%20client%20samples/active-directory-dotnetcore-daemon-v2%20CI)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=695)

## About this sample

> This sample uses Microsoft.Identity.Web, which is a higher level API on top of MSAL.NET. If you are interested in the raw MSAL.NET code, see [this archived branch](https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2/tree/withMsal)

### Scenario

In these scenarios we show how unattended daemon applications can authenticate as itself using the Microsoft Authentication Library for .NET ([MSAL.NET](https://aka.ms/msal-net)) SDK and acquire [Access Tokens](https://aka.ms/access-tokens) for various web APIs like [Microsoft Graph](https://aka.ms/msgraph) or any other API secured with the [Microsoft Identity Platform](https://aka.ms/identityplatform)

### Structure of the repository

This repository contains a chapter-wise tutorial made of three parts:

Sub folder                    | Description
----------------------------- | -----------
[1-Call-Graph](https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2/tree/master/1-Call-MSGraph) | This sample application shows how to use the Microsoft Authentication Library for .NET ([MSAL.NET](https://aka.ms/msal-net)) to access the data of Microsoft business customers in a long-running, non-interactive process. It uses the [OAuth 2 client credentials grant](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow)to acquire an access token, which can be used to call the [Microsoft Graph](https://aka.ms/msgraph) and access organizational data </p> ![Topology](./1-Call-MSGraph/ReadmeFiles/topology.png)
[2-Call-OwnApi](https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2/tree/master/2-Call-OwnApi)  | This sample application shows how to use the Microsoft Authentication Library for .NET ([MSAL.NET](https://aka.ms/msal-net)) to access the data from a protected Web API, in a non-interactive process. It uses the [OAuth 2 client credentials grant](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow) to acquire an access token, which is then used to call the Web API. Additionally, it lays down all the steps developers need to take to secure their Web APIs with the Microsoft identity platform. </p>  ![Topology](./2-Call-OwnApi/ReadmeFiles/topology.png)
[3-Using-KeyVault](https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2/tree/master/3-Using-KeyVault)  | This chapter explains how to integrate [Azure Key Vault](https://docs.microsoft.com/azure/key-vault/general/basic-concepts) and [Managed Identities for Azure Resources](https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/overview) with a daemon application. Additionally, it has a code snippet on how to get an access token for Key Vault.

## How to run this sample

To run this sample, you'll need:

- [Visual Studio](https://aka.ms/vsdownload) and the [.NET Core SDK](https://www.microsoft.com/net/learn/get-started)
- An Internet connection
- A Windows machine (necessary if you want to run the app on Windows)
- An OS X machine (necessary if you want to run the app on Mac)
- A Linux machine (necessary if you want to run the app on Linux)
- a Microsoft Entra tenant. For more information on how to get a Microsoft Entra tenant, see [How to get a Microsoft Entra tenant](https://azure.microsoft.com/documentation/articles/active-directory-howto-tenant/)
- A user account in your Microsoft Entra tenant. This sample will not work with a Microsoft account (formerly Windows Live account). Therefore, if you signed in to the [Microsoft Entra admin center](https://entra.microsoft.com) with a Microsoft account and have never created a user account in your directory before, you need to do that now.

### Step 1:  Clone or download this repository

From your shell or command line:

```Shell
git clone https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2.git
```

or download and exact the repository .zip file.

> Given that the name of the sample is pretty long, and so are the name of the referenced NuGet packages, you might want to clone it in a folder close to the root of your hard drive, to avoid file size limitations on Windows.

## Community Help and Support

Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) to get support from the community.
Ask your questions on Stack Overflow first and browse existing issues to see if someone has asked your question before.
Make sure that your questions or comments are tagged with [`msal` `dotnet`].

If you find a bug in the sample, please raise the issue on [GitHub Issues](https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2/issues).

If you find a bug in msal.Net, please raise the issue on [MSAL.NET GitHub Issues](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues).

To provide a recommendation, visit the following [User Voice page](https://feedback.azure.com/forums/169401-azure-active-directory).

> [Consider taking a moment to share your experience with us.](https://forms.office.com/Pages/ResponsePage.aspx?id=v4j5cvGGr0GRqy180BHbRy8G199fkJNDjJ9kJaxUJIhUNUJGSDU1UkxFMlRSWUxGVTlFVkpGT0tOTi4u)

## Contributing

If you'd like to contribute to this sample, see [CONTRIBUTING.MD](https://github.com/Azure-Samples/active-directory-dotnetcore-daemon-v2/blob/master/CONTRIBUTING.md).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information, see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## More information

For more information, see MSAL.NET's conceptual documentation:

- [Quickstart: Register an application with the Microsoft identity platform](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
- [Quickstart: Configure a client application to access web APIs](https://docs.microsoft.com/azure/active-directory/develop/quickstart-configure-app-access-web-apis)
- [Acquiring a token for an application with client credential flows](https://aka.ms/msal-net-client-credentials)

For more information about the underlying protocol:

- [Microsoft identity platform and the OAuth 2.0 client credentials flow](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow)

For a more complex multi-tenant Web app daemon application, see [active-directory-dotnet-daemon-v2](https://github.com/Azure-Samples/active-directory-dotnet-daemon-v2)
