using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Common
{
    public static class CustomClaimTypes
    {
        public const string SiteId = "SiteId";
        public const string SiteDomain = "SiteDomain";
        public const string IdentityProvider = "http://schemas.microsoft.com/identity/claims/identityprovider";
        public const string ExtClaims = "ExtClaims";
        public const string AuthType = "AuthType";
        public const string IsHQ = "IsHQ";
        public const string IsSTS = "IsSTS";
        public const string FullName = "FullName";
        public const string ObjectIdentifier = "http://schemas.microsoft.com/identity/claims/objectidentifier";
    }
    public static class CustomAuthTypes
    {
        public const string B2E = "OpenIdConnect-B2E";
        public const string B2B = "OpenIdConnect-B2B";
        public const string B2EMulti = "OpenIdConnect-Multi";
        public const string B2C = "OpenIdConnect-B2C";
        public const string Api = "ApiAuth";
    }
}
