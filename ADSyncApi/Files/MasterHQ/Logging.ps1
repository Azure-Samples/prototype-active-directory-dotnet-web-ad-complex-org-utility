$stamp=(Get-Date).toString("yyyy-MM-dd-HH-mm-ss")
$logFile = "$PSScriptRoot\Logs\$stamp.log"

Function _addLogEntry {
    param(
        [parameter(Position=0, Mandatory=$true)]
        [Object]
        $SyncLogItem
    )

    _writeHost $SyncLogItem
    Add-Content $logFile -Value $SyncLogItem.Detail | Out-Null

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

Function Add-BatchLogEntries {
    param(
        [parameter(Position=0, Mandatory=$true)]
        [System.Collections.ArrayList]$SyncLogBatch
    )

    Add-SyncLogBatch -SyncLogBatch $SyncLogBatch | Out-Null

    $count = $SyncLogBatch.Count
    $errors = $SyncLogBatch | where { $_.ErrorType -eq "Error" }
    $errCount = $errors.Count
    $warnCount = ($SyncLogBatch | where { $_.ErrorType -eq "Warning" }).Count
    $msg = "$count log entry(s) transmitted, including $errCount error(s) and $warnCount warning(s)."

    if ($errCount -gt 0) {
        Write-Error $msg
    }
    elseif ($warnCount -gt 0) {
        Write-Warning $msg
    }
    else {
        Write-Host $msg
    }

    Add-Content $logFile -Value $msg
    for($x=0; $x -lt $errors.Count; $x++) {
        Add-Content $logFile -Value "    $($errors[$x].Detail)" | Out-Null
    }

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

    $newLog=@{
        "ErrorType" = $ErrorType;
        "Detail" = $Detail;
        "RemoteSiteID" = $RemoteSiteID;
        "Source" = $Source;
        "StagedUserID" = $StagedUserID;
    }

    return $newLog
}
