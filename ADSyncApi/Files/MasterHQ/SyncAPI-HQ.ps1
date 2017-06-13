function Get-NewStagedUsers
{
    param(
        [parameter(Position=0, Mandatory=$true)]
        [ValidateSet("MasterHQ","AADB2B","LocalADOnly","All")]
        [string]$SiteType
    )

    $Type = Get-SiteType -SiteType $SiteType
    $Endpoint = "$global:SyncAPI_UriRoot/StagedUsersAdm/GetAllByStageAndSiteType?stage=0&type=$Type"

    if ($Type -eq $null)
    {
        #user chose "All"
        $Endpoint = "$global:SyncAPI_UriRoot/StagedUsersAdm/GetAllByStage?stage=0"
    }

    $api = Invoke-WebRequest -Uri $Endpoint -Method GET -Headers $global:SyncAPI_AuthHeader
    $res = (ConvertFrom-Json -InputObject $api)
    return $res
}

function Get-DomainsToSync
{
    $Endpoint = "$global:SyncAPI_UriRoot/StagedUsersAdm/GetRemoteSiteList"
    $api = Invoke-WebRequest -Uri $Endpoint -Method GET -Headers $global:SyncAPI_AuthHeader
    $res = (ConvertFrom-Json -InputObject $api)
    return $res
}

function Set-MasterGuid
{
    param(
        [Object[]]$UpdateUserBatch
    )

    $Endpoint = "$global:SyncAPI_UriRoot/StagedUsersAdm/UpdateBatchAdmin"

    #loadStage 1 = "PendingRemoteUpdate"
    $UpdateUserBatch | foreach { $_.loadState = 1 }

    $api = Invoke-WebRequest -Uri $Endpoint -Method Post -Headers $global:SyncAPI_AuthHeader -Body (ConvertTo-Json -InputObject $UpdateUserBatch) -ContentType "application/json"
    return $api
}

function Get-StagedUsersPendingUpdate
{
    $Endpoint = "$global:SyncAPI_UriRoot/StagedUsersAdm/GetAllByStage?stage=2"
    $api = Invoke-WebRequest -Uri $Endpoint -Method GET -Headers $global:SyncAPI_AuthHeader
    $res = (ConvertFrom-Json -InputObject $api)
    return $res
}

function Get-StagedUsersPendingDelete
{
    $Endpoint = "$global:SyncAPI_UriRoot/StagedUsersAdm/GetAllByStage?stage=4"
    $api = Invoke-WebRequest -Uri $Endpoint -Method GET -Headers $global:SyncAPI_AuthHeader
    $res = (ConvertFrom-Json -InputObject $api)
    return $res
}
