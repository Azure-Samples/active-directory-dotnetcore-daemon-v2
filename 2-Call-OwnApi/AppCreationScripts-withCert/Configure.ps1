[CmdletBinding()]
param(
    [PSCredential] $Credential,
    [Parameter(Mandatory=$False, HelpMessage='Tenant ID (This is a GUID which represents the "Directory ID" of the AzureAD tenant into which you want to create the apps')]
    [string] $tenantId
)

<#
 This script creates the Azure AD applications needed for this sample and updates the configuration files
 for the visual Studio projects from the data in the Azure AD applications.

 Before running this script you need to install the AzureAD cmdlets as an administrator. 
 For this:
 1) Run Powershell as an administrator
 2) in the PowerShell window, type: Install-Module AzureAD

 There are four ways to run this script. For more information, read the AppCreationScripts.md file in the same folder as this script.
#>

# Create a password that can be used as an application key
Function ComputePassword
{
    $aesManaged = New-Object "System.Security.Cryptography.AesManaged"
    $aesManaged.Mode = [System.Security.Cryptography.CipherMode]::CBC
    $aesManaged.Padding = [System.Security.Cryptography.PaddingMode]::Zeros
    $aesManaged.BlockSize = 128
    $aesManaged.KeySize = 256
    $aesManaged.GenerateKey()
    return [System.Convert]::ToBase64String($aesManaged.Key)
}

# Create an application key
# See https://www.sabin.io/blog/adding-an-azure-active-directory-application-and-key-using-powershell/
Function CreateAppKey([DateTime] $fromDate, [double] $durationInYears, [string]$pw)
{
    $endDate = $fromDate.AddYears($durationInYears) 
    $keyId = (New-Guid).ToString();
    $key = New-Object Microsoft.Open.AzureAD.Model.PasswordCredential
    $key.StartDate = $fromDate
    $key.EndDate = $endDate
    $key.Value = $pw
    $key.KeyId = $keyId
    return $key
}

