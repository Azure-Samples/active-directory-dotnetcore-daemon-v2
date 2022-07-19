
[CmdletBinding()]
param(
    [Parameter(Mandatory=$False, HelpMessage='Tenant ID (This is a GUID which represents the "Directory ID" of the AzureAD tenant into which you want to create the apps')]
    [string] $tenantId,
    [Parameter(Mandatory=$False, HelpMessage='Azure environment to use while running the script. Default = Global')]
    [string] $azureEnvironmentName
)

<#
 This script creates the Azure AD applications needed for this sample and updates the configuration files
 for the visual Studio projects from the data in the Azure AD applications.

 In case you don't have Microsoft.Graph.Applications already installed, the script will automatically install it for the current user
 
 There are four ways to run this script. For more information, read the AppCreationScripts.md file in the same folder as this script.
#>

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
                $resourceAccess = New-Object Microsoft.Graph.PowerShell.Models.MicrosoftGraphResourceAccess
                $resourceAccess.Type = $permissionType # Scope = Delegated permissions | Role = Application permissions
                $resourceAccess.Id = $exposedPermission.Id # Read directory data
                $requiredAccess.ResourceAccess += $resourceAccess
                }
        }
    }
}

#
# Example: GetRequiredPermissions "Microsoft Graph"  "Graph.Read|User.Read"
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
        $sp = Get-MgServicePrincipal -Filter "DisplayName eq '$applicationDisplayName'"
    }
    $appid = $sp.AppId
    $requiredAccess = New-Object Microsoft.Graph.PowerShell.Models.MicrosoftGraphRequiredResourceAccess
    $requiredAccess.ResourceAppId = $appid 
    $requiredAccess.ResourceAccess = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphResourceAccess]

    # $sp.Oauth2Permissions | Select Id,AdminConsentDisplayName,Value: To see the list of all the Delegated permissions for the application:
    if ($requiredDelegatedPermissions)
    {
        AddResourcePermission $requiredAccess -exposedPermissions $sp.Oauth2PermissionScopes -requiredAccesses $requiredDelegatedPermissions -permissionType "Scope"
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
    $index = $line.IndexOf(':')
    $lineEnd = ''

    if($line[$line.Length - 1] -eq ','){   $lineEnd = ',' }
    
    if ($index -ige 0)
    {
        $line = $line.Substring(0, $index+1) + " " + '"' + $value+ '"' + $lineEnd
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
<#.Description
   This function creates a new Azure AD scope (OAuth2Permission) with default and provided values
#>  
Function CreateScope( [string] $value, [string] $userConsentDisplayName, [string] $userConsentDescription, [string] $adminConsentDisplayName, [string] $adminConsentDescription)
{
    $scope = New-Object Microsoft.Graph.PowerShell.Models.MicrosoftGraphPermissionScope
    $scope.Id = New-Guid
    $scope.Value = $value
    $scope.UserConsentDisplayName = $userConsentDisplayName
    $scope.UserConsentDescription = $userConsentDescription
    $scope.AdminConsentDisplayName = $adminConsentDisplayName
    $scope.AdminConsentDescription = $adminConsentDescription
    $scope.IsEnabled = $true
    $scope.Type = "User"
    return $scope
}

<#.Description
   This function creates a new Azure AD AppRole with default and provided values
#>  
Function CreateAppRole([string] $types, [string] $name, [string] $description)
{
    $appRole = New-Object Microsoft.Graph.PowerShell.Models.MicrosoftGraphAppRole
    $appRole.AllowedMemberTypes = New-Object System.Collections.Generic.List[string]
    $typesArr = $types.Split(',')
    foreach($type in $typesArr)
    {
        $appRole.AllowedMemberTypes += $type;
    }
    $appRole.DisplayName = $name
    $appRole.Id = New-Guid
    $appRole.IsEnabled = $true
    $appRole.Description = $description
    $appRole.Value = $name;
    return $appRole
}
Function CreateOptionalClaim([string] $name)
{
    <#.Description
    This function creates a new Azure AD optional claims  with default and provided values
    #>  

    $appClaim = New-Object Microsoft.Graph.PowerShell.Models.MicrosoftGraphOptionalClaim
    $appClaim.AdditionalProperties =  New-Object System.Collections.Generic.List[string]
    $appClaim.Source =  $null
    $appClaim.Essential = $false
    $appClaim.Name = $name
    return $appClaim
}


Function ConfigureApplications
{
    <#.Description
       This function creates the Azure AD applications for the sample in the provided Azure AD tenant and updates the
       configuration files in the client and service project  of the visual studio solution (App.Config and Web.Config)
       so that they are consistent with the Applications parameters
    #> 
    
    if (!$azureEnvironmentName)
    {
        $azureEnvironmentName = "Global"
    }

    # Connect to the Microsoft Graph API, non-interactive is not supported for the moment (Oct 2021)
    Write-Host "Connecting to Microsoft Graph"
    if ($tenantId -eq "") {
        Connect-MgGraph -Scopes "Application.ReadWrite.All" -Environment $azureEnvironmentName
        $tenantId = (Get-MgContext).TenantId
    }
    else {
        Connect-MgGraph -TenantId $tenantId -Scopes "Application.ReadWrite.All" -Environment $azureEnvironmentName
    }
    

   # Create the service AAD application
   Write-Host "Creating the AAD application (TodoList-webapi-daemon-v2)"
   
   # create the application 
   $serviceAadApplication = New-MgApplication -DisplayName "TodoList-webapi-daemon-v2" `
                                                       -Web `
                                                       @{ `
                                                           HomePageUrl = "https://localhost:44372"; `
                                                         } `
                                                         -Api `
                                                         @{ `
                                                            RequestedAccessTokenVersion = 2 `
                                                         } `
                                                        -SignInAudience AzureADMyOrg `
                                                       #end of command
    $serviceIdentifierUri = 'api://'+$serviceAadApplication.AppId
    Update-MgApplication -ApplicationId $serviceAadApplication.Id -IdentifierUris @($serviceIdentifierUri)
    
    # create the service principal of the newly created application 
    $currentAppId = $serviceAadApplication.AppId
    $serviceServicePrincipal = New-MgServicePrincipal -AppId $currentAppId -Tags {WindowsAzureActiveDirectoryIntegratedApp}

    # add the user running the script as an app owner if needed
    $owner = Get-MgApplicationOwner -ApplicationId $serviceAadApplication.Id
    if ($owner -eq $null)
    { 
        New-MgApplicationOwnerByRef -ApplicationId $serviceAadApplication.Id  -BodyParameter = @{"@odata.id" = "htps://graph.microsoft.com/v1.0/directoryObjects/$user.ObjectId"}
        Write-Host "'$($user.UserPrincipalName)' added as an application owner to app '$($serviceServicePrincipal.DisplayName)'"
    }

    # Add Claims

    $optionalClaims = New-Object Microsoft.Graph.PowerShell.Models.MicrosoftGraphOptionalClaims
    $optionalClaims.AccessToken = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphOptionalClaim]
    $optionalClaims.IdToken = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphOptionalClaim]
    $optionalClaims.Saml2Token = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphOptionalClaim]


    # Add Optional Claims

    $newClaim =  CreateOptionalClaim  -name "idtyp" 
    $optionalClaims.AccessToken += ($newClaim)
    Update-MgApplication -ApplicationId $serviceAadApplication.Id -OptionalClaims $optionalClaims
    
    # Add application permissions/user roles
    $appRoles = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphAppRole]
    $newRole = CreateAppRole -types "Application" -name "Todo.ReadWrite.All" -description "An application permissions that gives you read and write access for all to-do's"
    $appRoles.Add($newRole)
    Update-MgApplication -ApplicationId $serviceAadApplication.Id -AppRoles $appRoles
    
    # rename the user_impersonation scope if it exists to match the readme steps or add a new scope
       
    # delete default scope i.e. User_impersonation
    # Alex: the scope deletion doesn't work - see open issue - https://github.com/microsoftgraph/msgraph-sdk-powershell/issues/1054
    $scopes = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphPermissionScope]
    $scope = $serviceAadApplication.Api.Oauth2PermissionScopes | Where-Object { $_.Value -eq "User_impersonation" }
    
    if($scope -ne $null)
    {    
        # disable the scope
        $scope.IsEnabled = $false
        $scopes.Add($scope)
        Update-MgApplication -ApplicationId $serviceAadApplication.Id -Api @{Oauth2PermissionScopes = @($scopes)}

        # clear the scope
        Update-MgApplication -ApplicationId $serviceAadApplication.Id -Api @{Oauth2PermissionScopes = @()}
    }

    $scopes = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphPermissionScope]
    $scope = CreateScope -value Todo.Write  `
    -userConsentDisplayName "Access TodoList-webapi-daemon-v2"  `
    -userConsentDescription "Allow the application to access TodoList-webapi-daemon-v2 on your behalf."  `
    -adminConsentDisplayName "Access TodoList-webapi-daemon-v2"  `
    -adminConsentDescription "Allow the app TodoList-webapi-daemon-v2 to [ex, read ToDo list items]"
            
    $scopes.Add($scope)
    $scope = CreateScope -value Todo.Read  `
    -userConsentDisplayName "Access TodoList-webapi-daemon-v2"  `
    -userConsentDescription "Allow the application to access TodoList-webapi-daemon-v2 on your behalf."  `
    -adminConsentDisplayName "Access TodoList-webapi-daemon-v2"  `
    -adminConsentDescription "Allow the app TodoList-webapi-daemon-v2 to [ex, read ToDo list items]"
            
    $scopes.Add($scope)
    
    # add/update scopes
    Update-MgApplication -ApplicationId $serviceAadApplication.Id -Api @{Oauth2PermissionScopes = @($scopes)}
    Write-Host "Done creating the service application (TodoList-webapi-daemon-v2)"

    # URL of the AAD application in the Azure portal
    # Future? $servicePortalUrl = "https://portal.azure.com/#@"+$tenantName+"/blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/Overview/appId/"+$serviceAadApplication.AppId+"/objectId/"+$serviceAadApplication.Id+"/isMSAApp/"
    $servicePortalUrl = "https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/CallAnAPI/appId/"+$serviceAadApplication.AppId+"/objectId/"+$serviceAadApplication.Id+"/isMSAApp/"
    Add-Content -Value "<tr><td>service</td><td>$currentAppId</td><td><a href='$servicePortalUrl'>TodoList-webapi-daemon-v2</a></td></tr>" -Path createdApps.html

   # Create the client AAD application
   Write-Host "Creating the AAD application (daemon-console-v2)"
   
   # create the application 
   $clientAadApplication = New-MgApplication -DisplayName "daemon-console-v2" `
                                                      -Web `
                                                      @{ `
                                                          RedirectUris = "https://localhost:7238"; `
                                                        } `
                                                       -SignInAudience AzureADMyOrg `
                                                      #end of command
    $tenantName = (Get-MgApplication -ApplicationId $clientAadApplication.Id).PublisherDomain
    Update-MgApplication -ApplicationId $clientAadApplication.Id -IdentifierUris @("https://$tenantName/daemon-console-v2")
    
    # Generate a certificate
    Write-Host "Creating the client application (daemon-console-v2)"

    $certificateName = 'daemon-console-v2'

    # temporarily disable the option and procees to certificate creation
    #$isOpenSSL = Read-Host ' By default certificate is generated using New-SelfSignedCertificate. Do you want to generate cert using OpenSSL(Y/N)?'
    $isOpenSSl = 'N'
    if($isOpenSSL -eq 'Y')
    {
        $certificate=openssl req -x509 -newkey rsa:4096 -sha256 -days 365 -keyout "$certificateName.key" -out "$certificateName.cer" -nodes -batch
        openssl pkcs12 -export -out "$certificateName.pfx" -inkey $certificateName.key -in "$certificateName.cer"
    }
    else
    {
        $certificate=New-SelfSignedCertificate -Subject $certificateName `
                                                -CertStoreLocation "Cert:\CurrentUser\My" `
                                                -KeyExportPolicy Exportable `
                                                -KeySpec Signature

        $thumbprint = $certificate.Thumbprint
        $certificatePassword = Read-Host -Prompt "Enter password for your certificate (Please remember the password, you will need it when uploading to KeyVault): " -AsSecureString
        Write-Host "Exporting certificate as a PFX file"
        Export-PfxCertificate -Cert "Cert:\Currentuser\My\$thumbprint" -FilePath "$pwd\$certificateName.pfx" -ChainOption EndEntityCertOnly -NoProperties -Password $certificatePassword
        Write-Host "PFX written to:"
        Write-Host "$pwd\$certificateName.pfx"

        # Add a Azure Key Credentials from the certificate for the application
        $clientKeyCredentials = Update-MgApplication -ApplicationId $clientAadApplication.Id `
            -KeyCredentials @(@{Type = "AsymmetricX509Cert"; Usage = "Verify"; Key= $certificate.RawData; StartDateTime = $certificate.NotBefore; EndDateTime = $certificate.NotAfter;})       
       
    }
  
    
    # create the service principal of the newly created application 
    $currentAppId = $clientAadApplication.AppId
    $clientServicePrincipal = New-MgServicePrincipal -AppId $currentAppId -Tags {WindowsAzureActiveDirectoryIntegratedApp}

    # add the user running the script as an app owner if needed
    $owner = Get-MgApplicationOwner -ApplicationId $clientAadApplication.Id
    if ($owner -eq $null)
    { 
        New-MgApplicationOwnerByRef -ApplicationId $clientAadApplication.Id  -BodyParameter = @{"@odata.id" = "htps://graph.microsoft.com/v1.0/directoryObjects/$user.ObjectId"}
        Write-Host "'$($user.UserPrincipalName)' added as an application owner to app '$($clientServicePrincipal.DisplayName)'"
    }

    # Add Claims

    $optionalClaims = New-Object Microsoft.Graph.PowerShell.Models.MicrosoftGraphOptionalClaims
    $optionalClaims.AccessToken = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphOptionalClaim]
    $optionalClaims.IdToken = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphOptionalClaim]
    $optionalClaims.Saml2Token = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphOptionalClaim]


    # Add Optional Claims

    $newClaim =  CreateOptionalClaim  -name "idtyp" 
    $optionalClaims.AccessToken += ($newClaim)
    Update-MgApplication -ApplicationId $clientAadApplication.Id -OptionalClaims $optionalClaims
    
    # Add application permissions/user roles
    $appRoles = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphAppRole]
    Update-MgApplication -ApplicationId $clientAadApplication.Id -AppRoles $appRoles
    Write-Host "Done creating the client application (daemon-console-v2)"

    # URL of the AAD application in the Azure portal
    # Future? $clientPortalUrl = "https://portal.azure.com/#@"+$tenantName+"/blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/Overview/appId/"+$clientAadApplication.AppId+"/objectId/"+$clientAadApplication.Id+"/isMSAApp/"
    $clientPortalUrl = "https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/CallAnAPI/appId/"+$clientAadApplication.AppId+"/objectId/"+$clientAadApplication.Id+"/isMSAApp/"
    Add-Content -Value "<tr><td>client</td><td>$currentAppId</td><td><a href='$clientPortalUrl'>daemon-console-v2</a></td></tr>" -Path createdApps.html
    $requiredResourcesAccess = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphRequiredResourceAccess]

    
    # Add Required Resources Access (from 'client' to 'service')
    Write-Host "Getting access from 'client' to 'service'"
    $requiredPermissions = GetRequiredPermissions -applicationDisplayName "TodoList-webapi-daemon-v2" `
        -requiredApplicationPermissions "Todo.ReadWrite.All" `
    

    $requiredResourcesAccess.Add($requiredPermissions)
    Update-MgApplication -ApplicationId $clientAadApplication.Id -RequiredResourceAccess $requiredResourcesAccess
    Write-Host "Granted permissions."

   # Create the webApp AAD application
   Write-Host "Creating the AAD application (daemon-console-v2-sample-app)"
   
   # create the application 
   $webAppAadApplication = New-MgApplication -DisplayName "daemon-console-v2-sample-app" `
                                                      -Web `
                                                      @{ `
                                                          RedirectUris = "https://localhost:7238/signin-oidc"; `
                                                        } `
                                                       -SignInAudience AzureADMyOrg `
                                                      #end of command
    $tenantName = (Get-MgApplication -ApplicationId $webAppAadApplication.Id).PublisherDomain
    Update-MgApplication -ApplicationId $webAppAadApplication.Id -IdentifierUris @("https://$tenantName/daemon-console-v2-sample-app")
    
    # Generate a certificate
    Write-Host "Creating the webApp application (daemon-console-v2-sample-app)"

    $certificateName = 'daemon-console-v2-sample-app'

    # temporarily disable the option and procees to certificate creation
    #$isOpenSSL = Read-Host ' By default certificate is generated using New-SelfSignedCertificate. Do you want to generate cert using OpenSSL(Y/N)?'
    $isOpenSSl = 'N'
    if($isOpenSSL -eq 'Y')
    {
        $certificate=openssl req -x509 -newkey rsa:4096 -sha256 -days 365 -keyout "$certificateName.key" -out "$certificateName.cer" -nodes -batch
        openssl pkcs12 -export -out "$certificateName.pfx" -inkey $certificateName.key -in "$certificateName.cer"
    }
    else
    {
        $certificate=New-SelfSignedCertificate -Subject $certificateName `
                                                -CertStoreLocation "Cert:\CurrentUser\My" `
                                                -KeyExportPolicy Exportable `
                                                -KeySpec Signature

        $thumbprint = $certificate.Thumbprint
        $certificatePassword = Read-Host -Prompt "Enter password for your certificate (Please remember the password, you will need it when uploading to KeyVault): " -AsSecureString
        Write-Host "Exporting certificate as a PFX file"
        Export-PfxCertificate -Cert "Cert:\Currentuser\My\$thumbprint" -FilePath "$pwd\$certificateName.pfx" -ChainOption EndEntityCertOnly -NoProperties -Password $certificatePassword
        Write-Host "PFX written to:"
        Write-Host "$pwd\$certificateName.pfx"

        # Add a Azure Key Credentials from the certificate for the application
        $webAppKeyCredentials = Update-MgApplication -ApplicationId $webAppAadApplication.Id `
            -KeyCredentials @(@{Type = "AsymmetricX509Cert"; Usage = "Verify"; Key= $certificate.RawData; StartDateTime = $certificate.NotBefore; EndDateTime = $certificate.NotAfter;})       
       
    }
  
    
    # create the service principal of the newly created application 
    $currentAppId = $webAppAadApplication.AppId
    $webAppServicePrincipal = New-MgServicePrincipal -AppId $currentAppId -Tags {WindowsAzureActiveDirectoryIntegratedApp}

    # add the user running the script as an app owner if needed
    $owner = Get-MgApplicationOwner -ApplicationId $webAppAadApplication.Id
    if ($owner -eq $null)
    { 
        New-MgApplicationOwnerByRef -ApplicationId $webAppAadApplication.Id  -BodyParameter = @{"@odata.id" = "htps://graph.microsoft.com/v1.0/directoryObjects/$user.ObjectId"}
        Write-Host "'$($user.UserPrincipalName)' added as an application owner to app '$($webAppServicePrincipal.DisplayName)'"
    }

    # Add Claims

    $optionalClaims = New-Object Microsoft.Graph.PowerShell.Models.MicrosoftGraphOptionalClaims
    $optionalClaims.AccessToken = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphOptionalClaim]
    $optionalClaims.IdToken = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphOptionalClaim]
    $optionalClaims.Saml2Token = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphOptionalClaim]


    # Add Optional Claims

    Update-MgApplication -ApplicationId $webAppAadApplication.Id -OptionalClaims $optionalClaims
    
    # Add application permissions/user roles
    $appRoles = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphAppRole]
    Update-MgApplication -ApplicationId $webAppAadApplication.Id -AppRoles $appRoles
    Write-Host "Done creating the webApp application (daemon-console-v2-sample-app)"

    # URL of the AAD application in the Azure portal
    # Future? $webAppPortalUrl = "https://portal.azure.com/#@"+$tenantName+"/blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/Overview/appId/"+$webAppAadApplication.AppId+"/objectId/"+$webAppAadApplication.Id+"/isMSAApp/"
    $webAppPortalUrl = "https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/CallAnAPI/appId/"+$webAppAadApplication.AppId+"/objectId/"+$webAppAadApplication.Id+"/isMSAApp/"
    Add-Content -Value "<tr><td>webApp</td><td>$currentAppId</td><td><a href='$webAppPortalUrl'>daemon-console-v2-sample-app</a></td></tr>" -Path createdApps.html
    $requiredResourcesAccess = New-Object System.Collections.Generic.List[Microsoft.Graph.PowerShell.Models.MicrosoftGraphRequiredResourceAccess]

    
    # Add Required Resources Access (from 'webApp' to 'service')
    Write-Host "Getting access from 'webApp' to 'service'"
    $requiredPermissions = GetRequiredPermissions -applicationDisplayName "TodoList-webapi-daemon-v2" `
        -requiredDelegatedPermissions "Todo.Write|Todo.Read" `
    

    $requiredResourcesAccess.Add($requiredPermissions)
    Update-MgApplication -ApplicationId $webAppAadApplication.Id -RequiredResourceAccess $requiredResourcesAccess
    Write-Host "Granted permissions."
    
    # Update config file for 'service'
    $configFile = $pwd.Path + "\..\TodoList-WebApi\appsettings.json"
    $dictionary = @{  };

    Write-Host "Updating the sample code ($configFile)"

    UpdateTextFile -configFilePath $configFile -dictionary $dictionary
    
    # Update config file for 'client'
    $configFile = $pwd.Path + "\..\Daemon-Console\appsettings.json"
    $dictionary = @{  };

    Write-Host "Updating the sample code ($configFile)"

    UpdateTextFile -configFilePath $configFile -dictionary $dictionary
    
    # Update config file for 'webApp'
    $configFile = $pwd.Path + "\..\TodoList-WebApp\appsettings.json"
    $dictionary = @{  };

    Write-Host "Updating the sample code ($configFile)"

    UpdateTextFile -configFilePath $configFile -dictionary $dictionary
        
    $appSettingsObject = (Get-Content ..\TodoList-WebApi\appsettings.json | ConvertFrom-Json)

    # JSON is auto-generated.
    $appSettingsObject.AzureAd = ConvertFrom-Json "{""Instance"":""https://login.microsoftonline.com/"",""Domain"":""Auto"",""TenantId"":""Auto"",""ClientId"":""Auto"",""WithSpaAuthCode"":false,""ClientCertificates"":[]}";

    $appSettingsObject.AzureAd.TenantId = $tenantId;
    $appSettingsObject.AzureAd.ClientId = $serviceAadApplication.AppId;

    Write-Host "Updating the appsetings.json file at '..\TodoList-WebApi\appsettings.json'"
    $appSettingsObject | ConvertTo-Json -Depth 3 | Out-File ..\TodoList-WebApi\appsettings.json


        
    $appSettingsObject = (Get-Content ..\daemon-console\appsettings.json | ConvertFrom-Json)

    # JSON is auto-generated.
    $appSettingsObject.AzureAd = ConvertFrom-Json "{""Instance"":""https://login.microsoftonline.com/"",""Domain"":""Auto"",""TenantId"":""Auto"",""ClientId"":""Auto"",""WithSpaAuthCode"":false,""ClientCertificates"":[{""SourceType"":""StoreWithDistinguishedName"",""CertificateStorePath"":""CurrentUser/My"",""CertificateDistinguishedName"":""CN=daemon-console-v2""}]}";

    $appSettingsObject.AzureAd.TenantId = $tenantId;
    $appSettingsObject.AzureAd.ClientId = $clientAadApplication.AppId;
    $appSettingsObject.AzureAd.Domain = $tenantName;

    # JSON is auto-generated.
    $appSettingsObject.DownStreamApi = ConvertFrom-Json "{""BaseUrl"":""https://localhost:44372/"",""Scopes"":""$serviceIdentifierUri/.default""}";

    Write-Host "Updating the appsetings.json file at '..\daemon-console\appsettings.json'"
    $appSettingsObject | ConvertTo-Json -Depth 3 | Out-File ..\daemon-console\appsettings.json


        
    $appSettingsObject = (Get-Content ..\TodoList-WebApp\appsettings.json | ConvertFrom-Json)

    # JSON is auto-generated.
    $appSettingsObject.AzureAd = ConvertFrom-Json "{""Instance"":""https://login.microsoftonline.com/"",""Domain"":""Auto"",""TenantId"":""Auto"",""ClientId"":""Auto"",""CallbackPath"":""/signin-oidc"",""WithSpaAuthCode"":false,""ClientCertificates"":[{""SourceType"":""StoreWithDistinguishedName"",""CertificateStorePath"":""CurrentUser/My"",""CertificateDistinguishedName"":""CN=daemon-console-v2-sample-app""}]}";

    $appSettingsObject.AzureAd.TenantId = $tenantId;
    $appSettingsObject.AzureAd.ClientId = $webAppAadApplication.AppId;
    $appSettingsObject.AzureAd.Domain = $tenantName;

    # JSON is auto-generated.
    $appSettingsObject.DownStreamApi = ConvertFrom-Json "{""BaseUrl"":""https://localhost:44372/"",""Scopes"":""$serviceIdentifierUri/Todo.Read $serviceIdentifierUri/Todo.Write""}";

    Write-Host "Updating the appsetings.json file at '..\TodoList-WebApp\appsettings.json'"
    $appSettingsObject | ConvertTo-Json -Depth 3 | Out-File ..\TodoList-WebApp\appsettings.json


    Write-Host -ForegroundColor Green "------------------------------------------------------------------------------------------------" 
    Write-Host "IMPORTANT: Please follow the instructions below to complete a few manual step(s) in the Azure portal":
    Write-Host "- For client"
    Write-Host "  - Navigate to $clientPortalUrl"
    Write-Host "  - Navigate to the API permissions page and click on 'Grant admin consent for {tenant}'" -ForegroundColor Red 
    Write-Host -ForegroundColor Green "------------------------------------------------------------------------------------------------" 
       if($isOpenSSL -eq 'Y')
    {
        Write-Host -ForegroundColor Green "------------------------------------------------------------------------------------------------" 
        Write-Host "You have generated certificate using OpenSSL so follow below steps: "
        Write-Host "Install the certificate on your system from current folder."
        Write-Host -ForegroundColor Green "------------------------------------------------------------------------------------------------" 
    }
    Add-Content -Value "</tbody></table></body></html>" -Path createdApps.html  
}

# Pre-requisites
if ($null -eq (Get-Module -ListAvailable -Name "Microsoft.Graph.Applications")) {
    Install-Module "Microsoft.Graph.Applications" -Scope CurrentUser 
}

Import-Module Microsoft.Graph.Applications

Set-Content -Value "<html><body><table>" -Path createdApps.html
Add-Content -Value "<thead><tr><th>Application</th><th>AppId</th><th>Url in the Azure portal</th></tr></thead><tbody>" -Path createdApps.html

$ErrorActionPreference = "Stop"

# Run interactively (will ask you for the tenant ID)
ConfigureApplications -tenantId $tenantId -environment $azureEnvironmentName

Write-Host "Disconnecting from tenant"
Disconnect-MgGraph