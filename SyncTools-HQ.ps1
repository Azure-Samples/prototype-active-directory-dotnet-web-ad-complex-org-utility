function Process-PendingStagedUsers
{
    param(
        [parameter(Position=0, Mandatory=$true)]
        [ValidateSet("MasterHQ","AADB2B","LocalADOnly","All")]
        [string]$SiteType
    )

    #pull down pending staged users
    $StagedUsers = Get-NewStagedUsers -SiteType $SiteType

    if ($SiteType -eq "AADB2B") { $SearchBase = "ou=$B2BOU,$ForestRoot" }
    if ($SiteType -eq "LocalADOnly") { $SearchBase = "ou=$RemoteOU,$ForestRoot" }

    #filter gets all users from the remote OU
    $DomUsers = Get-ADUser -Filter * -SearchBase $SearchBase -Properties @("cn","mail","co","company","department","displayName","l","mobile","objectSid","st","streetAddress","telephoneNumber","homePhone","postalCode","title")
    $newUsers = $false
    $logs = New-Object System.Collections.ArrayList

    foreach($StagedUser in $StagedUsers) {
        $user = $DomUsers | where { $_.UserPrincipalName -eq $StagedUser.upn }

        if ($user -eq $null) {
            #create stub user on-prem
            $msg="Creating stub user $StagedUser.upn..."
            $e= Create-LogEntry -ErrorType Info -Detail $msg -Source "Sync.Process-PendingStagedUsers" -RemoteSiteID $RemoteSiteID -StagedUserID $StagedUser.id
            $logs.Add($e)

            try {
   	            $bytes = New-Object Byte[] 32
	            $rand = [System.Security.Cryptography.RandomNumberGenerator]::Create()
	            $rand.GetBytes($bytes)
	            $rand.Dispose()
	            $RandPassword = [System.Convert]::ToBase64String($bytes)
            
                $UserDomain = $StagedUser.domainName
                $UserSAM = $StagedUser.upn.replace('@','').replace('.','')
                if ($UserSAM.Length -gt 20) {
                    $UserSAM = $UserSAM.substring(0,20)
                }

                $UserOU = "OU=$RemoteOU,OU=$UserDomain,$ForestRoot"
                $CurrUserOU = $DomainList | where { $_.Name -eq $UserDomain }
                if ($CurrUserOU -eq $null) {
                    #sanity check - should not happen
                    $msg = "User {0} not matched to a remoteOU - please check the domain list" -f $StagedUser.UserPrincipalName
                    $e = Create-LogEntry -ErrorType Warning -Detail $msg -Source "Sync.Process-PendingStagedUsers" -RemoteSiteID $RemoteSiteID -StagedUserID $StagedUser.id
                    $logs.Add($e)
                }

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
                $e = Create-LogEntry -ErrorType Info -Detail $msg -Source "Sync.Process-PendingStagedUsers" -RemoteSiteID $RemoteSiteID -StagedUserID $StagedUser.id
                $logs.Add($e)

                $NewUsers = $true

                $userSite = $DomainsToSync | where { $_.siteDomain.Contains($StagedUser.domainName) }

                $userSiteType = Get-SiteType -SiteType $userSite.siteType

                if ($userSiteType -eq "AADB2B")
                {
                    $e = Send-B2BInvite -user $StagedUser -RedirectTo $userSite.b2bRedirectUrl
                    $logs.AddRange($e)
                }

            }
            catch [Exception] {
                $errMsg = $_.Exception.ToString()
                $msg = "Error creating user : $errMsg"
                $e = Create-LogEntry -ErrorType Error -Detail $msg -Source "Sync.Process-PendingStagedUsers" -RemoteSiteID $RemoteSiteID -StagedUserID $StagedUser.id
                $logs.Add($e)

            }
        }
    }

    if (!$newUsers) {
        $msg = "No new users created"
        $e = Create-LogEntry -ErrorType Info -Detail $msg -Source "Sync.Process-PendingStagedUsers" -RemoteSiteID $RemoteSiteID -StagedUserID $StagedUser.id
        $logs.Add($e)
    } else {
        $returnUsers = $StagedUsers | where { $_.masterGUID -ne $null } 
        $NumUsers = $returnUsers.Count

        $msg = "$NumUsers updating..."
        $e = Create-LogEntry -ErrorType Info -Detail $msg -Source "Sync.Process-PendingStagedUsers" -RemoteSiteID $RemoteSiteID -StagedUserID $StagedUser.id
        $logs.Add($e)

        $apiRes =Set-MasterGuid -UpdateUserBatch $returnUsers
        $msg = "Upload API status: $($apiRes.StatusCode) $($apiRes.StatusDescription)"
        $e = Create-LogEntry -ErrorType Info -Detail $msg -Source "Sync.Process-PendingStagedUsers" -RemoteSiteID $RemoteSiteID -StagedUserID $StagedUser.id
        $logs.Add($e)

        $err =$($apiRes.Headers["ErrorMessage"])

        if ($err.Length -gt 0) {
            $e = Create-LogEntry -ErrorType Error -Detail $err -Source "Sync.Process-PendingStagedUsers" -RemoteSiteID $RemoteSiteID -StagedUserID $StagedUser.id
            $logs.Add($e)
        }
    }

    Add-SyncLogBatch -SyncLogBatch $logs
}

