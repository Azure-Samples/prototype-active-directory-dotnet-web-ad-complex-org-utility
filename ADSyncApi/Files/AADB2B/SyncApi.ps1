$SyncAPI_AuthHeader = @{}
$SyncAPI_UriRoot = ""

function Init-SyncAPI
{
    param(
        [parameter(Position=0, Mandatory=$true)]
        [object]$SyncVars
    )
    $ApiKey = $SyncVars.ApiKey
    $ApiSite = $SyncVars.ApiSite
    $global:SyncAPI_UriRoot = "$APISite/api"
    $global:SyncAPI_AuthHeader = @{apikey = $ApiKey}
    $global:SiteConfig = Get-SiteConfig
    $global:RemoteSiteID = $SiteConfig.id
}

function Get-SiteConfig
{
    $Endpoint = "$SyncAPI_UriRoot/StagedUsers/GetSiteConfig"
    $api = Invoke-RestMethod -Uri $Endpoint -Method Get -Headers $SyncAPI_AuthHeader
    return $api
}

function Get-AllStaged
{
    $Endpoint = "$SyncAPI_UriRoot/StagedUsers/GetAllStaged"
    $api = Invoke-RestMethod -Uri $Endpoint -Method Get -Headers $SyncAPI_AuthHeader
    return $api
}

function Get-UpdatedStagedUsers
{
    $Endpoint = "$SyncAPI_UriRoot/StagedUsers/GetAllByStage?stage=1"
    $api = Invoke-WebRequest -Uri $Endpoint -Method GET -Headers $SyncAPI_AuthHeader
    $res = (ConvertFrom-Json -InputObject $api)
    return $res
}

function Set-UpdatedUsers
{
    param(
        [parameter(Position=0, Mandatory=$true)]
        [Object[]]$UpdateUserBatch
    )

    $Endpoint = "$SyncAPI_UriRoot/StagedUsers/UpdateBatch"

    $api = Invoke-WebRequest -Uri $Endpoint -Method Post -Headers $SyncAPI_AuthHeader -Body (ConvertTo-Json -InputObject $UpdateUserBatch) -ContentType "application/json"
    return $api
}

function Add-SyncLog
{
    param(
        [parameter(Position=0, Mandatory=$true)]
        [Object]$LogItem
    )

    $Endpoint = "$SyncAPI_UriRoot/SyncLogUpdate/AddLogEntry"

    $api = Invoke-WebRequest -Uri $Endpoint -Method Post -Headers $SyncAPI_AuthHeader -Body (ConvertTo-Json -InputObject $LogItem) -ContentType "application/json"
    return $null
}

function Add-SyncLogBatch
{
    param(
        [parameter(Position=0, Mandatory=$true)]
        [Object[]]$SyncLogBatch
    )

    $Endpoint = "$SyncAPI_UriRoot/SyncLogUpdate/AddBatchLogs"

    $api = Invoke-WebRequest -Uri $Endpoint -Method Post -Headers $SyncAPI_AuthHeader -Body (ConvertTo-Json -InputObject $SyncLogBatch) -ContentType "application/json"
    return $null
}
