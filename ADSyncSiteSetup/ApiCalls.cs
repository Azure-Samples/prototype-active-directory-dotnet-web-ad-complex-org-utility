using ADSync.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ADSyncSiteSetup
{
    public class ApiCalls: IDisposable
    {
        private string _apiKey;
        private string _siteUrl;
        private WebClient _web;

        public ApiCalls(string ApiKey, string SiteUrl)
        {
            _apiKey = ApiKey;
            _siteUrl = SiteUrl;
            _web = new WebClient();
            _web.BaseAddress = _siteUrl;
            _web.Headers.Add("apikey", _apiKey);
        }

        public RemoteSite GetSiteConfig()
        {
            Uri uri = new Uri(string.Format("{0}/api/StagedUsers/GetSiteConfig", _siteUrl));
            var data = _web.DownloadString(uri);
            var res = JsonConvert.DeserializeObject<RemoteSite>(data);
            return res;
        }

        public void Dispose()
        {
            _web.Dispose();
            _web = null;
        }
    }
}
