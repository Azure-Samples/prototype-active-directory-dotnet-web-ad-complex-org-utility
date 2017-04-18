$SyncAPI_AuthHeader = @{}
$SyncAPI_UriRoot = ""

function Init-SyncAPI
{
    param(
        [string]$ApiKey,
        [string]$APISite
    )
    $global:SyncAPI_UriRoot = "$APISite/api/StagedUsersAdm"
    $global:SyncAPI_AuthHeader = @{apikey = $ApiKey}
}

function Get-DomainsToSync
{
    $Endpoint = "$SyncAPI_UriRoot/GetRemoteSiteList"
    $api = Invoke-WebRequest -Uri $Endpoint -Method GET -Headers $SyncAPI_AuthHeader
    $res = (ConvertFrom-Json -InputObject $api)
    return $res
}

function Get-NewStagedUsers
{
    $Endpoint = "$SyncAPI_UriRoot/GetAllByStage?stage=0"
    $api = Invoke-WebRequest -Uri $Endpoint -Method GET -Headers $SyncAPI_AuthHeader
    $res = (ConvertFrom-Json -InputObject $api)
    return $res
}

function Set-MasterGuid
{
    param(
        [Object[]]$UpdateUserBatch
    )

    $Endpoint = "$SyncAPI_UriRoot/UpdateBatchAdmin"

    #loadStage 1 = "PendingRemoteUpdate"
    $UpdateUserBatch | foreach { $_.loadState = 1 }

    $api = Invoke-WebRequest -Uri $Endpoint -Method Post -Headers $SyncAPI_AuthHeader -Body (ConvertTo-Json -InputObject $UpdateUserBatch) -ContentType "application/json"
    return $api
}
