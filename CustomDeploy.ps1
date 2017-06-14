$startTime=Get-Date
Import-Module Azure -ErrorAction SilentlyContinue

#DEPLOYMENT OPTIONS
    #optional, defines uniqueness for deployments
    $TestNo                  = "1"

    #The name of the Resource Group where all of these resources will be deployed
    $RGName                  = "ComplexOrgPOC$TestNo"

    #The "name" of your web application
    $SiteName                = "ComplexOrgAdmin$TestNo"

    $CustomSTSDisplayName    = "Complex Org Login"

    #The display name of your Azure AD administrative auth app. This name is displayed when a user logs in to your app from Azure AD
    $AdminAppName            = "Complex Org Administration$TestNo"

    #region to deploy into - see https://azure.microsoft.com/en-us/regions/
    $DeployRegion            = "West US 2"

    #The name of the Azure AD tenant that will host this app
    $TenantName              = "[your AAD Tenant name, like contoso.com or contoso.onmicrosoft.com]"

    #The GUID of that tenant
    $AADTenantId             = "[AAD TenantID for app hosting]"

    #Azure AD Application Client ID
    $ClientId                = $null

    #App secret for that app
    $ClientSecret            = $null
#END DEPLOYMENT OPTIONS

#Dot-sourced variable override (optional, comment out if not using)
. C:\dev\A_CustomDeploySettings\ComplexOrgSettings.ps1

$newApps = $false;

if ($ClientId -eq $null) {
    #generating a unique "secret" for your admin app to execute operations on your behalf
	$bytes = New-Object Byte[] 32
	$rand = [System.Security.Cryptography.RandomNumberGenerator]::Create()
	$rand.GetBytes($bytes)
	$rand.Dispose()
	$spAdminPassword = [System.Convert]::ToBase64String($bytes)

    #A unique URI that defines your application
    $AdminAppUri             = "https://$($SiteName).$TenantName"

    $adminApp = Get-AzureRmADApplication -DisplayNameStartWith $AdminAppName -ErrorAction Stop
    if ($adminApp -eq $null) {
        #generate required AzureAD applications
        #note: setting loopback on apps for now - will update after the ARM deployment is complete (below)...
        $adminApp = New-AzureRmADApplication -DisplayName $AdminAppName -HomePage "https://loopback" -IdentifierUris $AdminAppUri
            New-AzureRmADServicePrincipal -ApplicationId $adminApp.ApplicationId
        $newApps = $true
    }

    if ($newApps) {
        Start-Sleep 15
    }

    $adminAppCred = Get-AzureRmADAppCredential -ApplicationId $adminApp.ApplicationId
    if ($adminAppCred -eq $null) {
        New-AzureRmADAppCredential -ApplicationId $adminApp.ApplicationId -Password $spAdminPassword
    }
    $ClientId = $adminApp.ApplicationId
    $ClientSecret = $spAdminPassword
}

#deploy
$parms=@{
    "hostingPlanName"             = $SiteName;
    "skuName"                     = "F1";
    "skuCapacity"                 = 1;
    "CustomSTSDisplayName"        = $CustomSTSDisplayName;
    "tenantName"                  = $TenantName;
    "tenantId"                    = $AADTenantId;
    "clientId"                    = $ClientId;
    "clientSecret"                = $ClientSecret;
    "AdminRedisSKUName"           = "Basic";
    "AdminRedisSKUFamily"         = "C";
    "AdminRedisSKUCapacity"       = 0;
}

#$TemplateFile = "https://raw.githubusercontent.com/Azure-Samples/active-directory-dotnet-web-ad-complex-org-utility/master/azuredeploy.json"
$TemplateFile = "C:\Dev\ComplexOrg\active-directory-dotnet-web-ad-complex-org-utility\azuredeploy.json"

try {
    Get-AzureRmResourceGroup -Name $RGName -ErrorAction Stop
    Write-Host "Resource group $RGName exists, updating deployment"
}
catch {
    $RG = New-AzureRmResourceGroup -Name $RGName -Location $DeployRegion
    Write-Host "Created new resource group $RGName."
}
$version ++
$deployment = New-AzureRmResourceGroupDeployment -ResourceGroupName $RGName -TemplateParameterObject $parms -TemplateFile $TemplateFile -Name "ComplexOrgDeploy$version"  -Force -Verbose

if ($deployment) {
    #to-do: update URIs and reply URLs for apps, based on output parms from $deployment
    #also to-do: update application permissions and APIs - may need to be done in the portal
    $adminHostName = "https://$($deployment.Outputs.adminSiteObject.Value.defaultHostName.ToString())/"
    if ($newApps) {
        $url = "https://$adminHostName/"
        $adminApp.ReplyUrls.Add($url)
        #todo: update app reply urls
    }
    $stsHostName = "https://$($deployment.Outputs.stsSiteObject.Value.defaultHostName.ToString())/sts"

    $ProjectFolder = "$env:USERPROFILE\desktop\$RGName\"
    md $ProjectFolder -ErrorAction Ignore

    $WshShell = New-Object -comObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut("$($ProjectFolder)ComplexOrgAdmin.lnk")
    $Shortcut.TargetPath = "$adminHostName"
    $Shortcut.IconLocation = "%ProgramFiles%\Internet Explorer\iexplore.exe, 0"
    $Shortcut.Save()

    $WshShell = New-Object -comObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut("$($ProjectFolder)ComplexOrgSTS.lnk")
    $Shortcut.TargetPath = "$stsHostName"
    $Shortcut.IconLocation = "%ProgramFiles%\Internet Explorer\iexplore.exe, 0"
    $Shortcut.Save()

    start $ProjectFolder
}

$endTime=Get-Date

Write-Host ""
Write-Host "Total Deployment time:"
New-TimeSpan -Start $startTime -End $endTime | Select Hours, Minutes, Seconds
