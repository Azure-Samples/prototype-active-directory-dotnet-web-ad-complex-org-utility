#https://developer.microsoft.com/en-us/graph/docs/api-reference/v1.0/resources/invitation

#include files
. "$PSScriptRoot\GraphLogin.ps1"
. "$PSScriptRoot\AADSyncLogin.ps1"
. "$PSScriptRoot\Send-B2BInviteGraph.ps1"

$stamp=(Get-Date).toString("yyyy-MM-dd-HH-mm-ss")
$logFile = "$PSScriptRoot\Logs\$stamp.log"
$RedirectTo = "https://b2bmultidom-webuocf7lvgfqppw.azurewebsites.net/profile"

$ADUsers = Get-AzureADUser

#filter gets all users that are not built-in administrators
$DomUsers = Get-ADUser -Filter '-not adminCount -like "*" -and Enabled -eq "True"' -Properties @("cn","mail","co","company","department","displayName","l","mobile","objectSid","st","streetAddress","telephoneNumber","homePhone","postalCode","title")
$newUsers = $false

foreach($user in $DomUsers) {
    $adUser = $ADUsers | where { $_.Mail -eq $user.mail }

    if ($adUser -eq $null) {
        #create B2B user in cloud
        $msg="Creating new B2B Invitation for user $user.UserPrincipalName..."
        Write-Host $msg
        Add-Content $logFile -Value $msg

        try {
            $invitation = Send-B2BInviteGraph -Email $user.mail -DisplayName $user.DisplayName -GraphToken $GraphAccessToken -RedirectTo $RedirectTo -UserType Member
            $msg = ($invitation | Format-List | Out-String)
            Add-Content $logFile -Value $msg

            $NewUsers = $true
        }
        catch [Exception] {
            $errMsg = $_.Exception.ToString()
            $msg = "Error adding user : $errMsg"
            Write-Error $msg
            Add-Content $logFile -Value $msg

        }
    }
}

if (!$newUsers) {
    $msg = "No new users invited"
    Write-Host $msg
    Add-Content $logFile -Value $msg
}