using ComplexOrgSTS.Infrastructure;
using Portal.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ComplexOrgSTS
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Settings.SiteName = ConfigurationManager.AppSettings["SiteName"];
            Settings.SigningCertificate = ConfigurationManager.AppSettings["SigningCertificate"];
            Settings.SigningCertificatePassword = ConfigurationManager.AppSettings["SigningCertificatePassword"];
            Settings.Port = ConfigurationManager.AppSettings["Port"];
            Settings.HttpLocalhost = ConfigurationManager.AppSettings["HttpLocalhost"];
            Settings.STSApiKey = ConfigurationManager.AppSettings["STSApiKey"];
            Settings.AdminSiteUrl = ConfigurationManager.AppSettings["AdminSiteUrl"];
            Settings.IssuerUri = ConfigurationManager.AppSettings["IssuerUri"];

            DocDBRepo.Settings.DocDBUri = ConfigurationManager.AppSettings["DocDBUri"];
            DocDBRepo.Settings.DocDBAuthKey = ConfigurationManager.AppSettings["DocDBAuthKey"];
            DocDBRepo.Settings.DocDBName = ConfigurationManager.AppSettings["DocDBName"];
            var client = DocDBRepo.Initialize().Result;
        }
    }
}
