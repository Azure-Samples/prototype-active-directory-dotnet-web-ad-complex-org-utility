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

    $global:SyncAPI_UriRoot = "$($APISite)api"
    $global:SyncAPI_AuthHeader = @{apikey = $ApiKey}
    $global:SiteConfig = Get-SiteConfig
    $global:RemoteSiteID = $SiteConfig.id
}

function Check-ServiceUpdate
{
    $verFile = "$PSScriptRoot\ScriptVersion.txt"
    $currVersion = Get-Content $verFile -ErrorAction Ignore | Out-String

    $Endpoint = "$global:SyncAPI_UriRoot/Files/GetSiteServiceVersion"
    $res = Invoke-WebRequest -Uri $Endpoint -Method Get -Headers $global:SyncAPI_AuthHeader -ErrorAction Stop
    $newVersion = ConvertFrom-Json -InputObject $res.Content
    if ([long]$currVersion -eq [long]$newVersion) {
        Write-Host "No updates required"
        return
    }
    Write-Host "Updating service files..."
    Create-LogEntry -ErrorType Warning -Detail "Updating service files from $currVersion to $newVersion" -Source "Sync.Apply-ServiceUpdate" -RemoteSiteID $RemoteSiteID | Add-LogEntry

    Apply-ServiceUpdate
}

function Apply-ServiceUpdate
{
    $tempfile = [System.IO.Path]::GetTempFileName()
    $folder = [System.IO.Path]::GetDirectoryName($tempfile)
    (Get-Command Apply-ServiceUpdateMethod).Definition | Out-File $tempfile

    $ScriptPath = $folder + "\ApplyServiceUpdate.ps1"
	Remove-Item -Path $ScriptPath -ErrorAction Ignore
    Rename-Item -Path $tempfile -NewName "ApplyServiceUpdate.ps1"
    $ScriptPath = "$folder\ApplyServiceUpdate.ps1"
    &$ScriptPath
    exit
}

function Apply-ServiceUpdateMethod
{
    Start-Sleep -Seconds 1

    try {
        $service = get-wmiobject -query 'select * from win32_service where name="ComplexOrgSiteAgent"';
        if ($service -eq $null) {
            throw [System.Exception] "Service not installed"
        }

        $servicePath = $Service.PathName.Trim('"', ' ')
        $dir = [System.IO.Path]::GetDirectoryName($servicePath)
        $currFolder = [System.IO.DirectoryInfo]::new($dir)
       
        $SyncVars = Get-Content -Path "$dir\Scripts\SyncVars.json" | ConvertFrom-Json

        $tfile = [System.IO.Path]::GetTempFileName()
        $tfolder = [System.IO.Path]::GetDirectoryName($tfile)
        Copy-Item -Path "$dir\Scripts\SyncAPI.ps1" -Destination "$tfolder\SyncAPI.ps1"
        Copy-Item -Path "$dir\Scripts\SyncTools.ps1" -Destination "$tfolder\SyncTools.ps1"
        Copy-Item -Path "$dir\Scripts\Logging.ps1" -Destination "$tfolder\Logging.ps1"
        md "$tfolder\logs" -ErrorAction Ignore

        . "$tfolder\SyncAPI.ps1"
        . "$tfolder\SyncTools.ps1"
        . "$tfolder\Logging.ps1"

        Init-SyncAPI -SyncVars $SyncVars

        #getting fresh ZIP
        $Endpoint = "$SyncAPI_UriRoot/Files/GetSiteServiceZip"

        Add-Type -assembly "system.io.compression.filesystem" -ErrorAction Stop

        $currFolderName = $currFolder.Name
        $parentFolder = $currFolder.Parent.FullName

        #stopping existing service
        $service.StopService()
        Start-Sleep -Seconds 10
		if ((Get-Service ComplexOrgSiteAgent).Status -ne "Stopped") {
			Stop-process -name ComplexOrgSiteAgent -Force
		}
    
        #backing up existing service
        $backupFile = "$parentFolder\$currFolderName.backup.zip"
        Remove-Item -Path $backupFile -ErrorAction Ignore

        [System.IO.Compression.ZipFile]::CreateFromDirectory($currFolder, $backupFile)
        Remove-Item -Path "$($CurrFolder.FullName)\*" -Recurse -Force

        #copying new service
        $webclient = New-Object System.Net.WebClient
        $webclient.Headers.Add("apikey", $SyncAPI_AuthHeader.apikey)

        $ZipPath = "$parentFolder\ComplexOrgUtilServiceUpdate.zip"
        Remove-Item -Path $ZipPath -ErrorAction Ignore

        $webclient.DownloadFile($Endpoint, $ZipPath)
        [System.IO.Compression.ZipFile]::ExtractToDirectory($ZipPath, $currFolder.FullName)

        #starting service
        $service.StartService()

	    $verFile = "$dir\Scripts\ScriptVersion.txt"
	    $currVersion = Get-Content $verFile -ErrorAction Ignore | Out-String

        Create-LogEntry -ErrorType Info -Detail "Service updated to $currVersion" -Source "Sync.Apply-ServiceUpdate" -RemoteSiteID $RemoteSiteID | Add-LogEntry
		Write-EventLog -LogName 'Azure AD Complex Org Site Agent-console' -Source 'Azure AD Complex Org Site Agent-console' -EntryType Information -EventId 100 -Message "Service updated to $currVersion"
    }
    catch [Exception] {
        $errMsg = $_.Exception.ToString()
        Create-LogEntry -ErrorType Error -Detail $errMsg -Source "Sync.Apply-ServiceUpdate" -RemoteSiteID $RemoteSiteID | Add-LogEntry
    }
}

