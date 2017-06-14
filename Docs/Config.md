
__Configuration__

Log in. You will see the dashboard along with links along the top nav.

![alt text][Dash]

Click "Sites". On the top right you'll see "+ New" - click it.

![alt text][AddSite]

Fill out the form. "Site Domains" should contain all of the UPN suffixes from your on-prem AD:

![alt text][UPNSuffix]

* "On-premises Domain Name" is the primary UPN suffix in use at this location and will be used for identification purposes.

* "Site Type" includes 4 options:
  * MasterHQ - the HQ site that is running AAD Connect and synchronizing with an AAD tenant
  * AADB2B - an affiliate site that has it's own AAD tenant - these users will be added as B2B guests
  * LocalADOnly - an affiliate that doesn't have it's own AAD tenant - these users will be synchronized as users in the HQ tenant
  * AADB2BCloudOnly - an affiliate this ONLY has an AAD tenant with no on-prem hybrid identity (not implemented in code yet)

* "B2B Default Redirect URL" - for B2B affiliates, this is the URL a user will be redirected to after redeeming an invitation

Click "Save" and you will be returned to the Affiliate Configuration screen.

Click on the site you just created, and click on the "Setup" tab.

![alt text][ServiceDL]

Click to download the service. The application will automatically generate a custom ZIP file with the site's API key (created automatically when you created the site).

Log into a Windows server at the site you just configured. This server should be a member server in the on-prem AD you specified in the site configuration.

Copy the ZIP file you just downloaded to the server, and extract it. NOTE: you should extract it to the permanent location where the service files will be located - the installer doesn't create and copy these files to Program Files.

![alt text][ServiceFiles]

As an administrator, run "ComplexOrgSiteSetup.exe".

![alt text][ServiceInstaller]

The app will automatically login to the WebAPI using the API key assigned to this site, and you will see the correct domains listed.

Enter domain admin login credentials under "Local Domain Settings", and click "Login to AD". The app will authenticate to your local AD and retrieve
the list of UPN suffixes in the domain. If all of the names in the remote domain list are present in the Local UPN Suffix List, the configuration
will be confirmed and the "Go to Service Config >>" button will be enabled.

![alt text][ServiceInstaller2]

Click that button to view the "Service Config" tab.

![alt text][ServiceInstaller3]

Click to re-use the credentials from the previous step, or enter new credentials. This is the account that the service will be installed to run as.

Click "Install". The service will be created on this server, and started.

Within a few minutes, users from your on-prem AD should be copied up to the database and visible in the portal. 

To confirm the service is communicating successfully with the SignalR backplane, click on one of the users, and click "Manage". The admin portal will
call down to the service agent via the SignalR hub, and retrieve the status of the user in the local AD.

![alt text][ManageOnPrem]

Assuming the site you just created was "LocalADOnly", these users will remain in a waiting state until a site of the type "MasterHQ" is created. Follow the above steps to create that site, and the users should be picked up and created on-prem.

The MasterHQ PowerShell scripts are pre-set to create OUs in your on-prem directory. These settings may be fine for a lab environment or POC but you should edit the PS to ensure that the names align with your naming strategy.

![alt text][HQOUs]

![alt text][InitOUs]

In the example, the "RemoteUsers-B2B" OU will need to be EXCLUDED from the Azure AD Connect synchronization. These are shadow accounts that
will be used by Azure Active Directory Application Proxy - B2B users will authenticate to the app proxy agent using their AAD credentials, and those
credentials will be used by the on-prem agent to authenticate the user to the on-prem resource using Kerberos Constrained Delegation (KDC). Since these
B2B users are already present in this tenant as B2B guest, we don't want the AAD Connect tool to try and sync them to the tenant again.

![alt text][OUFilter]



[Dash]: ../DocImages/SiteDashboard.png "Site Dashboard"
[AddSite]: ../DocImages/AddSite.png "Add New Site"
[ServiceDL]: ../DocImages/SiteSetupDownload.png "Site Service Download"
[UPNSuffix]: ../DocImages/UPNSuffixesOnPrem.png "Manage On-prem UPN Suffixes"
[ServiceFiles]: ../DocImages/ServiceFiles.png "Relay Service Files"
[ServiceInstaller]: ../DocImages/ServiceInstaller.png "Relay Service Installer"
[ServiceInstaller2]: ../DocImages/ServiceInstaller2.png "Service Confirmed"
[ServiceInstaller3]: ../DocImages/ServiceInstaller3.png "Install Service"
[ManageOnPrem]: ../DocImages/UserOnPremManage.png "Manage On-prem user"
[InitOUs]: ../DocImages/InitHQ.png "PS script that init's HQ settings"
[HQOUs]: ../DocImages/HQOUs.png "HQ OUs in Users and Computers"
[OUFilter]: ../DocImages/ConnectOUFilter.png "AAD Connect - OU filter"
