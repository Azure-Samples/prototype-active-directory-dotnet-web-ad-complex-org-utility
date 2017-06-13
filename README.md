# Azure Active Directory/ASP.Net MVC Complex Organization Utility
## Sample/Prototype utility facilitating automated access to a headquarters Azure AD and on-premises resources from affiliated companies
## Quick Start

<a target="_blank" href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Factive-directory-dotnet-web-ad-complex-org-utility%2Fmaster%2Fazuredeploy.json"><img src="http://azuredeploy.net/deploybutton.png"/></a>

[Documentation](./Docs/Help.md)

[Detailed step-by-step deployment instructions](./Docs/Setup.md)

__Details__
* Creates a cloud-based management portal and database for staging of user accounts from one or more on-premises Active Directory forests, for creation in a master AD
  * The Azure AD administrator establishes a verified domain name in the master Azure AD, one for each remote site's UPN suffixes
  * "Master" accounts are created and then synchronized to Azure Active Directory via the Azure Active Directory Connect
  * The "immutable id" that is synchronized with Azure AD is then updated back in the staging database
  * The originating AD picks this record up and records the immutable id in it's local AD, using the "mS-DS-ConsistencyGuid"
  * Federation is then manually configured (via PowerShell) for this remote AD to the master Azure AD tenant, federating with associated domain previously established by HQ. Azure Active Directory Connect is NOT configured at these remote sites.
  * This federation is enabled on behalf of all remote sites via a central custom STS. This STS communicates with each remote site via a SignalR backplane running on the administrative portal. This real-time socket connection facilitates pass-through authentication directly from the STS to the appropriate on-prem AD.
  * The result of this orchestration is a central location to manage and authorize user identities across a distributed network of affiliated companies. Users credentials are managed in their local Active Directory, but they maintain cloud access for SaaS applications via the headquarters Azure AD
  * Additionally, the accounts created in the on-premises headquarters AD enable affiliate users to access on-premises applications hosted locally at HQ, and authorized against the HQ AD, by leveraging Azure Application Proxy
* Leverages Azure Cosmos DB. For development, a downloadable emulator is available: https://aka.ms/documentdb-emulator
* ARM template deploys the following:
  * Azure Web App
  * Azure Cosmos DB
* Requires the following (see step-by-step deployment instructions above for details):
  1. Azure AD application with the following:
     * Sign-in permissions
  2. Optional - custom DNS name and SSL cert

# As-Is Code

This code is made available as a sample to demonstrate a potential strategy for managing and integrating multiple disconnected directories with Azure Active Directory. It should be customized by your dev team or a partner, and should be reviewed before being deployed in a production scenario.

# Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
