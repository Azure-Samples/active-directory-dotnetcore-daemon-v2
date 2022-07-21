### Overview

This sample application shows how to use the [Microsoft identity platform](http://aka.ms/aadv2) to access the data from a protected Web API, in a non-interactive process.  It uses the [OAuth 2 client credentials grant](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow) to acquire an [Access Tokens](https://aka.ms/access-tokens), which is then used to call the protected Web API. Additionally, it also lays out all the steps developers need to take to secure their Web APIs with the [Microsoft identity platform](http://aka.ms/aadv2).

The app is a .NET Core console application that gets the list of "ToDos" from `TodoList-WebApi` project by using Microsoft Authentication Library for .NET ([MSAL.NET](https://aka.ms/msal-net)) to acquire an access token for `TodoList-WebApi`.

> ### Daemon applications can use two forms of credentials to authenticate themselves with Azure AD:
>
> - **Client secrets** (also called application password).
> - **Certificates**.
>
> The first type (Client secret) is covered first the next paragraphs.
> A variation of this sample that uses a **certificate**, is also discussed at the end of this article in [Variation: daemon application using client credentials with certificates](#Variation-daemon-application-using-client-credentials-with-certificates)

The console application:

- acquires an access token from Azure AD by authenticating as an application (no user interaction)
- and then calls the Web API  `TodoList-WebApi` protected using [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web) to get the a list of ToDo's, and displays the result

![Topology](./ReadmeFiles/daemon-with-secret.svg)


## Variation: daemon application using client credentials with certificates

Daemon applications can use two forms of secrets to authenticate themselves with Azure AD:

- **application secrets** (also named application password). This is what we've seen so far.
- **certificates**. This is the object of this paragraph.

![Topology](./ReadmeFiles/daemon-with-certificate.svg)

To [use client credentials protocol flow with certificates](https://docs.microsoft.com/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow#second-case-access-token-request-with-a-certificate) instead of an application secret, you will need to do little changes to what you have done so far:

- (optionally) generate a certificate and export it, if you don't have one already
- register the certificate with your application in the application registration portal
- enable the sample code to use certificates instead of app secret.

For more information on the concepts used in this sample, be sure to read the [Scenario: Daemon application that calls web APIs](https://docs.microsoft.com/azure/active-directory/develop/scenario-daemon-overview).

