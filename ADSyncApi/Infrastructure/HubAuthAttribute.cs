using ADSync.Common.Enums;
using ADSync.Common.Models;
using Common;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Security.Claims;
using Microsoft.AspNet.SignalR.Owin;
using System.Collections.Generic;

namespace Infrastructure
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class HubAuth : AuthorizeAttribute
    {
        public override bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
        {
            var connectionId = hubIncomingInvokerContext.Hub.Context.ConnectionId;
            var environment = hubIncomingInvokerContext.Hub.Context.Request.Environment;
            var principal = environment["server.User"] as ClaimsPrincipal;
            if (principal != null && principal.Identity != null && principal.Identity.IsAuthenticated)
            { 
                hubIncomingInvokerContext.Hub.Context = new HubCallerContext(new ServerRequest(environment), connectionId);
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        {
            IPrincipal p = null;

            try
            {
                var key = request.Headers.SingleOrDefault(h => h.Key == "apikey").Value;
               
                if (key == Settings.STSApiKey)
                {
                    //branch just for STS calls
                    if (!AuthAndAddClaims(new RemoteSite
                    {
                        ApiKey = key,
                        Id = key,
                        OnPremDomainName = "CustomSTS",
                        SiteType = SiteTypes.LocalADOnly
                    }, ref p, request))
                    {
                        return false;
                    }
                    request.Environment["server.User"] = p;
                    request.GetHttpContext().User = p;
                    return true;
                }

                RemoteSite site = null;
                var task = Task.Run(async () =>
                {
                    site = await SiteUtils.AuthorizeApiAsync(key);
                });
                task.Wait();

                if (site == null)
                {
                    return false;
                }

                //we're authenticated
                if (!AuthAndAddClaims(site, ref p, request))
                {
                    return false;
                }

                request.Environment["server.User"] = p;
                request.GetHttpContext().User = p;
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error authorizing access", ex);
            }
        }

        private static bool AuthAndAddClaims(RemoteSite site, ref IPrincipal principal, IRequest request)
        {
            try
            {
                string domainList = string.Join(",", site.SiteDomains.ToArray());

                List<Claim> claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, site.Id),
                    new Claim(CustomClaimTypes.SiteId, site.Id),
                    new Claim(CustomClaimTypes.OnPremDomainName, site.OnPremDomainName),
                    new Claim(CustomClaimTypes.SiteDomain, domainList),
                    new Claim(CustomClaimTypes.IsHQ, (site.SiteType == SiteTypes.MasterHQ).ToString()),
                    new Claim(CustomClaimTypes.IsSTS, (site.ApiKey == Settings.STSApiKey).ToString())
                };

                ClaimsIdentity identity = new ClaimsIdentity(claims, CustomAuthTypes.Api);

                principal = new ClaimsPrincipal(new[] { identity });
                return true;
            }
            catch (Exception ex)
            {
                Logging.WriteToAppLog("Auth Error", System.Diagnostics.EventLogEntryType.Error, ex);
                return false;
            }
        }
    }
}