function Init-HQ
{
    $ADDomain = Get-ADDomain
    $global:ForestRoot = $ADDomain.DistinguishedName
    $global:RemoteOU="RemoteUsers-AD"
    $global:B2BOU = "RemoteUsers-B2B"

    $global:DomainsToSync = Get-DomainsToSync

    #ensure OUs are established
    $global:domainList = @()
    foreach($site in $DomainsToSync)
    {
        switch($site.siteType)
        {
            1 { $DomOU = $B2BOU }
            2 { $DomOU = $RemoteOU }
        }

        foreach($domain in $site.siteDomain) {
            $SiteOU = "OU=$domain,OU=$DomOU,$ForestRoot"
            $ADSiteOU = Get-ADOrganizationalUnit -Filter "DistinguishedName -eq `"$SiteOU`""
            if ($ADSiteOU -eq $null) {
                $ADSiteOU = New-ADOrganizationalUnit -Name $domain -Path "ou=$DomOU,$ForestRoot"
            }
           $global:domainList += $ADSiteOU
        }
    }
}

function Send-B2BInvite 
{
    param(
        [object]$user,
        [string]$RedirectTo
    )
    $llogs = New-Object System.Collections.ArrayList
    $msg="Creating new B2B Invitation for user $($user.UserPrincipalName)..."
    $e = Create-LogEntry -ErrorType Info -Detail $msg -Source "Send-B2BInvite" -RemoteSiteID $RemoteSiteID
    $llogs.Add($e)

    if ($RedirectTo -eq $null) {
        $RedirectTo = $DefaultRedirectTo
    }
    try {
        $invitation = Send-B2BInviteGraph -Email $user.mail -DisplayName $user.displayName -GraphToken $GraphAccessToken -RedirectTo $RedirectTo -UserType Member
        $msg = ($invitation | Format-List | Out-String)
        $e = Create-LogEntry -ErrorType Info -Detail $msg -Source "Send-B2BInvite" -RemoteSiteID $RemoteSiteID
        $llogs.Add($e)
    }
    catch [Exception] {
        $errMsg = $_.Exception.ToString()
        $msg = "Error adding user : $errMsg"
        $e = Create-LogEntry -ErrorType Error -Detail $msg -Source "Send-B2BInvite" -RemoteSiteID $RemoteSiteID
        $llogs.Add($e)
    }
    return $llogs
}

function Get-LoadStage
{
    param(
        [int]$LoadStage
    )

    $res=""

    switch($LoadStage)
        {
            0 { $res = "PendingHQAdd" }
            1 { $res = "PendingRemoteUpdate" }
            2 { $res = "PendingHQUpdate" }
            3 { $res = "NothingPending" }
            4 { $res = "PendingHQDelete" }
            5 { $res = "Deleted" }
            6 { $res = "NewNothingPending" }
        }
    return $res
}

$SiteTypes = @{
    "MasterHQ"    = 0;
    "AADB2B"      = 1;
    "LocalADOnly" = 2;
}
$LoadStages = @{
    "PendingHQAdd"        = 0;
    "PendingRemoteUpdate" = 1;
    "PendingHQUpdate"     = 2;
    "NothingPending"      = 3;
    "PendingHQDelete"     = 4;
    "Deleted"             = 5;
    "NewNothingPending"   = 6;
}
