## Documentation/Help


__Application Diagram:__

![Diagram]

There are four main components of the solution:
* A C#/MVC administrative portal, backed by CosmosDB, running a webjob and hosting a SignalR hub and client
* A SignalR agent, installed as a Windows Service at each site, with line of sight to that location's Active Directory
* A custom Security Token Service (STS) with a SignalR agent
* A set of PowerShell scripts set to run as a Windows Scheduled Job at each site *

__Administrative Portal__

![SiteDashboard]
![UserGeneral]
![UserContact]
![UserAdvanced]
![SiteLogs]

Each location can configure multiple on-prem domain names corresponding to each of the UPN suffixes in use. Each site is also
configured with an "API Key" that's used for service authentication both for script calls and for access to the SignalR hub (below).

![SiteSettings]

__PowerShell Scripts__

PowerShell scripts are used to facilitate a rudimentary sync process between the remote locations and HQ. There are 
3 groups of script types:
* MasterHQ - these scripts are run at HQ and manage creation of shadow accounts and updating the central database with new ObjectGUIDs
* LocalADOnly - run at a remote site with no existing AAD tenant
* AADB2B - run at an affiliate site that also already has it's own Azure AD

"LocalADOnly" users are created at HQ in an OU that is included in Azure AD Connect's sync process. Users from "AADB2B" 
affiliates are created at HQ in an OU that is excluded from the Connect sync. By creating both types of users at HQ,
the HQ accounts are available for use by Azure AD Application Proxy, allowing these external users to authenticate
to on-premises applications using their home identies.

__SignalR Site Agent__

The SignalR agent deploys in each location and installs as a Windows service. This service consists of two components: 
the SignalR client, and a scheduler service that kicks off the PowerShell scripts.

The SignalR hub runs in the context of the Administrative Portal. Each site connects to the hub, authenticating with a
custom API key that's also used when authenticating for WebAPI calls from the scripts. The hub and
SignalR infrastructure serve two functions. 

* Enabling real-time management of on-prem AD users from the central portal. The portal enables updating
properties of the user account including enabling and disabling the account, and performing an 
administrative reset of the account's password.

  ![UserOnPremManage]
  ![UserAdminPWReset]

* The second function of the SignalR hub is to facilitate pass-through authentication of users.
The process for this configuration is
  * The AD domain of the remote site is configured in the HQ AAD tenant
  * Synchronize the users from the remote site to the HQ site
  * Those users are created in the HQ site with smart-card access required and with a complex random password
  * The ObjectGUID from these new AD accounts is copied back to the remote site via the administrative portal database
  * These remote users are updated, setting the msds_consistencyGUID to the HQ ObjectGUID value
  * The HQ accounts that were created are sync'd to the AAD tenant using the regular AAD Connect sync tool
  * Powershell is used to configure federation for the remote site domain in the tenant. This federation points to 
  * the custom STS (below)

The agent service is available to download from the administrative portal. The link is unique for each configured site, and 
on download, the configuration files are customized with that location's API keys and URLs before being bundled into a 
ZIP file and downloaded.

![SiteSetupDownload]

__Custom STS__

The custom STS runs as a stand-alone web application. It leverages the .Net Framework Windows Identity Foundation SDK to 
enable it to perform as a custom STS. It is designed to be configured as the federation endpoint for one or more
secondary domains configured in the HQ AAD tenant. Each domain's federation config requires a unique IssuerUri, so the 
STS is designed to generate a different IssuerUri in the SAML token that's returned back to AAD, depending on the login
identity of the user. The format of the IssuerUri is configurable in the web.config - the default is:
"http&#58;//[site FQDN]/sts/[AAD domain name for this location]/services/trust"



[Diagram]: ../DocImages/Diagram.png "Application Diagram"
[SiteDashboard]: ../DocImages/SiteDashboard.png "Site Dashboard"
[SiteLogs]: ../DocImages/SiteLogs.png "Site Logs"
[SiteSettings]: ../DocImages/SiteSettings.png "Site Settings"
[SiteSetupDownload]: ../DocImages/SiteSetupDownload.png "Site Setup Download"
[UserAdminPWReset]: ../DocImages/UserAdminPWReset.png "User Admin PW Reset"
[UserAdvanced]: ../DocImages/UserAdvanced.png "User Advanced"
[UserContact]: ../DocImages/UserContact.png "User Contact"
[UserGeneral]: ../DocImages/UserGeneral.png "User General"
[UserOnPremManage]: ../DocImages/UserOnPremManage.png "User On Prem Manage"
