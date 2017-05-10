using ADSync.Common.Enums;
using ADSync.Common.Models;
using ADSync.Data;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Common
{
    public static class SiteUtils
    {
        public static IEnumerable<string> GetSiteDomainList(IIdentity user)
        {
            string list = user.GetClaim(CustomClaimTypes.SiteDomain);
            return list.Split(',').ToList();
        }

        public static async Task<RemoteSite> AuthorizeApiAsync(string apiKey)
        {
            try
            {
                if (apiKey == null)
                {
                    return null;
                }

                RemoteSite site = null;
                site = await SiteCache.GetSiteByApiKey(HttpRuntime.Cache, apiKey.ToString());

                if (site == null)
                {
                    return null;
                }

                //we're authenticated
                return site;
            }
            catch (Exception ex)
            {
                throw new Exception("Error authorizing access", ex);
            }
        }
        public static string GetIPAddress(NameValueCollection serverVars)
        {
            string ipAddress = serverVars["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return serverVars["REMOTE_ADDR"];
        }
    }
}