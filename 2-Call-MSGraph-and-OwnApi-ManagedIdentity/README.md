---
topic: sample
languages:
  - csharp
products:
  - microsoft-entra-id
  - dotnet
  - office-ms-graph
description: "Shows how a .NET daemon uses managed identity to call Microsoft Graph and a custom protected web API."
---

# Call Microsoft Graph and a custom API with managed identity

This chapter contains one daemon application with two executable call paths:

- `CallMicrosoftGraphAsync` calls Microsoft Graph `/users`.
- `CallCustomApiAsync` calls the included `TodoList-WebApi`.

The API targets have separate `MicrosoftGraph` and `TodoListApi` configuration. Either target can use the resource's system-assigned managed identity or a separately configured user-assigned managed identity.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A Microsoft Entra tenant
- An Azure VM, App Service, Container App, or other supported host with a managed identity
- A tenant administrator who can assign application roles to the managed identity's service principal

Managed identity token acquisition requires an Azure host. The solution can be built elsewhere, but the daemon calls cannot authenticate locally through managed identity unless the selected environment provides one.

## Optional: enable Microsoft Graph

Only complete these steps if you will run the `graph` or `all` target:

1. Grant the managed identity's service principal the Microsoft Graph application role `User.Read.All`. Follow the application-role assignment process described in [Grant an app role to a managed identity](https://learn.microsoft.com/azure/app-service/scenario-secure-app-access-microsoft-graph-as-app?tabs=azure-powershell#grant-access-to-microsoft-graph).
1. Keep `MicrosoftGraph:Scopes` set to `https://graph.microsoft.com/.default`. For a national cloud, use that cloud's Microsoft Graph endpoint.
1. For a user-assigned identity, set `MicrosoftGraph:UserAssignedClientId` to its client ID. Leave it empty to use the system-assigned identity.

## Optional: enable the custom API

Only complete these steps if you will run the `custom-api` or `all` target:

1. Register `TodoList-webapi-daemon-v2`.
1. Set its Application ID URI to `api://<API_CLIENT_ID>`.
1. Expose an application role named `DaemonAppRole` with `allowedMemberTypes` set to `Application`.
1. Create a service principal for the API registration if one does not already exist.
1. Assign `DaemonAppRole` to the managed identity's service principal for the API service principal.
1. Configure `TodoList-WebApi/appsettings.json` with the tenant and API client ID.
1. Configure `TodoListApi:Scopes` as `api://<API_CLIENT_ID>/.default`.
1. For a user-assigned identity, set `TodoListApi:AcquireTokenOptions:ManagedIdentity:UserAssignedClientId`. Leave it empty to use the system-assigned identity.

App-creation scripts are intentionally not included in this chapter: creating an app registration is only part of managed identity setup, and role assignment must target the service principal of the identity attached to the Azure host.

## Configure

Edit `daemon-console/appsettings.json`:

```json
{
  "MicrosoftGraph": {
    "Scopes": [ "https://graph.microsoft.com/.default" ],
    "UserAssignedClientId": ""
  },
  "TodoListApi": {
    "BaseUrl": "https://localhost:44372/",
    "RelativePath": "api/TodoList",
    "RequestAppToken": true,
    "Scopes": [ "api://<API_CLIENT_ID>/.default" ],
    "AcquireTokenOptions": {
      "ManagedIdentity": {
        "UserAssignedClientId": ""
      }
    }
  }
}
```

## Run

For Microsoft Graph on the managed-identity-enabled host:

```console
cd daemon-console
dotnet run -- graph
```

For the custom API, start `TodoList-WebApi` at the configured URL and then run:

```console
cd daemon-console
dotnet run -- custom-api
```

Run both configured targets sequentially with:

```console
dotnet run -- all
```

`all` is the default when no target is supplied.

## How it works

The Graph request sets `AcquireTokenOptions.ManagedIdentity` before calling the Graph SDK. The named `TodoListApi` registration carries its own managed identity token-acquisition settings. This keeps the two resources independently configurable while sharing one daemon project.
