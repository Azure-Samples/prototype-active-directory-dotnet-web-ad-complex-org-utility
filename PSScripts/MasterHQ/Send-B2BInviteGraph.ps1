function Send-B2BInviteGraph {
    param (
        [Parameter( Position = 0, Mandatory = $true)]
        [string]$Email,

        [Parameter( Position = 1, Mandatory = $true)]
        [string]$DisplayName,

        [Parameter( Position = 2, Mandatory = $true)]
        [string]$RedirectTo,

        [Parameter( Position = 3, Mandatory = $true)]
        [ValidateSet('Guest','Member')]
        [string]$UserType,

        [Parameter( Position = 4, Mandatory = $true)]
        [string]$GraphToken
    )

    $endPoint = "https://graph.microsoft.com/v1.0/invitations"
    $Headers = @{Authorization = "Bearer "+$GraphToken}

    $invitation = @{
        InvitedUserDisplayName = $DisplayName;
        InvitedUserEmailAddress = $Email;
        InviteRedirectUrl = $RedirectTo;
        SendInvitationMessage = $true;
        InvitedUserType = $UserType;
    }
    $Body = ConvertTo-Json $invitation

    $res = Invoke-WebRequest -Uri $endPoint -Method Post -Headers $Headers -Body $Body
    return ConvertFrom-Json $res.Content
}

function Get-AADUsersGraph {
    param (
        [Parameter( Position = 4, Mandatory = $true)]
        [string]$GraphToken
    )

    $endPoint = "https://graph.microsoft.com/beta/users?`$select=id,displayName,mail,userPrincipalName,source,userType,altSecID"
    $Headers = @{Authorization = "Bearer "+$GraphToken}

    $res = Invoke-WebRequest -Uri $endPoint -Method Get -Headers $Headers
    return (ConvertFrom-Json $res.Content).value
}