# Use a client certificate stored in Azure Key Vault

This chapter is a credential-storage variation for [chapter 1](../1-Call-MSGraph-and-OwnApi/README.md). It applies to either the Microsoft Graph or custom API call path because both use the same `AzureAd` confidential-client credential.

It is retained alongside the managed identity chapter because the scenarios are different:

- Here, a managed identity may authenticate to Key Vault so the daemon can retrieve a certificate. The daemon then authenticates to Microsoft Entra ID as its **app registration** using that certificate.
- In [chapter 2](../2-Call-MSGraph-and-OwnApi-ManagedIdentity/README.md), the managed identity itself requests the downstream API token and no app-registration credential is loaded.

## Prerequisites

- A working chapter 1 registration for at least one API target
- An Azure Key Vault containing the daemon application's certificate
- An Azure host identity or developer identity with permission to read the certificate and secret from the vault
- The public certificate uploaded to the daemon app registration

## Configure the client credential

In `1-Call-MSGraph-and-OwnApi/daemon-console/appsettings.json`, replace `AzureAd:ClientCredentials` with:

```json
"ClientCredentials": [
  {
    "SourceType": "KeyVault",
    "KeyVaultUrl": "https://<KEY_VAULT_NAME>.vault.azure.net/",
    "KeyVaultCertificateName": "<CERTIFICATE_NAME>"
  }
]
```

Grant the runtime identity permission to read the certificate and its backing secret. Prefer Azure role-based access control; if the vault uses access policies, grant the required certificate and secret `Get` permissions.

Run either chapter 1 target normally:

```console
dotnet run -- graph
dotnet run -- custom-api
dotnet run -- all
```

For more information, see [Microsoft.Identity.Web client credentials](https://aka.ms/ms-id-web/client-credentials), [Azure Key Vault certificates](https://learn.microsoft.com/azure/key-vault/certificates/about-certificates), and [managed identities](https://learn.microsoft.com/entra/identity/managed-identities-azure-resources/overview).
