using ADSync.Common.Enums;
using ADSync.Common.Models;
using ADSync.Data.Models;
using ADSyncApi.Infrastructure;
using Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ADSyncApi.Controllers.Api
{
    [Authorize]
    public class RemoteSiteApiController : ApiController
    {
        [HttpGet]
        public async Task<RemoteSiteRes> GetSite(string id)
        {
            var site = await RemoteSiteUtil.GetSite(id);
            var res = new RemoteSiteRes(site);
            return res;
        }

        [HttpGet]
        public async Task<IEnumerable<RemoteSite>> GetAllSites()
        {
            var res = await RemoteSiteUtil.GetAllSites();
            return res;
        }

        [HttpGet]
        public async Task<string> GetSiteScriptVersion(string id)
        {
            var response = await AdminRelayClient.GetSiteScriptVersion(id);
            return response.Data;
        }

        [HttpPost]
        public async Task<RemoteSite> UpdateSite(RemoteSiteModel site)
        {
            site.SiteDomains = site.SiteDomainsList.Split(new string[] { "\n" }, StringSplitOptions.None).ToList();

            if (site.ResetApiKey)
            {
                site.ApiKey = Utils.GenApiKey();
            }
            var site2 = JsonConvert.DeserializeObject<RemoteSite>(JsonConvert.SerializeObject(site));
            var res = await RemoteSiteUtil.UpdateSite(site2);
            return res.Single(s => s.Id == site.Id);
        }
    }
    public class RemoteSiteRes
    {
        public RemoteSite Site { get; set; }
        public Dictionary<int,string> SiteTypes { get; set; }
        public RemoteSiteRes(RemoteSite site)
        {
            Site = site;
            SiteTypes = Enum.GetValues(typeof(SiteTypes))
                           .Cast<SiteTypes>()
                           .ToDictionary(t => (int)t, t => t.ToString());

        }

    }
}
