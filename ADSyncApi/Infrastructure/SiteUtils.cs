using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
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
    }
}