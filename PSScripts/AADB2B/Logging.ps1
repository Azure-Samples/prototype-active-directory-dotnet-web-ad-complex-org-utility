$stamp=(Get-Date).toString("yyyy-MM-dd-HH-mm-ss")
$logFile = "$PSScriptRoot\Logs\$stamp.log"

Function _addLogEntry {
    param(
        [parameter(Position=0, Mandatory=$true)]
        [Object]
        $SyncLogItem
    )

    _writeHost $SyncLogItem
    Add-Content $logFile -Value $SyncLogItem.Detail

    Add-SyncLog -LogItem $SyncLogItem -OutVariable $null
    return $null
}

Function _writeHost {
    param(
        [parameter(Position=0, Mandatory=$true)]
        [Object]
        $ActivityLogItem
    )
    switch($ActivityLogItem.Status) 
    { 
        {($_ -eq "Error")} { Write-Error $ActivityLogItem.Detail } 
        {($_ -eq "Info")} { Write-Information $ActivityLogItem.Detail } 
        {($_ -eq "Warning")} { Write-Warning $ActivityLogItem.Detail } 

        default { Write-Host $ActivityLogItem.Detail } 
    }
    return $null
}

Function Add-LogEntry {
    param(
        [parameter(Position=0, Mandatory=$true, ValueFromPipeline=$true)]
        [Object]$LogEntry

    )

    _addLogEntry -SyncLogItem $LogEntry
    return $null
}

Function Create-LogEntry {
    param(
        [parameter(Position=0, Mandatory=$true)]
        [ValidateSet('Success','Error','Info','Warning')]
        [string]$ErrorType,

        [parameter(Position=1, Mandatory=$true)]
        [string]$Detail,

        [parameter(Position=2, Mandatory=$true)]
        [string]$Source,

        [parameter(Position=3, Mandatory=$true)]
        [string]$RemoteSiteID,

        [parameter(Position=4)]
        [string]$StagedUserID
    )

    $newLog=@{}
    $newLog.ErrorType = $ErrorType
    $newLog.Detail = $Detail
    $newLog.RemoteSiteID = $RemoteSiteID
    $newLog.Source = $Source
    $newLog.StagedUserID = $StagedUserID

    return $newLog
}
