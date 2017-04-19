<#
Logging in via AAD Service Principal
#>

function Init-GraphAPI
{
    param(
        [parameter(Position=0, Mandatory=$true)]
        [object]$SyncVars
    )

    $AADClientID  = $SyncVars.AADClientID
    $AADClientKey = $SyncVars.AADClientKey
    $AADTenantID  = $SyncVars.AADTenantID

    #Get Token via REST
    $loginURL = "https://login.windows.net"
    $resource="https://graph.microsoft.com"
    $body = @{grant_type="client_credentials";resource=$resource;client_id=$AADClientID;client_secret=$AADClientKey}
    $LoginUri = "$loginURL/$AADTenantID/oauth2/token?api-version=1.0"
    $GraphOauth = Invoke-RestMethod -Method Post -Uri $LoginUri -Body $body
    $global:GraphAccessToken = $GraphOauth.access_token
    $global:GraphHeader = @{Authorization = "Bearer "+$GraphAccessToken}
}
