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
        Create-LogEntry -ErrorType Error -Detail $msg -Source "Script:Add-NewStagedUser" -RemoteSiteID $RemoteSiteID | Add-LogEntry
        return $null
    }

    if ($dom.indexof("onmicrosoft.com") -gt -1) {
        $msg = "Not adding {0}, excluding '*.onmicrosoft.com' UPNs" -f $user.UserPrincipalName
        Create-LogEntry -ErrorType Warning -Detail $msg -Source "Script:Add-NewStagedUser" -RemoteSiteID $RemoteSiteID | Add-LogEntry
        return $null
    }

    if (!$siteConfig.SiteDomain.Contains($dom)) {
        $msg = "Not adding {0}, domain not listed in site configuration" -f $user.UserPrincipalName
        Create-LogEntry -ErrorType Warning -Detail $msg -Source "Script:Add-NewStagedUser" -RemoteSiteID $RemoteSiteID | Add-LogEntry
        return $null
    }

    $msg="Staging new AD user $($user.UserPrincipalName)..."
    Create-LogEntry -ErrorType Info -Detail $msg -Source "Script:Add-NewStagedUser" -RemoteSiteID $RemoteSiteID | Add-LogEntry

    try {
        $res = @{}
        $res.loadState=0
        $res.localGuid = $user.ObjectGUID
        $res.domainName = $dom
        $res.masterGuid = $null
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
        Create-LogEntry -ErrorType Error -Detail $msg -Source "Script:Add-NewStagedUser" -RemoteSiteID $RemoteSiteID | Add-LogEntry
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