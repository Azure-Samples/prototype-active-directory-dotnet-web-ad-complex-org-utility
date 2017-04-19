using ADSync.Common.Enums;
using ADSync.Common.Models;
using Common;
using Portal.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ADSync.Data.Models
{
    public static class RemoteSiteUtil
    {
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