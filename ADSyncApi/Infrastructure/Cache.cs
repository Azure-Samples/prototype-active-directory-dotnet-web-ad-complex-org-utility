using System;
using System.Linq;
using System.Web;
using ADSync.Data.Models;
using System.Web.Caching;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure
{
    public static class SiteCache
    {
        public static async Task<RemoteSite> GetSiteByApiKey(Cache cache, string apiKey)
        {
            var sites = (IEnumerable<RemoteSite>)cache["RemoteSites"];
            if (sites != null)
            {
                var res = sites.SingleOrDefault(s => s.ApiKey == apiKey);
                if (res!=null)
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
        private static async Task<IEnumerable<RemoteSite>> refreshCache(Cache cache)
        {
            var sites = await RemoteSite.GetAllSites();
            cache.Add("RemoteSites", sites, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration, CacheItemPriority.BelowNormal, null);
            return sites;
        }
    }
}
