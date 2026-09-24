---
page_type: sample
languages:
- csharp
- powershell
products:
- microsoft-entra-id
description: "Daemon applications that call Microsoft Graph or a custom protected web API using client credentials or managed identity."
urlFragment: ms-identity-daemon
---

# .NET daemon applications calling protected APIs

[![Build status](https://identitydivision.visualstudio.com/IDDP/_apis/build/status/aad%20Samples/.NET%20client%20samples/active-directory-dotnetcore-daemon-v2%20CI)](https://identitydivision.visualstudio.com/IDDP/_build/latest?definitionId=695)

This repository demonstrates unattended .NET applications that authenticate as themselves and call either Microsoft Graph or a custom API protected by the Microsoft identity platform. The runnable chapters share the same two API targets so that the authentication method is the only major difference.

## Chapters

| Chapter | Authentication | Runnable targets |
| --- | --- | --- |
| [1-Call-MSGraph-and-OwnApi](./1-Call-MSGraph-and-OwnApi/README.md) | Client secret, certificate, workload identity, or another Microsoft.Identity.Web client credential | Microsoft Graph `/users` and the included `TodoList-WebApi` |
| [2-Call-MSGraph-and-OwnApi-ManagedIdentity](./2-Call-MSGraph-and-OwnApi-ManagedIdentity/README.md) | System-assigned or user-assigned managed identity | Microsoft Graph `/users` and the included `TodoList-WebApi` |
| [3-Using-KeyVault](./3-Using-KeyVault/README.md) | Certificate loaded from Azure Key Vault for chapter 1 | Credential-storage variation for either API target |

The Key Vault chapter is retained because it demonstrates retrieving a client certificate from Key Vault. It is distinct from using a managed identity directly to request the downstream API token.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- A Microsoft Entra tenant
- An internet connection
- An Azure-hosted resource with a managed identity for chapter 2

Clone the repository and follow the README in the chapter that matches the credential type you want to use.

## Support

Use [GitHub Issues](../../issues) for sample problems and [MSAL.NET GitHub Issues](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues) for library problems. See [CONTRIBUTING.md](./CONTRIBUTING.md) before contributing.
