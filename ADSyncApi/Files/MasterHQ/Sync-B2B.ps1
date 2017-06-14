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
    . "$PSScriptRoot\SyncAPI-HQ.ps1"
    . "$PSScriptRoot\SyncTools.ps1"
    . "$PSScriptRoot\SyncTools-HQ.ps1"
    . "$PSScriptRoot\Logging.ps1"
    . "$PSScriptRoot\GraphLogin.ps1"
    . "$PSScriptRoot\Send-B2BInviteGraph.ps1"

    Init-SyncAPI -SyncVars $SyncVars
    Init-GraphAPI -SyncVars $SyncVars

    #The "RedirectTo" link will be sent along with the email invitation - 
    #   this is where the user is taken after redeeming their invitation
    #   Each site may have its own configured, this will be used if 
    #   a site's redirect is null
    $DefaultRedirectTo = "https://b2bmultidom-webuocf7lvgfqppw.azurewebsites.net/profile"
   
<#
INIT HQ
#>
    Init-HQ
    
<#
PROCESS PENDING STAGED USERS
#>
    Process-PendingStagedUsers -SiteType AADB2B