function Get-SiteConfig
{
    $Endpoint = "$global:SyncAPI_UriRoot/StagedUsers/GetSiteConfig"
    $api = Invoke-RestMethod -Uri $Endpoint -Method Get -Headers $global:SyncAPI_AuthHeader
    return $api
}

function Get-AllStaged
{
    $Endpoint = "$global:SyncAPI_UriRoot/StagedUsers/GetAllStaged"
    $api = Invoke-RestMethod -Uri $Endpoint -Method Get -Headers $global:SyncAPI_AuthHeader
    return $api
}

function Get-UpdatedStagedUsers
{
    $Endpoint = "$global:SyncAPI_UriRoot/StagedUsers/GetAllByStage?stage=1"
    $api = Invoke-WebRequest -Uri $Endpoint -Method GET -Headers $global:SyncAPI_AuthHeader
    $res = (ConvertFrom-Json -InputObject $api)
    return $res
}

function Set-UpdatedUsers
{
    param(
        [parameter(Position=0, Mandatory=$true)]
        [Object[]]$UpdateUserBatch
    )

    $Endpoint = "$global:SyncAPI_UriRoot/StagedUsers/UpdateBatch"

    $api = Invoke-WebRequest -Uri $Endpoint -Method Post -Headers $global:SyncAPI_AuthHeader -Body (ConvertTo-Json -InputObject $UpdateUserBatch) -ContentType "application/json"
    return $api
}

function Add-SyncLog
{
    param(
        [parameter(Position=0, Mandatory=$true)]
        [Object]$LogItem
    )

    $Endpoint = "$global:SyncAPI_UriRoot/SyncLogUpdate/AddLogEntry"

    $api = Invoke-WebRequest -Uri $Endpoint -Method Post -Headers $global:SyncAPI_AuthHeader -Body (ConvertTo-Json -InputObject $LogItem) -ContentType "application/json"
    return $null
}

function Add-SyncLogBatch
{
    param(
        [parameter(Position=0, Mandatory=$true)]
        [Object[]]$SyncLogBatch
    )

    $Endpoint = "$global:SyncAPI_UriRoot/SyncLogUpdate/AddBatchLogs"

    $api = Invoke-WebRequest -Uri $Endpoint -Method Post -Headers $global:SyncAPI_AuthHeader -Body (ConvertTo-Json -InputObject $SyncLogBatch) -ContentType "application/json"
    return $null
}
