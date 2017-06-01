## Documentation/Help


__Application Diagram:__
![Diagram]

There are four main components of the solution:
* A C#/MVC administrative portal, backed by CosmosDB, running a webjob and hosting a SignalR hub and client
* A SignalR agent, installed as a Windows Service at each site, with line of sight to that site's Active Directory
* A custom Security Token Service (STS) with a SignalR agent
* A set of PowerShell scripts set to run as a Windows Scheduled Job at each site *

__Administrative Portal__
![SiteDashboard]
![UserGeneral]
![UserContact]
![UserAdvanced]
![UserOnPremManage]
![UserAdminPWReset]
![SiteLogs]
![SiteSetupDownload]
![SiteSettings]

__SignalR Site Agent__


__Custom STS__


__Powershell Scripts__



[Diagram]: ./DocImages/Diagram.png "Application Diagram"
[SiteDashboard]: ./DocImages/SiteDashboard.png "Site Dashboard"
[SiteLogs]: ./DocImages/SiteLogs.png "Site Logs"
[SiteSettings]: ./DocImages/SiteSettings.png "Site Settings"
[SiteSetupDownload]: ./DocImages/SiteSetupDownload.png "Site Setup Download"
[UserAdminPWReset]: ./DocImages/UserAdminPWReset.png "User Admin PW Reset"
[UserAdvanced]: ./DocImages/UserAdvanced.png "User Advanced"
[UserContact]: ./DocImages/UserContact.png "User Contact"
[UserGeneral]: ./DocImages/UserGeneral.png "User General"
[UserOnPremManage]: ./DocImages/UserOnPremManage.png "User On Prem Manage"
