using System;
using System.Linq;
using System.Web.Caching;
using System.Collections.Generic;
using System.Threading.Tasks;
using ADSync.Common.Models;
using ADSync.Data.Models;

namespace ADSync.Data
{
    public static class SiteCache
    {
        public static async Task<RemoteSite> GetSiteByApiKey(Cache cache, string apiKey)
        {
            var sites = (IEnumerable<RemoteSite>)cache["RemoteSites"];
            if (sites != null)
            {
                var res = sites.SingleOrDefault(s => s.ApiKey == apiKey);
                if (res != null)
                {
                    return res;
                }
                else
                {
                    //sanity check in case the site was just added
                    sites = await refreshCache(cache);
                    return sites.SingleOrDefault(s => s.ApiKey == apiKey);
                }
            }
            else
            {
                sites = await refreshCache(cache);
                return sites.SingleOrDefault(s => s.ApiKey == apiKey);
            }
        }
        public static async Task<RemoteSite> GetSiteByDomain(Cache cache, string domainName)
        {
            var sites = (IEnumerable<RemoteSite>)cache["RemoteSites"];
            if (sites != null)
            {
                var res = sites.SingleOrDefault(s => s.SiteDomains.Any(d => d.ToLower() == domainName));
                if (res != null)
                {
                    return res;
                }
                else
                {
                    //sanity check in case the site was just added
                    sites = await refreshCache(cache);
                    return sites.SingleOrDefault(s => s.SiteDomains.Any(d => d == domainName));
                }
            }
            else
            {
                sites = await refreshCache(cache);
                return sites.SingleOrDefault(s => s.SiteDomains.Any(d => d == domainName));
            }
        }
        private static async Task<IEnumerable<RemoteSite>> refreshCache(Cache cache)
        {
            var sites = await RemoteSiteUtil.GetAllSites();
            cache.Add("RemoteSites", sites, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration, CacheItemPriority.BelowNormal, null);
            return sites;
        }
    }
}
