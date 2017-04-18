#https://blogs.msdn.microsoft.com/aadgraphteam/2016/07/08/microsoft-graph-or-azure-ad-graph/
#https://msdn.microsoft.com/en-us/Library/Azure/Ad/Graph/howto/azure-ad-graph-api-differential-query
#https://developer.microsoft.com/en-us/graph/docs/concepts/delta_query_overview
#https://developer.microsoft.com/en-us/graph/docs/concepts/delta_query_users

#include files
. "$PSScriptRoot\SyncAPI.ps1"

#Set API variables, initialize sync API
$ApiKey = "wI6bJrk/S6kKtLwu8N/8Vs11JLDjxsbUta46k89crCg="
$ApiSite = "https://adsyncapi20170413061307.azurewebsites.net"
$ForestRoot="DC=multidompoc,DC=net"

Init-SyncAPI -APISite $ApiSite -ApiKey $ApiKey

$RemoteOU="RemoteUsers-AD"
$B2BOU = "RemnoteUsers-B2B"

$SearchBase="ou=$RemoteOU,$ForestRoot"

$DomainsToSync = Get-DomainsToSync

$domainList = @()
foreach($site in $DomainsToSync)
{
    foreach($domain in $site.siteDomain) {
        $SiteOU = "OU=$domain,OU=$RemoteOU,$ForestRoot"
        $ADSiteOU = Get-ADOrganizationalUnit -Filter "DistinguishedName -eq `"$SiteOU`""
        if ($ADSiteOU -eq $null) {
            $ADSiteOU = New-ADOrganizationalUnit -Name $domain -Path $SearchBase
        }
       $domainList += $ADSiteOU
    }
}

$stamp=(Get-Date).toString("yyyy-MM-dd-HH-mm-ss")
$logFile = "$PSScriptRoot\Logs\$stamp.log"
$logs = @()

$StagedUsers = Get-NewStagedUsers

#filter gets all users from the remote OU
$DomUsers = Get-ADUser -Filter * -SearchBase $SearchBase -Properties @("cn","mail","co","company","department","displayName","l","mobile","objectSid","st","streetAddress","telephoneNumber","homePhone","postalCode","title")
$newUsers = $false

foreach($StagedUser in $StagedUsers) {
    $user = $DomUsers | where { $_.UserPrincipalName -eq $StagedUser.upn }

    if ($user -eq $null) {
        #create stub user on-prem
        $msg="Creating stub user $StagedUser.upn..."
        $logs += $msg

        try {
   	        $bytes = New-Object Byte[] 32
	        $rand = [System.Security.Cryptography.RandomNumberGenerator]::Create()
	        $rand.GetBytes($bytes)
	        $rand.Dispose()
	        $RandPassword = [System.Convert]::ToBase64String($bytes)
            
            $UserDomain = $StagedUser.domainName
            $UserSAM = $StagedUser.upn.replace('@','').replace('.','').substring(0,20)

            $UserOU = "OU=$RemoteOU,OU=$UserDomain,$ForestRoot"
            $CurrUserOU = $DomainList | where { $_.Name -eq $UserDomain }
            if ($CurrUserOU -eq $null) {
                #sanity check - should not happen
                $msg = "User {0} not matched to a remoteOU - please check the domain list" -f $StagedUser.UserPrincipalName
                $logs += $msg
            }
            #New-ADUser -Name "New 11" -ChangePasswordAtLogon $false -SmartcardLogonRequired $true -UserPrincipalName my.smart.user@microsoft.com 
            #Enable-ADAccount -Identity "New 11" 

            $NewDOMUser = New-ADUser `
                -Name ([string]$StagedUser.Name) `
                -DisplayName ([string]$StagedUser.DisplayName) `
                -UserPrincipalName $StagedUser.upn `
                -SamAccountName $UserSAM `
                -City ([string]$StagedUser.city) `
                -Department ([string]$StagedUser.department) `
                -MobilePhone ([string]$StagedUser.mobile) `
                -OfficePhone ([string]$stagedUser.telephoneNumber) `
                -HomePhone ([string]$stagedUser.homePhone) `
                -PostalCode ([string]$stagedUser.postalCode) `
                -State ([string]$StagedUser.state) `
                -StreetAddress ([string]$stagedUser.streetAddress) `
                -Title ([string]$stagedUser.title) `
                -GivenName ([string]$StagedUser.givenName) `
                -Surname ([string]$StagedUser.surname) `
                -AccountPassword (ConvertTo-SecureString $RandPassword -AsPlainText -Force) `
                -Path $CurrUserOU.DistinguishedName `
                -ChangePasswordAtLogon $false `
                -Enabled $true `
                –PasswordNeverExpires $true `
                -SmartcardLogonRequired $true `
                -EmailAddress ([string]$StagedUser.mail) `
                -PassThru

            #Enable-ADAccount - -Identity $StagedUser.Name
            $StagedUser.masterGuid = $NewDomUser.ObjectGUID.ToString()

            $msg="Stub user $StagedUser.upn created and enabled"
            $logs += $msg

            $NewUsers = $true
        }
        catch [Exception] {
            $errMsg = $_.Exception.ToString()
            $msg = "Error creating user : $errMsg"
            $logs += $msg

        }
    }
}

if (!$newUsers) {
    $msg = "No new users created"
    $logs += $msg
} else {
    $returnUsers = $StagedUsers | where { $_.masterGUID -ne $null } 
    $NumUsers = $returnUsers.Count
    $logs += "$NumUsers updating..."
    $apiRes =Set-MasterGuid -UpdateUserBatch $returnUsers
    $logs += "Upload API status: $($apiRes.StatusCode) $($apiRes.StatusDescription)"
    $err =$($apiRes.Headers["ErrorMessage"])
    $err

    if ($err.Length -gt 0) {
        $logs += "Error: $err"
    }

}

$logs | Format-List | Add-Content $logFile
$logs | Format-List