# Adds the requiredAccesses (expressed as a pipe separated string) to the requiredAccess structure
# The exposed permissions are in the $exposedPermissions collection, and the type of permission (Scope | Role) is 
# described in $permissionType
Function AddResourcePermission($requiredAccess, `
                               $exposedPermissions, [string]$requiredAccesses, [string]$permissionType)
{
        foreach($permission in $requiredAccesses.Trim().Split("|"))
        {
            foreach($exposedPermission in $exposedPermissions)
            {
                if ($exposedPermission.Value -eq $permission)
                 {
                    $resourceAccess = New-Object Microsoft.Open.AzureAD.Model.ResourceAccess
                    $resourceAccess.Type = $permissionType # Scope = Delegated permissions | Role = Application permissions
                    $resourceAccess.Id = $exposedPermission.Id # Read directory data
                    $requiredAccess.ResourceAccess.Add($resourceAccess)
                 }
            }
        }
}

#
# Exemple: GetRequiredPermissions "Microsoft Graph"  "Graph.Read|User.Read"
# See also: http://stackoverflow.com/questions/42164581/how-to-configure-a-new-azure-ad-application-through-powershell
Function GetRequiredPermissions([string] $applicationDisplayName, [string] $requiredDelegatedPermissions, [string]$requiredApplicationPermissions, $servicePrincipal)
{
    # If we are passed the service principal we use it directly, otherwise we find it from the display name (which might not be unique)
    if ($servicePrincipal)
    {
        $sp = $servicePrincipal
    }
    else
    {
        $sp = Get-AzureADServicePrincipal -Filter "DisplayName eq '$applicationDisplayName'"
    }
    $appid = $sp.AppId
    $requiredAccess = New-Object Microsoft.Open.AzureAD.Model.RequiredResourceAccess
    $requiredAccess.ResourceAppId = $appid 
    $requiredAccess.ResourceAccess = New-Object System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.ResourceAccess]

    # $sp.Oauth2Permissions | Select Id,AdminConsentDisplayName,Value: To see the list of all the Delegated permissions for the application:
    if ($requiredDelegatedPermissions)
    {
        AddResourcePermission $requiredAccess -exposedPermissions $sp.Oauth2Permissions -requiredAccesses $requiredDelegatedPermissions -permissionType "Scope"
    }
    
    # $sp.AppRoles | Select Id,AdminConsentDisplayName,Value: To see the list of all the Application permissions for the application
    if ($requiredApplicationPermissions)
    {
        AddResourcePermission $requiredAccess -exposedPermissions $sp.AppRoles -requiredAccesses $requiredApplicationPermissions -permissionType "Role"
    }
    return $requiredAccess
}


Function UpdateLine([string] $line, [string] $value)
{
    $index = $line.IndexOf('=')
    $delimiter = ';'
    if ($index -eq -1)
    {
        $index = $line.IndexOf(':')
        $delimiter = ','
    }
    if ($index -ige 0)
    {
        $line = $line.Substring(0, $index+1) + " "+'"'+$value+'"'+$delimiter
    }
    return $line
}

Function UpdateTextFile([string] $configFilePath, [System.Collections.HashTable] $dictionary)
{
    $lines = Get-Content $configFilePath
    $index = 0
    while($index -lt $lines.Length)
    {
        $line = $lines[$index]
        foreach($key in $dictionary.Keys)
        {
            if ($line.Contains($key))
            {
                $lines[$index] = UpdateLine $line $dictionary[$key]
            }
        }
        $index++
    }

    Set-Content -Path $configFilePath -Value $lines -Force
}
Function CreateAppRole([string] $types, [string] $name, [string] $description)
{
    $appRole = New-Object Microsoft.Open.AzureAD.Model.AppRole
    $appRole.AllowedMemberTypes = New-Object System.Collections.Generic.List[string]
    $typesArr = $types.Split(',')
    foreach($type in $typesArr)
    {
        $appRole.AllowedMemberTypes.Add($type);
    }
    $appRole.DisplayName = $name
    $appRole.Id = New-Guid
    $appRole.IsEnabled = $true
    $appRole.Description = $description
    $appRole.Value = $name;
    return $appRole
}


Set-Content -Value "<html><body><table>" -Path createdApps.html
Add-Content -Value "<thead><tr><th>Application</th><th>AppId</th><th>Url in the Azure portal</th></tr></thead><tbody>" -Path createdApps.html

Function ConfigureApplications
{
<#.Description
   This function creates the Azure AD applications for the sample in the provided Azure AD tenant and updates the
   configuration files in the client and service project  of the visual studio solution (App.Config and Web.Config)
   so that they are consistent with the Applications parameters
#> 

    $commonendpoint = "common"

    # $tenantId is the Active Directory Tenant. This is a GUID which represents the "Directory ID" of the AzureAD tenant
    # into which you want to create the apps. Look it up in the Azure portal in the "Properties" of the Azure AD.

    # Login to Azure PowerShell (interactive if credentials are not already provided:
    # you'll need to sign-in with creds enabling your to create apps in the tenant)
    if (!$Credential -and $TenantId)
    {
        $creds = Connect-AzureAD -TenantId $tenantId
    }
    else
    {
        if (!$TenantId)
        {
            $creds = Connect-AzureAD -Credential $Credential
        }
        else
        {
            $creds = Connect-AzureAD -TenantId $tenantId -Credential $Credential
        }
    }

    if (!$tenantId)
    {
        $tenantId = $creds.Tenant.Id
    }

    $tenant = Get-AzureADTenantDetail
    $tenantName =  ($tenant.VerifiedDomains | Where { $_._Default -eq $True }).Name

    # Get the user running the script
    $user = Get-AzureADUser -ObjectId $creds.Account.Id

   # Create the service AAD application
   Write-Host "Creating the AAD application (TodoList-webapi-daemon-v2)"
   $serviceAadApplication = New-AzureADApplication -DisplayName "TodoList-webapi-daemon-v2" `
                                                   -HomePage "https://localhost:44372" `
                                                   -PublicClient $False
   $serviceIdentifierUri = 'api://'+$serviceAadApplication.AppId
   Set-AzureADApplication -ObjectId $serviceAadApplication.ObjectId -IdentifierUris $serviceIdentifierUri

   $currentAppId = $serviceAadApplication.AppId
   $serviceServicePrincipal = New-AzureADServicePrincipal -AppId $currentAppId -Tags {WindowsAzureActiveDirectoryIntegratedApp}

   # add the user running the script as an app owner if needed
   $owner = Get-AzureADApplicationOwner -ObjectId $serviceAadApplication.ObjectId
   if ($owner -eq $null)
   { 
        Add-AzureADApplicationOwner -ObjectId $serviceAadApplication.ObjectId -RefObjectId $user.ObjectId
        Write-Host "'$($user.UserPrincipalName)' added as an application owner to app '$($serviceServicePrincipal.DisplayName)'"
   }

   # Add application Roles
   $appRoles = New-Object System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.AppRole]
   $newRole = CreateAppRole -types "Application" -name "DaemonAppRole" -description "Daemon apps in this role can consume the web api."
   $appRoles.Add($newRole)
   Set-AzureADApplication -ObjectId $serviceAadApplication.ObjectId -AppRoles $appRoles

   Write-Host "Done creating the service application (TodoList-webapi-daemon-v2)"

   # URL of the AAD application in the Azure portal
   # Future? $servicePortalUrl = "https://portal.azure.com/#@"+$tenantName+"/blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/Overview/appId/"+$serviceAadApplication.AppId+"/objectId/"+$serviceAadApplication.ObjectId+"/isMSAApp/"
   $servicePortalUrl = "https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/CallAnAPI/appId/"+$serviceAadApplication.AppId+"/objectId/"+$serviceAadApplication.ObjectId+"/isMSAApp/"
   Add-Content -Value "<tr><td>service</td><td>$currentAppId</td><td><a href='$servicePortalUrl'>TodoList-webapi-daemon-v2</a></td></tr>" -Path createdApps.html

   # Create the client AAD application
   Write-Host "Creating the AAD application (daemon-console-v2)"
   # Get a 2 years application key for the client Application
   $pw = ComputePassword
   $fromDate = [DateTime]::Now;
   $key = CreateAppKey -fromDate $fromDate -durationInYears 2 -pw $pw
   $clientAppKey = $pw
   $clientAadApplication = New-AzureADApplication -DisplayName "daemon-console-v2" `
                                                  -ReplyUrls "https://daemon" `
                                                  -IdentifierUris "https://$tenantName/daemon-console-v2" `
                                                  -PasswordCredentials $key `
                                                  -PublicClient $False

   # Generate a certificate
   Write-Host "Creating the client application (daemon-console-v2)"
   $certificate=New-SelfSignedCertificate -Subject CN=DaemonConsoleCert `
                                           -CertStoreLocation "Cert:\CurrentUser\My" `
                                           -KeyExportPolicy Exportable `
                                           -KeySpec Signature
   $certKeyId = [Guid]::NewGuid()
   $certBase64Value = [System.Convert]::ToBase64String($certificate.GetRawCertData())
   $certBase64Thumbprint = [System.Convert]::ToBase64String($certificate.GetCertHash())

   # Add a Azure Key Credentials from the certificate for the daemon application
   $clientKeyCredentials = New-AzureADApplicationKeyCredential -ObjectId $clientAadApplication.ObjectId `
                                                                    -CustomKeyIdentifier "CN=DaemonConsoleCert" `
                                                                    -Type AsymmetricX509Cert `
                                                                    -Usage Verify `
                                                                    -Value $certBase64Value `
                                                                    -StartDate $certificate.NotBefore `
                                                                    -EndDate $certificate.NotAfter

   $currentAppId = $clientAadApplication.AppId
   $clientServicePrincipal = New-AzureADServicePrincipal -AppId $currentAppId -Tags {WindowsAzureActiveDirectoryIntegratedApp}

   # add the user running the script as an app owner if needed
   $owner = Get-AzureADApplicationOwner -ObjectId $clientAadApplication.ObjectId
   if ($owner -eq $null)
   { 
        Add-AzureADApplicationOwner -ObjectId $clientAadApplication.ObjectId -RefObjectId $user.ObjectId
        Write-Host "'$($user.UserPrincipalName)' added as an application owner to app '$($clientServicePrincipal.DisplayName)'"
   }


   Write-Host "Done creating the client application (daemon-console-v2)"

   # URL of the AAD application in the Azure portal
   # Future? $clientPortalUrl = "https://portal.azure.com/#@"+$tenantName+"/blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/Overview/appId/"+$clientAadApplication.AppId+"/objectId/"+$clientAadApplication.ObjectId+"/isMSAApp/"
   $clientPortalUrl = "https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/CallAnAPI/appId/"+$clientAadApplication.AppId+"/objectId/"+$clientAadApplication.ObjectId+"/isMSAApp/"
   Add-Content -Value "<tr><td>client</td><td>$currentAppId</td><td><a href='$clientPortalUrl'>daemon-console-v2</a></td></tr>" -Path createdApps.html

   $requiredResourcesAccess = New-Object System.Collections.Generic.List[Microsoft.Open.AzureAD.Model.RequiredResourceAccess]

   # Add Required Resources Access (from 'client' to 'service')
   Write-Host "Getting access from 'client' to 'service'"
   $requiredPermissions = GetRequiredPermissions -applicationDisplayName "TodoList-webapi-daemon-v2" `
                                                -requiredApplicationPermissions "DaemonAppRole" `

   $requiredResourcesAccess.Add($requiredPermissions)


   Set-AzureADApplication -ObjectId $clientAadApplication.ObjectId -RequiredResourceAccess $requiredResourcesAccess
   Write-Host "Granted permissions."

   # Update config file for 'service'
   $configFile = $pwd.Path + "\..\TodoList-WebApi\appsettings.json"
   Write-Host "Updating the sample code ($configFile)"
   $dictionary = @{ "Domain" = $tenantName;"TenantId" = $tenantId;"ClientId" = $serviceAadApplication.AppId };
   UpdateTextFile -configFilePath $configFile -dictionary $dictionary

   # Update config file for 'client'
   $configFile = $pwd.Path + "\..\Daemon-Console\appsettings.json"
   Write-Host "Updating the sample code ($configFile)"
   $dictionary = @{ "Tenant" = $tenantName;"ClientId" = $clientAadApplication.AppId;"ClientSecret" = '';"CertificateName" = "CN=DaemonConsoleCert";"TodoListScope" = ("api://"+$serviceAadApplication.AppId+"/.default");"TodoListBaseAddress" = $serviceAadApplication.HomePage };
   UpdateTextFile -configFilePath $configFile -dictionary $dictionary
   Write-Host ""
   Write-Host -ForegroundColor Green "------------------------------------------------------------------------------------------------" 
   Write-Host "IMPORTANT: Please follow the instructions below to complete a few manual step(s) in the Azure portal":
   Write-Host "- For 'client'"
   Write-Host "  - Navigate to '$clientPortalUrl'"
   Write-Host "  - Navigate to the API permissions page and click on 'Grant admin consent for {tenant}'" -ForegroundColor Red 

   Write-Host -ForegroundColor Green "------------------------------------------------------------------------------------------------" 
     
   Add-Content -Value "</tbody></table></body></html>" -Path createdApps.html  
}

# Pre-requisites
if ((Get-Module -ListAvailable -Name "AzureAD") -eq $null) { 
    Install-Module "AzureAD" -Scope CurrentUser 
} 
Import-Module AzureAD

# Run interactively (will ask you for the tenant ID)
ConfigureApplications -Credential $Credential -tenantId $TenantId