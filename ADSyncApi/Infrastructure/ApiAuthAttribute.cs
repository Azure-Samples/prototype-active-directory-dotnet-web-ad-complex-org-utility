using ADSync.Data.Models;
using Common;
using Portal.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Mvc;

namespace Infrastructure
{
    public class ApiAuth: System.Web.Http.AuthorizeAttribute
    {
        private bool _isAdmin;
        public ApiAuth(string isAdmin="")
        {
            _isAdmin = (isAdmin == "Admin");
        }
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            base.HandleUnauthorizedRequest(actionContext);
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            try
            {
                var key = actionContext.Request.Headers.SingleOrDefault(h => h.Key=="apikey").Value.FirstOrDefault();
                
                if (key == null)
                {
                    Unauthorized(actionContext);
                    return;
                }

                RemoteSite site = null;
                var task = Task.Run(async () => {
                    site = await SiteCache.GetSiteByApiKey(HttpRuntime.Cache, key.ToString());
                });
                task.Wait();

                if (site == null)
                {
                    Unauthorized(actionContext);
                    return;
                }
                if (_isAdmin && site.SiteType != SiteTypes.MasterHQ)
                {
                    Unauthorized(actionContext);
                    return;
                }

                //we're authenticated
                if (!AuthAndAddClaims(site, actionContext))
                {
                    Unauthorized(actionContext);
                    return;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error authorizing access", ex);
            }
        }

        private static void Unauthorized(HttpActionContext actionContext)
        {
            actionContext.Response = actionContext.Request.CreateResponse(System.Net.HttpStatusCode.Forbidden);
            actionContext.Response.Headers.Add("AuthenticationStatus", "NotAuthorized");
            actionContext.Response.ReasonPhrase = "ApiKey is invalid.";
        }

        private static bool AuthAndAddClaims(RemoteSite site, HttpActionContext context)
        {
            try
            {
                string domainList = string.Join(",", site.SiteDomains.ToArray());
                List<Claim> claims = new List<Claim>();
                claims.Add(new Claim(CustomClaimTypes.SiteId, site.Id));
                claims.Add(new Claim(CustomClaimTypes.SiteDomain, domainList));

                // create an identity with the valid claims.
                ClaimsIdentity identity = new ClaimsIdentity(claims, CustomAuthTypes.Api);

                // set the context principal.
                context.RequestContext.Principal = new ClaimsPrincipal(new[] { identity });
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
