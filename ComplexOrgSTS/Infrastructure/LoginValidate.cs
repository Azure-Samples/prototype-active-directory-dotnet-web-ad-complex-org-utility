using ADSync.Common.Models;
using ADSync.Data;
using Common;
using ComplexOrgSTS.Models;
using OrgRelay;
using System;
using System.Threading.Tasks;
using System.Web.Caching;

namespace ComplexOrgSTS.Infrastructure
{
    public static class LoginValidate
    {
        public static async Task<ValidationResponse> ValidateAsync(LoginModel login, Cache cache)
        {
            try
            {
                var client = new SigRClient(Settings.AdminSiteUrl, Settings.STSApiKey, "SiteHub");

                var domainName = login.UserName.Split('@')[1].ToLower();

                var site = await SiteCache.GetSiteByDomain(cache, domainName);

                var cred = new STSCredential
                {
                    Domain = site.OnPremDomainName,
                    UserName = login.UserName,
                    Password = login.Password,
                    RemoteSiteId = site.Id
                };

                await client.StartAsync();
                ValidationResponse res = await client.ProcessSTSValidationRequest(cred);

                return res;
            }
            catch (Exception ex)
            {
                Utils.AddLogEntry("Error during user validation", System.Diagnostics.EventLogEntryType.Error, 0, ex);
                return new ValidationResponse
                {
                    IsValid = false
                };
            }
        }
    }
}