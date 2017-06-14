## Deployment Steps

An Azure Active Directory app must be created in your tenant for login to the admin portal.

* Log into the Azure portal, and click on Azure Active Directory, then click on Properties

  ![alt text][App1a]

* Click to copy the "Directory ID". This is also referred to as a "Tenant Id". Save this string, you'll need it in a bit.
* Click on App registrations

  ![alt text][App1]

* Click "+ New application registration" and enter the name of your app (like "Complex Org Admin"). This title will be seen when users are prompted for their credentials.
* Select "Web app / API", and enter the Sign-on URL. If you're setting this up before you deploy the app to Azure, you can enter https://loopback as a placeholder. Click "Create".

  ![alt text][App2]

* From the application list, find the app you just created and click to open and edit it
* Click on "Required permissions", then click "+ Add". On "Select an API", click and select "Microsoft Graph"
* Click "Select permissions". On the "Enable Access" panel that appears, check the following items:
  * APPLICATION PERMISSIONS
    * Read and write directory data
    * Read and write all users' full profiles
  * DELEGATED PERMISSIONS
    * Sign in and read user profile
* Click "Select"


<a target="_blank" href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Factive-directory-dotnet-web-ad-complex-org-utility%2Fmaster%2Fazuredeploy.json"><img src="http://azuredeploy.net/deploybutton.png"/></a>


[App1]: ../DocImages/App1.png "Open Azure AD Application Panel"
[App1a]: ../DocImages/App1a.png "Copy tenant id"
[App2]: ../DocImages/App2.png "Create Application"
[App3a]: ../DocImages/App3a.png "Grant permissions"
[App4]: ../DocImages/App4.png "Generate app secret"
[App5]: ../DocImages/App5.png "Copy app id"
[Url]: ../DocImages/Url.png "Copy web URL"
[Url2]: ../DocImages/Url2.png "Update reply address"
[Url3]: ../DocImages/Url3.png "Update home page"
