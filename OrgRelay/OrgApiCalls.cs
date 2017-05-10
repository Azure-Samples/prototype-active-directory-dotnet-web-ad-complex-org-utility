using ADSync.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OrgRelay
{
    public static class OrgApiCalls
    {
        public static string ApiKey { get; set; }
        public static string SiteUrl { get; set; }
        public static string RemoteSiteId { get; set; }

        public static RemoteSite GetSiteConfig()
        {
            Uri uri = new Uri(string.Format("{0}api/StagedUsers/GetSiteConfig", SiteUrl));
            WebClient web;
            using (web = new WebClient())
            {
                ConfigWeb(ref web);
                var data = web.DownloadString(uri);
                var res = JsonConvert.DeserializeObject<RemoteSite>(data);
                RemoteSiteId = res.Id;
                return res;
            }
        }

        public static void AddSyncLog(SyncLogEntry log)
        {
            log.RemoteSiteID = RemoteSiteId;

            Uri uri = new Uri(string.Format("{0}/api/SyncLogUpdate/AddLogEntry", SiteUrl));
            var data = JsonConvert.SerializeObject(log);
            WebClient web;
            using (web = new WebClient())
            {
                ConfigWeb(ref web);
                web.UploadString(uri, data);
            }
        }
        public static void ConfigWeb(ref WebClient web)
        {
            web.Headers.Add("apikey", ApiKey);
            web.Headers.Add("Content-Type", "application/json");
        }
    }
}
