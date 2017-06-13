using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Common
{
    public static class Settings
    {
        public static string AppRootPath = HttpContext.Current.Server.MapPath("//");
        public static string StorageConnectionString { get; set; }
        public static string STSApiKey { get; set; }
        public static string AdminApiKey { get; set; }
        public static string ClientId { get; set; }
        public static string ClientSecret { get; set; }
        public static string TenantId { get; set; }
        public static string AdminSiteUrl { get; set; }
        public static string CurrSiteScriptVersion { get; set; }
        public static EnvType Environment { get; set; }

        public static SiteMode SiteMode { get; set; }

        public static DateTime StartTimeUtc { get; set; }

        public const string Redir403 = "~/Home/Unauthorized";

        public const string Redir404 = "~/Home/NotFound";

        public const string Redir500 = "~/Home/Error";

        public static void Setup(NameValueCollection settings)
        {
            Environment = (EnvType)Enum.Parse(typeof(EnvType), settings["Environment"]);
        }
    }
    public enum SiteMode
    {
        Production,
        Maintenance
    }
    public enum EnvType
    {
        Dev,
        Int,
        QA,
        UAT,
        Prod
    }
}
