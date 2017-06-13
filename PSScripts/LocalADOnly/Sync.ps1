<#
Validate
#>
if (($PSVersionTable.PSVersion).Major -lt 5) {
    Write-Error "Powershell version must be 5 or greater - please upgrade"
    exit
}

<#
INITIALIZE
#>
    #Set API variables, initialize sync API
    $SyncVars = Get-Content -Path "$PSScriptRoot\SyncVars.json" | ConvertFrom-Json

    #include files
    . "$PSScriptRoot\SyncAPI.ps1"
    . "$PSScriptRoot\SyncTools.ps1"
    . "$PSScriptRoot\Logging.ps1"

    Init-SyncAPI -SyncVars $SyncVars

<#
PROCESS NEW USERS
#>
    Process-NewUsers


<#
PROCESS HQ UPDATES
#>
    Process-UpdatesFromHQ


<#
PROCESS AD UPDATES BACK TO HQ (not implemented)
#>
    #Process-UpdatesToHQ