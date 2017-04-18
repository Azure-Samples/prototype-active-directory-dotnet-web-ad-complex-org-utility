using Common;
using Newtonsoft.Json;
using Portal.Data;
using Portal.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ADSync.Data.Models
{
    public class RemoteSite: DocModelBase, IDocModelBase
    {
        [JsonProperty(PropertyName = "siteDomain")]
        [DisplayName("Site Domains")]
        public List<string> SiteDomains { get; set; }

        [JsonProperty(PropertyName = "siteType")]
        [DisplayName("Site Type")]
        public SiteTypes SiteType { get; set; }

        [JsonProperty(PropertyName = "apiKey")]
        [DisplayName("API Key")]
        public string ApiKey { get; set; }

        public RemoteSite()
        {
            SiteDomains = new List<string>();
        }
        public static async Task<RemoteSite> GetSite(string id)
        {
            var res = await DocDBRepo.DB<RemoteSite>.GetItemAsync(id);
            return res;
        }

        public static async Task<RemoteSite> GetSiteByDomain(string siteDomain)
        {
            var res = await DocDBRepo.DB<RemoteSite>.GetItemsAsync(s => s.SiteDomains.Any(d => d == siteDomain));
            return res.SingleOrDefault();
        }

        public static async Task<RemoteSite> GetSiteByApiKey(string apiKey)
        {
            var res = await DocDBRepo.DB<RemoteSite>.GetItemsAsync(s => s.ApiKey == apiKey);
            return res.SingleOrDefault();
        }

        public static async Task<IEnumerable<RemoteSite>> GetRemoteSites()
        {
            var res = await DocDBRepo.DB<RemoteSite>.GetItemsAsync(s => s.SiteType != SiteTypes.MasterHQ);
            return res;
        }
        public static async Task<IEnumerable<RemoteSite>> GetAllSites()
        {
            var res = await DocDBRepo.DB<RemoteSite>.GetItemsAsync();
            return res;
        }

        public static async Task<IEnumerable<RemoteSite>> DeleteSite(string id)
        {
            var res = await DocDBRepo.DB<RemoteSite>.DeleteItemAsync(new RemoteSite { Id = id });
            return await GetAllSites();
        }

        public static async Task<IEnumerable<RemoteSite>> UpdateSite(RemoteSite site)
        {
            var res = await DocDBRepo.DB<RemoteSite>.UpdateItemAsync(site);
            return await GetAllSites();
        }

        public static async Task<IEnumerable<RemoteSite>> AddSite(RemoteSite site)
        {
            site.ApiKey = Utils.GenApiKey();
            var res = await DocDBRepo.DB<RemoteSite>.CreateItemAsync(site);
            return await GetAllSites();
        }
    }
}