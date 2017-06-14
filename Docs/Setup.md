## Resources Deployed

The following Azure resources are deployed by this ARM template:
* App Hosting Plan 
  * https://docs.microsoft.com/en-us/azure/app-service/azure-web-sites-web-hosting-plans-in-depth-overview?toc=%2fazure%2fapp-service-web%2ftoc.json
* Cosmos DB 
  * https://docs.microsoft.com/en-us/azure/cosmos-db/
* Redis Cache 
  * https://docs.microsoft.com/en-us/azure/redis-cache/
* Azure Storage (blob and queues are used) 
  * https://docs.microsoft.com/en-us/azure/storage/
* Admin Web App
  * https://docs.microsoft.com/en-us/azure/app-service-web/
* Custom STS Web App


## Deployment Steps

An Azure Active Directory app must be created in your tenant for login to the admin portal.

* Log into the Azure portal, and click on Azure Active Directory, then click on Properties

  ![alt text][App1a]

* Click to copy the "Directory ID". This is also referred to as a "Tenant Id". Save this string, you'll need it in a bit. Also make note of the Name.
* Click on App registrations

  ![alt text][App1]

* Click "+ New application registration" and enter the name of your app (like "Complex Org Admin"). This title will be seen when users are prompted for their credentials.
* Select "Web app / API", and enter the Sign-on URL. If you're setting this up before you deploy the app to Azure, you can enter https://loopback as a placeholder. Click "Create".

  ![alt text][App2]

* From the application list, find the app you just created and click to open and edit it
* Click "Keys". Under Description, enter a name for the application key, like "Key 1". Under Expires, select 1 or 2 years. (NOTE: you or someone in your organization 
will need to make a note to come back and refresh this key before it expires.)
* Click "Save". An application secret will be generated and displayed. COPY this key and record it - you'll need it in an minute when setting up the web application. 
NOTE: this key will not be displayed again and cannot be retrieved. If you lose it, you'll have to come back, delete it, and create another one.

  ![alt text][App4]

* Finally, record the "Application ID". You can click to the right of it in the main panel and it will copy it to your clipboard. Record it along with the app secret from above - these two strings will be needed to setup the web app.

 __Web Application Setup__

At this point, you should have 4 items saved: the tenant ID and name, and the app ID and secret. You can now click the "Deploy to Azure" button and it will take you to Azure (right-click and open in a new window so you can continue to follow along here):

<a target="_blank" href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Factive-directory-dotnet-web-ad-complex-org-utility%2Fmaster%2Fazuredeploy.json"><img src="http://azuredeploy.net/deploybutton.png"/></a>

Log in, and you'll see a form like this one:

  ![alt text][App6]

  * Enter the name of your new Resource Group. This is where the app resources (admin and STS apps, DocumentDB, storage account, etc.) will be deployed.
  * Select a region to deploy into.
  * Enter the Hosting Plan Name. This is the name of the compute resources that will power your web apps. This name will be used throughout the deployment as part of the web app and DocumentDB names as well.
  * SKU refers to the size and price of your hosting plan. See https://azure.microsoft.com/en-us/pricing/details/app-service/ for details.
  * SKU capacity refers to the number of compute instances deployed in your farm. NOTE: this code is currently tested and optimized for a single deployment. A Redis Cache should be implemented as a shared state engine for a farm deployment.
  * Tenant Name: this is the name of the Azure Active Directory tenant where you deployed your application, like "contoso.onmicrosoft.com", or "contoso.com".
  * Tenant Id: Paste the TenantID you copied earlier.
  * Client Id: Paste the ApplicationID from your admin app.
  * Client Secret: Paste the application secret from your admin app.
  * Redis settings: the defaults are for a minimal managed Redis Cache deployment. See https://azure.microsoft.com/en-us/pricing/details/cache/ for details.

That's it. Click "Purchase" (there's no charge for this software - you are agreeing to pay for the Azure compute resources you are about to provision). 
Within 5-10 minutes, the deployment will complete and your application will be ready.

__Cleanup__

Now that the app is deployed, there's a final step. We need to go back into our Azure Active Directory app and update the URLs to match the URL of our admin app.

  * Navigate to your new web application - it's under "App Services". Click to copy the URL.

    ![alt text][Url]
  
  * Edit each of your Azure AD apps. First, update the Home page URL:

    ![alt text][Url3]
  
  * Now edit the reply URL:

    ![alt text][Url2]
  
All done!



[App1]: ../DocImages/App1.png "Open Azure AD Application Panel"
[App1a]: ../DocImages/App1a.png "Copy tenant id"
[App2]: ../DocImages/App2.png "Create Application"
[App3a]: ../DocImages/App3a.png "Grant permissions"
[App4]: ../DocImages/App4.png "Generate app secret"
[App5]: ../DocImages/App5.png "Copy app id"
[App6]: ../DocImages/ARMForm.png "ARM template form"
[Url]: ../DocImages/Url.png "Copy web URL"
[Url2]: ../DocImages/Url2.png "Update reply address"
[Url3]: ../DocImages/Url3.png "Update home page"
