function Add-NewStagedUser
{
    param(
        [object]$User,
        [object]$SiteConfig
    )
    <#
    --stage user in cloud--
    HQ will pick up user and create in HQ AD
    User will then be sync'd to HQ Azure AD
    User will be authenticated via federation to this ADFS
    #>
    $dom = $user.UserPrincipalName.Split('@')[1]

    if ($user.UserPrincipalName.Length -eq 0) { 
        $msg = "User $($user.DistinguishedName) missing UPN: not added."
        Create-LogEntry -ErrorType Error -Detail $msg -Source "Script:Add-NewStagedUser" -RemoteSiteID $RemoteSiteID | Add-LogEntry | Out-Null
        return $null
    }

    if ($dom.indexof("onmicrosoft.com") -gt -1) {
        $msg = "Not adding {0}, excluding '*.onmicrosoft.com' UPNs" -f $user.UserPrincipalName
        Create-LogEntry -ErrorType Warning -Detail $msg -Source "Script:Add-NewStagedUser" -RemoteSiteID $RemoteSiteID | Add-LogEntry | Out-Null
        return $null
    }

    if (!$siteConfig.SiteDomain.Contains($dom)) {
        $msg = "Not adding {0}, domain not listed in site configuration" -f $user.UserPrincipalName
        Create-LogEntry -ErrorType Warning -Detail $msg -Source "Script:Add-NewStagedUser" -RemoteSiteID $RemoteSiteID | Add-LogEntry | Out-Null
        return $null
    }

    $msg="Staging new AD user $($user.UserPrincipalName)..."
    Create-LogEntry -ErrorType Info -Detail $msg -Source "Script:Add-NewStagedUser" -RemoteSiteID $RemoteSiteID | Add-LogEntry | Out-Null

    $loadState = 0
    $masterGuid = $null
    $siteType = Get-SiteType -SiteType $SiteConfig.siteType
    if ($siteType -eq "MasterHQ") { 
        $masterGuid = $user.ObjectGUID
        $loadstate = 6
    }

    try {
        $res = @{}
        $res.loadState = $loadState
        $res.localGuid = $user.ObjectGUID
        $res.domainName = $dom
        $res.siteType = $SiteConfig.siteType
        $res.masterGuid = $masterGuid
        $res.department = $user.Department
        $res.mobile = $user.Mobile
        $res.title = $user.Title
        $res.telephoneNumber = $user.telephoneNumber
        $res.homePhone = $user.HomePhone
        $res.postalCode = $user.PostalCode
        $res.mail = $user.mail
        $res.surname = $user.Surname
        $res.givenName = $user.GivenName
        $res.displayName = $user.DisplayName
        $res.name = $user.name
        $res.streetAddress = $user.StreetAddress
        $res.city = $user.l
        $res.state = $user.st
        $res.country = $user.co
        $res.upn = $user.UserPrincipalName

        return $res
    }
    catch [Exception] {
        $errMsg = $_.Exception.ToString()
        $msg = "Error adding user : $errMsg"
        Create-LogEntry -ErrorType Error -Detail $msg -Source "Script:Add-NewStagedUser" -RemoteSiteID $RemoteSiteID | Add-LogEntry | Out-Null
        return $null
    }
}

function Get-ADUsersToSync
{
    $UpnSuffix = $SiteConfig.siteDomain
    $userProps = @("cn","mail","co","name","company","department","displayName","l","mobile","objectSid","st","streetAddress","telephoneNumber","homePhone","postalCode","title")
    $userFilter = '-not adminCount -like "*" -and Enabled -eq "True" -and UserPrincipalName -like "*@' + $UpnSuffix + '"'
    $res = Get-ADUser -Filter $userFilter -Properties $userProps
    return $res
}

function Process-UpdatesFromHQ
{
    $usersToUpdate = Get-UpdatedStagedUsers
    $doUpd = $false
    $batchError = $()
    foreach($updUser in $usersToUpdate) 
    {
        try {
            $consistencyguid = ([System.Guid]"{$($updUser.masterGuid)}").ToByteArray()
            Set-ADUser -Identity $updUser.localGuid -Add @{ "mS-DS-ConsistencyGuid" = $consistencyguid } -ErrorAction Stop
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
}

function Process-NewUsers
{
    #Load config and current users from cloud
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
}

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

                $userSite = $DomainsToSync | where { $_.siteDomain.Contains($StagedUser.domainName) }

                $userSiteType = Get-SiteType -SiteType $userSite.siteType

                if ($userSiteType -eq "AADB2B")
                {
                    Send-B2BInvite -user $StagedUser -RedirectTo $userSite.b2bRedirectUrl
                }

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
    $msg="Creating new B2B Invitation for user $($user.UserPrincipalName)..."
    Create-LogEntry -ErrorType Info -Detail $msg -Source "Send-B2BInvite" -RemoteSiteID $RemoteSiteID | Add-LogEntry | Out-Null

    if ($RedirectTo -eq $null) {
        $RedirectTo = $DefaultRedirectTo
    }
    try {
        $invitation = Send-B2BInviteGraph -Email $user.mail -DisplayName $user.displayName -GraphToken $GraphAccessToken -RedirectTo $RedirectTo -UserType Member
        $msg = ($invitation | Format-List | Out-String)
        Create-LogEntry -ErrorType Info -Detail $msg -Source "Send-B2BInvite" -RemoteSiteID $RemoteSiteID | Add-LogEntry | Out-Null
    }
    catch [Exception] {
        $errMsg = $_.Exception.ToString()
        $msg = "Error adding user : $errMsg"
        Create-LogEntry -ErrorType Error -Detail $msg -Source "Send-B2BInvite" -RemoteSiteID $RemoteSiteID | Add-LogEntry | Out-Null
    }
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

function Get-SiteType
{
    param(
        $SiteType
    )

    $res=""
    [int]$type | Out-Null
   
    if ([int]::TryParse($SiteType, [ref][int]$type)){
        switch($SiteType)
        {
            0 { $res = "MasterHQ" }
            1 { $res = "AADB2B" }
            2 { $res = "LocalADOnly" }
            default { $res = $null }
        }
    } else {
        switch($SiteType)
        {
            "MasterHQ" { $res = 0 }
            "AADB2B" { $res = 1 }
            "LocalADOnly" { $res = 2 }
            default { $res = $null }
        }
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
