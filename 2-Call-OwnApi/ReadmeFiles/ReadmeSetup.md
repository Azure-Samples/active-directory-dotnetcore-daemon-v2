### (Optional) use the automation script

1. On Windows run PowerShell and navigate to the root of the cloned directory
2. In PowerShell run:

   ```PowerShell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process -Force
   ```

3. Run the script to create your Azure AD application and configure the code of the sample application accordingly.

   ```PowerShell
   .\AppCreationScripts-WtihCert\Configure.ps1
   ```

   > Other ways of running the scripts are described in [App Creation Scripts](./AppCreationScripts-WithCert/AppCreationScripts.md)

If you don't want to use this automation, follow the following steps:

#### (Optional) Create a self-signed certificate

  To complete this step, you will use the `New-SelfSignedCertificate` Powershell command. You can find more information about the New-SelfSignedCertificate command [here](https://docs.microsoft.com/powershell/module/pkiclient/new-selfsignedcertificate).
  
  1. Open PowerShell and run `New-SelfSignedCertificate` with the following parameters to create a self-signed certificate in the user certificate store on your computer:
  
      ```PowerShell
      $cert=New-SelfSignedCertificate -Subject "CN=daemon-console-v2" -CertStoreLocation "Cert:\CurrentUser\My"  -KeyExportPolicy Exportable -KeySpec Signature
      ```
  
  1. Export this certificate using the "Manage User Certificate" MMC snap-in accessible from the Windows Control Panel. You can also add other options to generate the certificate in a different
  store such as the Computer or service store (See [How to: View Certificates with the MMC Snap-in](https://docs.microsoft.com/dotnet/framework/wcf/feature-details/how-to-view-certificates-with-the-mmc-snap-in)).
  
  Alternatively you can use an existing certificate if you have one (just be sure to record its name for the next steps)
  
##### Add the certificate for the application in Azure AD
  
  In the application registration blade for your application, in the **Certificates & secrets** page, in the **Certificates** section:
  
  1. Click on **Upload certificate** and, in click the browse button on the right to select the certificate you just exported (or your existing certificate)
  1. Click **Add**
  
##### Configure the Visual Studio project
  
  To change the visual studio project to enable certificates you need to:
  
  1. Open the `appsettings.json` file
  2. Find the app key `Clientertificates` in the `AzureAd` section and insert the `CertificateDescription` properties of your certificate within an array. You can see some examples below and read more about how to configure certificate descriptions [here](https://github.com/AzureAD/microsoft-identity-web/wiki/Certificates#specifying-certificates).
  
##### Get certificate from certificate store
  
  You can retrieve a certificate from your local store by adding the configuration below to the `ClientCertificates` array in the `appsettings.json` file replacing **<CERTIFICATE_STORE_PATH>** with the store path to your certificate and **<CERTIFICATE_DISTINGUISHED_NAME>** with the distinguished name of your certificate. If you used the configuration scripts to generate the application this will be done for you using a sample self-signed certificate. You can read more about certificate stores [here](https://docs.microsoft.com/windows-hardware/drivers/install/certificate-stores).
  
  ```json
  {
    // ... 
    "AzureAd": {
      // ...
        "ClientCertificates":  [{
          "SourceType":  "StoreWithDistinguishedName",
          "CertificateStorePath":  "<CERTIFICATE_STORE_PATH>",
          "CertificateDistinguishedName":  "<CERTIFICATE_DISTINGUISHED_NAME>"
        }]
    }
  }
  ```

##### Get certificate from file path
  
  It's possible to get a certificate file, such as a **pfx** file, directly from a file path on your machine and load it into the application by using the configuration as shown below. Add the configuration below to the `ClientCertificates` array of the `appsettings.json` file. Replace `<PATH_TO_YOUR_CERTIFICATE_FILE>` with the path to your certificate file and `<CERTIFICATE_PASSWORD>` with that certificates password. If you created the application with the `Configure.ps1` script found in the `AppCreationScripts-withCert` a **pfx** file called **daemon-console-v2.pfx** will be generated with the certificate that is associated with  your app and can be used as a credential. If you like, you can use configure the `Certificate` property to reference this file and use it as a credential.
  
  ```json
  {
    // ... 
    "AzureAd": {
      // ... 
      "ClientCertificates": [{
        "SourceType":  "Path",
        "CertificateDiskPath":  "<PATH_TO_YOUR_CERTIFICATE_FILE>",
        "CertificatePassword":  "<CERTIFICATE_PASSWORD>"
      }]
    } 
  }
  ```
  
##### Get certificate from Key Vault
  
  It's also possible to get certificates from an [Azure Key Vault](https://docs.microsoft.com/azure/key-vault/general/overview). Add the configuration below to the `ClientCertificates` array of the `appsettings.json` file. Replace `<YOUR_KEY_VAULT_URL>` with the URL of the Key Vault holding your certificate and `<YOUR_KEY_VAULT_CERTIFICATE_NAME>` with the name of that certificate as shown in your Key Vault. If you created the application with the `Configure.ps1` script found in the `AppCreationScripts-withCert` a **pfx** file called **daemon-console-v2.pfx** will be generated that is associated with the certificate that can be used as a credential for your app. If you like, you can load that certificate into a Key Vault and then access that Key Vault to use as a credential for your application.

  ```json
  {
    // ... 
    "AzureAd": {
      // ... 
      "ClientCertificates":  [{
        "SourceType":  "KeyVault",
        "KeyVaultUrl":  "<YOUR_KEY_VAULT_URL>",
        "KeyVaultCertificateName":  "<YOUR_KEY_VAULT_CERTIFICATE_NAME>"
      }]
    }
  }
  ```
  
  1. If you had set `ClientSecret` previously, change its value to an empty string, `""`.

#### Build and run

Build and run your project. You have the same output, but this time, your application is authenticated with Azure AD with the certificate instead of the application secret.

#### About the alternate code

This application makes use of the [Microsoft Identity Web Library](https://docs.microsoft.com/azure/active-directory/develop/microsoft-identity-web) to load the certificate based on the configurations in the `daemon-console/appsettings.json` for the `ClientCertificates` property setting. The `DefaultCertificateLoader` class contains the logic needed to load a certificate into your application and can store it into a `CertificateDescription` object as a [X509Certificate2](https://docs.microsoft.com/dotnet/api/system.security.cryptography.x509certificates.x509certificate2?view=net-6.0) object.

The application uses a `DefaultCertificateLoader` instance to load a `X509Certificate2` into the `config.Certificate` object. After this is done the certificate becomes accessible as in the `config` object as shown below by calling `config.Certificate.Certificate`. Instead of using the `WithClientSecret` to add a client secret as a credential `WithCertificate` is used associate a certificate as the credential.

You can find this logic in the `ConfidentialClientApplicationService.cs` file.

```CSharp
ICertificateLoader certificateLoader = new DefaultCertificateLoader();
certificateLoader.LoadIfNeeded(_azureAdOptions.ClientCertificates.First());

_confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(_azureAdOptions.ClientId)
    .WithAuthority(new Uri(_azureAdOptions.Authority))
    .WithCertificate(_azureAdOptions.ClientCertificates.First().Certificate)
    .Build();
```

The rest of the application remains the same.