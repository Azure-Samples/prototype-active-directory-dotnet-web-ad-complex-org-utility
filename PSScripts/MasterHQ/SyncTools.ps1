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

    if ($user.UserPrincipalName.Length -eq 0) { 
        $msg = "User $($user.DistinguishedName) missing UPN: not added."
        Create-LogEntry -ErrorType Error -Detail $msg -Source "Script:Add-NewStagedUser" -RemoteSiteID $RemoteSiteID | Add-LogEntry | Out-Null
        return $null
    }

    $dom = $user.UserPrincipalName.Split('@')[1]

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
        $res.siteId = $SiteConfig.id
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
    $userProps = @("cn","mail","co","name","company","department","displayName","l","mobile","objectSid","st","streetAddress","telephoneNumber","homePhone","postalCode","title")
    $userFilter = '-not adminCount -like "*" -and Enabled -eq "True"'
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
            $currUser = Get-ADUser -Identity $updUser.localGuid -Properties @("mS-DS-ConsistencyGuid")
            $consistencyguid = ([System.Guid]"{$($updUser.masterGuid)}").ToByteArray()

            if (!$consistencyguid.Equals($currUser.'mS-DS-ConsistencyGuid')) {
                if ($currUser.'mS-DS-ConsistencyGuid' -ne $null) {
                    $orgGuid = [System.Guid]::new($currUser.'ms-ds-consistencyguid').ToString()

                    $errMsg = "User $($currUser.name) has a consistency GUID that is set but doesn't match the one from HQ. It will be cleared and the new one set."
                    $errMsg += "Original GUID: $orgGuid New GUID: $updUser.masterGuid"
                    $batchError += Create-LogEntry -ErrorType Warning -Detail $errMsg -Source "Sync.Main.HQUpdate" -RemoteSiteID $RemoteSiteID

                    #clearing existing GUID
                    Set-ADUser -Identity $updUser.localGuid -Replace @{ "mS-DS-ConsistencyGuid" = $consistencyguid } -ErrorAction Stop | Out-Null
                } else {
                    Set-ADUser -Identity $updUser.localGuid -Add @{ "mS-DS-ConsistencyGuid" = $consistencyguid } -ErrorAction Stop | Out-Null
                }
            }

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
