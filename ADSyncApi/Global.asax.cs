using ADSyncApi.Infrastructure;
using Common;
using Portal.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ADSyncApi
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AntiForgeryConfig.UniqueClaimTypeIdentifier = "http://schemas.microsoft.com/identity/claims/objectidentifier";

            try
            {
                DocDBRepo.Settings.DocDBUri = ConfigurationManager.AppSettings["DocDBUri"];
                DocDBRepo.Settings.DocDBAuthKey = ConfigurationManager.AppSettings["DocDBAuthKey"];
                DocDBRepo.Settings.DocDBName = ConfigurationManager.AppSettings["DocDBName"];

                Settings.StorageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
                Settings.STSApiKey = ConfigurationManager.AppSettings["STSApiKey"];
                Settings.AdminSiteUrl = ConfigurationManager.AppSettings["AdminSiteUrl"];
                Settings.AdminApiKey = ConfigurationManager.AppSettings["AdminApiKey"];

                Settings.ClientId = ConfigurationManager.AppSettings["ida:ClientId"];
                Settings.ClientSecret = ConfigurationManager.AppSettings["ida:ClientSecret"];
                Settings.TenantId = ConfigurationManager.AppSettings["ida:TenantId"];

                //Zip init
                ZipCopy.InitZip(Settings.AppRootPath);
                Settings.CurrSiteScriptVersion = ZipCopy.GetCurrSiteVersion(Path.Combine(Settings.AppRootPath, "Files"));

                var client = DocDBRepo.Initialize().Result;
            }
            catch (Exception ex)
            {
                Logging.WriteToAppLog("Error during site initialization", System.Diagnostics.EventLogEntryType.Error, ex);
                throw;
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError();
            Utils.AddLogEntry("Global error caught", System.Diagnostics.EventLogEntryType.Error, 0, ex);
        }
    }
}
