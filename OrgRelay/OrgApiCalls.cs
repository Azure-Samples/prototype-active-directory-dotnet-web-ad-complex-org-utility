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
            WebClient2 web;
            try
            {
                using (web = new WebClient2())
                {
                    ConfigWeb(ref web);
                    var data = web.DownloadString(uri);
                    var res = JsonConvert.DeserializeObject<RemoteSite>(data);
                    RemoteSiteId = res.Id;
                    return res;
                }
            }
            catch (WebException ex)
            {

                throw;
            }
        }

        public static void AddSyncLog(SyncLogEntry log)
        {
            log.RemoteSiteID = RemoteSiteId;

            Uri uri = new Uri(string.Format("{0}api/SyncLogUpdate/AddLogEntry", SiteUrl));
            var data = JsonConvert.SerializeObject(log);
            WebClient2 web;
            using (web = new WebClient2())
            {
                ConfigWeb(ref web);
                web.UploadString(uri, data);
            }
        }
        public static void ConfigWeb(ref WebClient2 web)
        {
            web.Headers.Add("apikey", ApiKey);
            web.Headers.Add("Content-Type", "application/json");
        }
    }
}
