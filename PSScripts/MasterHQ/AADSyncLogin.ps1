<#
Logging in via AAD Service Principal
#>
$ClientID="ea41f282-dc3c-482f-b679-cb1405fa1481"
$ClientKey="4wt7wtWJolyMH3WAsxV1nYRD1ApvBgnfkdxio4N40pI="
$TenantID="a038f999-4e71-40c4-aa5b-f1aec8ae12e3"
$Tenant="MSTND786974.onmicrosoft.com"

#Get Token via REST
$loginURL = "https://login.microsoftonline.com"
$resource="https://graph.windows.net"
$body = @{grant_type="client_credentials";resource=$resource;client_id=$ClientID;client_secret=$ClientKey}
$LoginUri = "$loginURL/$TenantID/oauth2/token?api-version=1.0"
$oauth = Invoke-RestMethod -Method Post -Uri $LoginUri -Body $body
$AccessToken = $oauth.access_token

<#
#Get Token via ADAL
Add-Type -Path 'C:\Program Files\WindowsPowerShell\Modules\AzureADPreview\2.0.0.98\Microsoft.IdentityModel.Clients.ActiveDirectory.dll'

[Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential]$adCred = New-Object Microsoft.IdentityModel.Clients.ActiveDirectory.ClientCredential ($ClientID, $ClientKey)

$authenticationContext = New-Object Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext -ArgumentList @(
    "https://login.microsoftonline.com/$TenantID" #authority
    $false                             #validateAuthority
)

$resource    = "https://graph.windows.net"
$authenticationResult = ($authenticationContext.AcquireTokenAsync($resource, $adCred)).Result

$AccessToken = $authenticationResult.AccessToken
#>

AzureAD\Connect-AzureAD -TenantId $TenantID -MsAccessToken $AccessToken -AadAccessToken $AccessToken -AccountId "https://$Tenant/974e0d68-3d7a-4459-a9c5-4eacddc965fb"
#AzureADPreview\Connect-AzureAD -TenantId $TenantID -MsAccessToken $AccessToken -AadAccessToken $AccessToken -AccountId "https://$Tenant/974e0d68-3d7a-4459-a9c5-4eacddc965fb"


<#
#Manual login
try {
    Get-AzureADDomain -ErrorAction Stop
} catch {
    Connect-AzureAD
}
#>
