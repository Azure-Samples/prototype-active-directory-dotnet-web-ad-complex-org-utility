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

#NOTE: this returns all, filtered by THIS site
function Get-AllStaged
{
    $Endpoint = "$SyncAPI_UriRoot/StagedUsers/GetAllStaged"
    $api = Invoke-RestMethod -Uri $Endpoint -Method Get -Headers $SyncAPI_AuthHeader
    return $api
}

function Get-DomainsToSync
{
    $Endpoint = "$SyncAPI_UriRoot/StagedUsersAdm/GetRemoteSiteList"
    $api = Invoke-WebRequest -Uri $Endpoint -Method GET -Headers $SyncAPI_AuthHeader
    $res = (ConvertFrom-Json -InputObject $api)
    return $res
}

function Get-NewStagedUsers
{
    param(
        [parameter(Position=0, Mandatory=$true)]
        [ValidateSet("MasterHQ","AADB2B","LocalADOnly","All")]
        [string]$SiteType
    )

    $Type = Get-SiteType -SiteType $SiteType
    $Endpoint = "$SyncAPI_UriRoot/StagedUsersAdm/GetAllByStageAndSiteType?stage=0&type=$Type"

    if ($Type -eq $null)
    {
        #user chose "All"
        $Endpoint = "$SyncAPI_UriRoot/StagedUsersAdm/GetAllByStage?stage=0"
    }

    $api = Invoke-WebRequest -Uri $Endpoint -Method GET -Headers $SyncAPI_AuthHeader
    $res = (ConvertFrom-Json -InputObject $api)
    return $res
}

function Get-StagedUsersPendingUpdate
{
    $Endpoint = "$SyncAPI_UriRoot/StagedUsersAdm/GetAllByStage?stage=2"
    $api = Invoke-WebRequest -Uri $Endpoint -Method GET -Headers $SyncAPI_AuthHeader
    $res = (ConvertFrom-Json -InputObject $api)
    return $res
}
function Get-StagedUsersPendingDelete
{
    $Endpoint = "$SyncAPI_UriRoot/StagedUsersAdm/GetAllByStage?stage=4"
    $api = Invoke-WebRequest -Uri $Endpoint -Method GET -Headers $SyncAPI_AuthHeader
    $res = (ConvertFrom-Json -InputObject $api)
    return $res
}

function Set-MasterGuid
{
    param(
        [Object[]]$UpdateUserBatch
    )

    $Endpoint = "$SyncAPI_UriRoot/StagedUsersAdm/UpdateBatchAdmin"

    #loadStage 1 = "PendingRemoteUpdate"
    $UpdateUserBatch | foreach { $_.loadState = 1 }

    $api = Invoke-WebRequest -Uri $Endpoint -Method Post -Headers $SyncAPI_AuthHeader -Body (ConvertTo-Json -InputObject $UpdateUserBatch) -ContentType "application/json"
    return $api
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
