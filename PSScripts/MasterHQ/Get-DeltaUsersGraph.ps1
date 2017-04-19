function Get-DeltaUsersGraph {
    param (
        [Parameter( Position = 0, Mandatory = $true)]
        [string]$GraphToken,

        [Parameter( Position = 1)]
        [string]$SkipToken,

        [Parameter( Position = 2)]
        [string]$DeltaToken,

        [Parameter( Position = 3)]
        [string]$DeltaLink,

        [Parameter( Position = 4)]
        [string]$NextLink
    )

    $select="`$select-displayName,givenName,surnane,id,email,source,userPrincipalName,userType,immutableId"
    $endPoint = "https://graph.microsoft.com/beta/users/delta?"
    $hasToken=$false

    if ($SkipToken.Length -gt 0) {
        $endPoint+="`$skipToken=$SkipToken"
        $hasToken=$true
    }

    if ($DeltaToken.Length -gt 0) {
        $endPoint+="`$deltaToken=$DeltaToken"
        $hasToken=$true
    }

    if (!$hasToken) {
        $endPoint+=$select
    }

    $Headers = @{Authorization = "Bearer "+$GraphToken}

    $res = Invoke-WebRequest -Uri $endPoint -Method Get -Headers $Headers
    return ConvertFrom-Json $res.Content
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