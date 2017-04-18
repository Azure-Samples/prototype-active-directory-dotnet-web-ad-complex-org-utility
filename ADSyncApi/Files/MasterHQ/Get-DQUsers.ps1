function Get-DQUsers {
    param (
        [Parameter( Position = 0, Mandatory = $true)]
        [string]$AADToken,

        [Parameter( Position = 1, Mandatory = $true)]
        [string]$TenantID,

        [Parameter( Position = 2)]
        [string]$DeltaLink
    )

    #&`$select=displayName,givenName,surname,emailAddress,userPrincipalName,userType,immutableId
    $apiVersion="2013-04-05"
    $select="&`$filter=isof('Microsoft.WindowsAzure.ActiveDirectory.User')"
    $endPoint = "https://graph.windows.net/{0}/users?api-version={1}&deltaLink=" -f $TenantID, $apiVersion

    $hasToken=$false

    if ($DeltaLink.Length -gt 0) {
        $endPoint+=$DeltaLink
        $hasToken=$true
    }

    if (!$hasToken) {
        $endPoint+=$select
    }

    $Headers = @{Authorization = "Bearer "+$AADToken; "ocp-aad-dq-include-only-changed-properties" = "true"}

    $res = Invoke-WebRequest -Uri $endPoint -Method Get -Headers $Headers
    $json = ConvertFrom-Json $res.Content 
    $DeltaLink = $json.Content.'aad.deltaLink'
    return $json
}



function Get-AADUsersGraph {
    param (
        [Parameter( Position = 4, Mandatory = $true)]
        [string]$GraphToken
    )

    $endPoint = "https://graph.microsoft.com/beta/users?`$select=id,displayName,mail,userPrincipalName,source,userType,altSecID,CreationType"
    $Headers = @{Authorization = "Bearer "+$GraphToken}

    $res = Invoke-WebRequest -Uri $endPoint -Method Get -Headers $Headers
    return (ConvertFrom-Json $res.Content).value
}