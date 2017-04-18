<#
INITIALIZE
#>
    #include files
    . "$PSScriptRoot\SyncAPI.ps1"
    . "$PSScriptRoot\SyncTools.ps1"
    . "$PSScriptRoot\Logging.ps1"

    #Set API variables, initialize sync API
    $SyncVars = Get-Content -Path "$PSScriptRoot\SyncVars.json" | ConvertFrom-Json
    $ApiKey = $SyncVars.ApiKey #pocgenii
    $ApiSite = $SyncVars.ApiSite
    Init-SyncAPI -APISite $ApiSite -ApiKey $ApiKey

<#
PROCESS NEW USERS
#>
    #Load config and current users from cloud
    $SiteConfig = Get-SiteConfig
    $RemoteSiteID = $SiteConfig.id

    $StagedUsers = Get-AllStaged

    #filter gets all users that are not built-in administrators
    $DomUsers = Get-ADUsersToSync
    $newUsers = $false

    $stagedUserBatch = @()

    foreach($user in $DomUsers) {
        $adUser = $StagedUsers | where { $_.localGuid -eq $user.ObjectGUID }
        if ($adUser -eq $null) {

            $newUser = Add-NewStagedUser -User $user -SiteConfig $SiteConfig
            if ($newUser -eq $null) {
                #logged, continuing
                continue
            }

            $stagedUserBatch += $newUser
            $NewUsers = $true
        }
    }

    if (!$newUsers) {
        $msg = "No new users added"
        Create-LogEntry -ErrorType Info -Detail $msg -Source "Sync.Main.NewUsers" -RemoteSiteID $RemoteSiteID | Add-LogEntry

    } else {
        $NumUsers = $stagedUserBatch.Count
        $msg = "$NumUsers uploading..."
        Create-LogEntry -ErrorType Info -Detail $msg -Source "Sync.Main.NewUsers" -RemoteSiteID $RemoteSiteID | Add-LogEntry

        $newUserRes = Set-UpdatedUsers -UpdateUserBatch $StagedUserBatch

        $msg = "Upload API status: $($newUserRes.StatusCode) $($newUserRes.StatusDescription)"
        Create-LogEntry -ErrorType Info -Detail $msg -Source "Sync.Main.NewUsers" -RemoteSiteID $RemoteSiteID | Add-LogEntry

        $err =$($newUserRes.Headers["ErrorMessage"])
        if ($err.Length -gt 0) {
            $msg += "Upload Error: $err"
            Create-LogEntry -ErrorType Error -Detail $msg -Source "Sync.Main.NewUsers" -RemoteSiteID $RemoteSiteID | Add-LogEntry
        }
    }

<#
PROCESS HQ UPDATES
#>

$usersToUpdate = Get-UpdatedStagedUsers
$doUpd = $false
$batchError = $()
foreach($updUser in $usersToUpdate) 
{
    try {
        Set-ADUser -Identity $updUser.localGuid -Add @{ "mS-DS-ConsistencyGuid" = $updUser.masterGuid } -ErrorAction Stop
        $updUser.loadState=3
        $doUpd = $true
    } 
    catch [Exception] {
        $errMsg = $_.Exception.ToString()
        $batchError += Create-LogEntry -ErrorType Error -Detail $errMsg -Source "Sync.Main.HQUpdate" -RemoteSiteID $RemoteSiteID
    }
}

if (!$doUpd) {
    $msg = "No HQ updates processed"
    Create-LogEntry -ErrorType Info -Detail $msg -Source "Sync.Main.NewUsers" -RemoteSiteID $RemoteSiteID | Add-LogEntry
    
} else {
    $updUserRes = Set-UpdatedUsers -UpdateUserBatch $usersToUpdate

    $msg = "Upload API status: $($updUserRes.StatusCode) $($updUserRes.StatusDescription)"
    $batchError += Create-LogEntry -ErrorType Info -Detail $msg -Source "Sync.Main.NewUsers" -RemoteSiteID $RemoteSiteID

    $err =$($updUserRes.Headers["ErrorMessage"])
    if ($err.Length -gt 0) {
        $msg += "Upload Error: $err"
        $batchError += Create-LogEntry -ErrorType Error -Detail $msg -Source "Sync.Main.HQUpdate" -RemoteSiteID $RemoteSiteID
    }

}
if ($batchError.Count -gt 0) {
    Add-SyncLogBatch -SyncLogBatch $batchError
}

<#
PROCESS AD UPDATES BACK TO HQ (not implemented)
#>