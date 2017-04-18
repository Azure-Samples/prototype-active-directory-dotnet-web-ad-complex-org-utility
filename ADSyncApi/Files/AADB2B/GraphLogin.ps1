<#
Logging in via AAD Service Principal
#>
$ClientID="ea41f282-dc3c-482f-b679-cb1405fa1481"
$ClientKey="4wt7wtWJolyMH3WAsxV1nYRD1ApvBgnfkdxio4N40pI="
$TenantID="a038f999-4e71-40c4-aa5b-f1aec8ae12e3"

#Get Token via REST
$loginURL = "https://login.windows.net"
$resource="https://graph.microsoft.com"
$body = @{grant_type="client_credentials";resource=$resource;client_id=$ClientID;client_secret=$ClientKey}
$LoginUri = "$loginURL/$TenantID/oauth2/token?api-version=1.0"
$GraphOauth = Invoke-RestMethod -Method Post -Uri $LoginUri -Body $body
$GraphAccessToken = $GraphOauth.access_token
$GraphHeader = @{Authorization = "Bearer "+$GraphAccessToken}

