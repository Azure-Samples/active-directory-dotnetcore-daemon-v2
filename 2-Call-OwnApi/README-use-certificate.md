### (Optional) Create a self-signed certificate

To complete this step, you will use the `New-SelfSignedCertificate` Powershell command. You can find more information about the New-SelfSignedCertificate command [here](https://docs.microsoft.com/powershell/module/pkiclient/new-selfsignedcertificate).

1. Open PowerShell and run `New-SelfSignedCertificate` with the following parameters to create a self-signed certificate in the user certificate store on your computer:

    ```PowerShell
    $cert=New-SelfSignedCertificate -Subject "CN=DaemonConsoleCert" -CertStoreLocation "Cert:\CurrentUser\My"  -KeyExportPolicy Exportable -KeySpec Signature
    ```

1. Export this certificate using the "Manage User Certificate" MMC snap-in accessible from the Windows Control Panel. You can also add other options to generate the certificate in a different
store such as the Computer or service store (See [How to: View Certificates with the MMC Snap-in](https://docs.microsoft.com/dotnet/framework/wcf/feature-details/how-to-view-certificates-with-the-mmc-snap-in)).

Alternatively you can use an existing certificate if you have one (just be sure to record its name for the next steps)

### Add the certificate for the daemon-console-v2 application in Azure AD

In the application registration blade for your application, in the **Certificates & secrets** page, in the **Certificates** section:

1. Click on **Upload certificate** and, in click the browse button on the right to select the certificate you just exported (or your existing certificate)
1. Click **Add**

### Configure the Visual Studio project

To change the visual studio project to enable certificates you need to:

1. Open the `daemon-console\appsettings.json` file
2. Find the app key `Certificate` and insert the `CertificateDescription` properties of your certificate. You can see some examples below and read more about how to configure certificate descriptions [here](https://github.com/AzureAD/microsoft-identity-web/wiki/Certificates#specifying-certificates).

### Get certificate from certificate store

You can retrieve a certificate from your local store by adding the configuration below to the `Certificate` property in the `appsettings.json` file in the `daemon-console` directory replacing **<CERTIFICATE_STORE_PATH>** with the store path to your certificate and **<CERTIFICATE_STORE_PATH>** with the distinguished name of your certificate. If you used the configuration scripts to generate the application this will be done for you using a sample self-signed certificate. You can read more about certificate stores [here](https://docs.microsoft.com/windows-hardware/drivers/install/certificate-stores).

```json
{
  // ... 
  "Certificate":  {
    "SourceType":  "StoreWithDistinguishedName",
    "CertificateStorePath":  "<CERTIFICATE_STORE_PATH>",
    "CertificateDistinguishedName":  "<CERTIFICATE_DISTINGUISHED_NAME>"
  }
}
```

#### Get certificate from file path

It's possible to get a certificate file, such as a **pfx** file, directly from a file path on your machine and load it into the application by using the configuration as shown below. Replace the values in the `Certificate` key of the `appsettings.json` file in the `daemon-console` directory with the snippet shown below also replacing `<PATH_TO_YOUR_CERTIFICATE_FILE>` with the path to your certificate file and `<PATH_TO_YOUR_CERTIFICATE_FILE>` with that certificates password. If you created the application with the `Configure.ps1` script found in the `AppCreationScripts-withCert` a **pfx** file called **DaemonConsoleCert.pfx** will be generated with the certificate that is associated with  your app and can be used as a credential. If you like, you can use configure the `Certificate` property to reference this file and use it as a credential.

```json
{
  // ... 
  "Certificate":  {
    "SourceType":  "Path",
    "CertificateDiskPath":  "<PATH_TO_YOUR_CERTIFICATE_FILE>",
    "CertificatePassword":  "<CERTIFICATE_PASSWORD>"
  }
}
```

#### Get certificate from Key Vault

It's also possible to get certificates from an [Azure Key Vault](https://docs.microsoft.com/azure/key-vault/general/overview). Replace the values in the `Certificate` key of the `appsettings.json` file in the `daemon-console` directory with the snippet shown below also replacing `<YOUR_KEY_VAULT_URL>` with the URL of the Key Vault holding your certificate and `<YOUR_KEY_VAULT_CERTIFICATE_NAME>` with the name of that certificate as shown in your Key Vault. If you created the application with the `Configure.ps1` script found in the `AppCreationScripts-withCert` a **pfx** file called **DaemonConsoleCert.pfx** will be generated that is associated with the certificate that can be used as a credential for your app. If you like, you can load that certificate into a Key Vault and then access that Key Vault to use as a credential for your application. See the [chapter 3 readme](../3-Using-KeyVault/README.md) for more information.

```json
{
  // ... 
  "Certificate":  {
    "SourceType":  "KeyVault",
    "KeyVaultUrl":  "<YOUR_KEY_VAULT_URL>",
    "KeyVaultCertificateName":  "<YOUR_KEY_VAULT_CERTIFICATE_NAME>"
  }
}
```

3. If you had set `ClientSecret` previously, change its value to empty string, `""`.

#### Build and run

Build and run your project. You have the same output, but this time, your application is authenticated with Azure AD with the certificate instead of the application secret.

#### About the alternate code

This application makes use of the [Microsoft Identity Web Library](https://docs.microsoft.com/azure/active-directory/develop/microsoft-identity-web) to load the certificate based on the configurations in the `daemon-console/appsettings.json` for the `Certificate` property settings. The `DefaultCertificateLoader` class contains the logic needed to load a certificate into your application and can store it into a `CertificateDescription` object as a [X509Certificate2](https://docs.microsoft.com/dotnet/api/system.security.cryptography.x509certificates.x509certificate2?view=net-6.0) object.

The application uses a `DefaultCertificateLoader` instance to load a `X509Certificate2` into the `config.Certificate` object. After this is done the certificate becomes accessible as in the `config` object as shown below by calling `config.Certificate.Certificate`. Instead of using the `WithClientSecret` to add a client secret as a credential `WithCertificate` is used associate a certificate as the credential.

```CSharp
ICertificateLoader certificateLoader = new DefaultCertificateLoader();
certificateLoader.LoadIfNeeded(config.Certificate);

app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                .WithCertificate(config.Certificate.Certificate)
                .WithAuthority(new Uri(config.Authority))
                .Build();
```

The rest of the application remains the same.