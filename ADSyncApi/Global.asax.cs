using ADSyncApi.Infrastructure;
using Common;
using Portal.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ADSyncApi
{
    public class MvcApplication : System.Web.HttpApplication
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
                DocDBRepo.Settings.DocDBCollection = ConfigurationManager.AppSettings["DocDBCollection"];

                Settings.StorageConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];

                //Zip init
                ZipCopy.InitZip(Settings.AppRootPath);

                var client = DocDBRepo.Initialize().Result;
            }
            catch (Exception ex)
            {
                Logging.WriteToAppLog("Error during site initialization", System.Diagnostics.EventLogEntryType.Error, ex);
                throw;
            }
        }
    }
}
